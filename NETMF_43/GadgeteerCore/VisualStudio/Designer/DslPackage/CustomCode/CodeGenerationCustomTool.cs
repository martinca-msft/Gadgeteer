// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Designer.Interfaces;
using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using System;
using System.Linq;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Gadgeteer.Designer.Definitions;

namespace Microsoft.Gadgeteer.Designer
{
    [Guid("C9EC42CC-D634-4665-8123-889F7F468D14")]
    public class CodeGenerationCustomTool : BaseCodeGeneratorWithSite
    {
        /// <summary>
        /// Generates and returns the generated output
        /// </summary>
        /// <param name="inputFileName">The name of the file the custom tool is being run against</param>
        /// <param name="inputFileContent">The contents of the file the custom tool is being run against</param>
        protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
        {
            GadgeteerModel model = LoadGadgeteerModel(inputFileName);

            if (model == null)
            {
                OnError("Unable to load model: {0}", inputFileName);
                return null;
            }

            string userCodeFileName = Path.ChangeExtension(inputFileName, CodeProvider.FileExtension);
            model.GenerateUsingsInUserCode(GlobalServiceProvider, userCodeFileName);

            CodeCompileUnit code = new CodeCompileUnit();
            CodeTypeDeclaration programClass;
            CodeStatementCollection mainStatements;
            CodeStatementCollection initializeModuleStatements;
            CodeMemberProperty mainboard;

            GenerateFileHeader(inputFileName, code, out programClass, out mainStatements, out initializeModuleStatements, out mainboard);

            GenerateMainMethod(mainStatements, model);
            GenerateModulesDeclaration(programClass, model);
            GenerateModulesInitialization(initializeModuleStatements, model);
            GenerateMainboardProperty(mainboard, model);

            if (model.Store != null)
                model.Store.Dispose();

            using (MemoryStream codeStream = new MemoryStream())
            {
                IndentedTextWriter codeWriter = new IndentedTextWriter(new StreamWriter(codeStream));

                try
                {
                    CodeProvider.GenerateCodeFromCompileUnit(code, codeWriter, null);
                }
                catch (Exception compileException)
                {
                    OnError("Code generation failed: {0}", compileException.Message);
                    return new byte[0];
                }

                codeWriter.Flush();
                return codeStream.ToArray();
            }
        }

        protected GadgeteerModel LoadGadgeteerModel(string fileName)
        {
            string tempFileName = null;
            Transaction loadTransaction = null;

            try
            {
                tempFileName = CreateTempFileIfDirty(fileName);

                Store store = new Store(GlobalServiceProvider, typeof(GadgeteerDSLDomainModel));
                loadTransaction = store.TransactionManager.BeginTransaction("Load", true);

                GadgeteerModel model = GadgeteerDSLSerializationHelper.Instance.LoadModel(store, tempFileName ?? fileName, null, null, null);

                loadTransaction.Commit();
                return model;
            }
            finally
            {
                if (tempFileName != null)
                    File.Delete(tempFileName);

                if (loadTransaction != null)
                    loadTransaction.Dispose();
            }
        }
        private string CreateTempFileIfDirty(string fileName)
        {
            var dd = GetDocData(fileName);
            Log.WriteWarningIf(dd == null, "Could not find DocData for {0}. Was the editor closing?", fileName);

            //If this gets called when the docdata is dirty, save to a temp file and use that for 
            //codegen                    
            if (dd != null && dd.IsDocDirty)
            {
                //generate a file in the temp directory
                string tempFile = Path.Combine(Path.GetTempPath(), Path.GetFileName(fileName));
                if (0 == dd.Save(tempFile, 0 /*don't retain this as the file name*/, 0/*unused*/))
                {
                    //The Save above turns off the dirty bit, but we need to turn it back on
                    //because the document has only been saved to a temp location
                    dd.SetDocDataDirty(1);

                    return tempFile;
                }
            }

            return null;
        }

        private void GenerateFileHeader(string inputFileName, CodeCompileUnit code, out CodeTypeDeclaration programClass, out CodeStatementCollection mainStatements, out CodeStatementCollection initializeModulesStatements, out CodeMemberProperty mainboard)
        {
            string className = Path.GetFileNameWithoutExtension(inputFileName);
            string classNamespace = GetDefaultNamespace(inputFileName);

            programClass = new CodeTypeDeclaration(className);
            programClass.IsClass = true;
            programClass.IsPartial = true;
            programClass.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            programClass.BaseTypes.Add("Gadgeteer.Program");

            CodeMemberMethod main = new CodeMemberMethod();
            main.Comments.Add(new CodeCommentStatement("<summary>This method runs automatically when the device is powered, and calls ProgramStarted.</summary>", true));
            main.Name = "Main";
            main.Attributes = MemberAttributes.Public | MemberAttributes.Static | MemberAttributes.Final;
            mainStatements = main.Statements;

            CodeMemberMethod initializeModules = new CodeMemberMethod();
            initializeModules.Name = "InitializeModules";
            initializeModules.Attributes = MemberAttributes.Private | MemberAttributes.Final;
            initializeModulesStatements = initializeModules.Statements;

            mainboard = new CodeMemberProperty();
            mainboard.Comments.Add(new CodeCommentStatement("<summary>This property provides access to the Mainboard API. This is normally not necessary for an end user program.</summary>", true));
            mainboard.Name = "Mainboard";
            mainboard.Attributes = MemberAttributes.Family | MemberAttributes.Static | MemberAttributes.New;

            programClass.Members.Add(main);
            programClass.Members.Add(initializeModules);
            programClass.Members.Add(mainboard);

            CodeNamespace programNamespace = new CodeNamespace(classNamespace);
            programNamespace.Imports.Add(new CodeNamespaceImport("Gadgeteer"));
            programNamespace.Imports.Add(new CodeNamespaceImport(Module.GadgeteerModuleRootNamespaceAlias + " = " + Module.GadgeteerModuleRootNamespace));
            programNamespace.Types.Add(programClass);

            code.Namespaces.Add(programNamespace);
        }
        private void GenerateMainMethod(CodeStatementCollection mainStatements, GadgeteerModel model)
        {
            mainStatements.Add(new CodeCommentStatement("Important to initialize the Mainboard first"));

            if (model.Mainboard == null)
            {
                GenerateIssueStatements(new GadgeteerModel.Issue(GadgeteerModel.IssueLevel.Error, "No mainboard is defined. Please add a mainboard in the Gadgeteer Designer."), mainStatements);
            }
            else
            {
                CodePropertyReferenceExpression mainboardReference = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("Program"), "Mainboard");

                mainStatements.Add(new CodeAssignStatement(
                    mainboardReference,
                    new CodeObjectCreateExpression(model.Mainboard.MainboardDefinitionTypeName)));
            }

            mainStatements.Add(new CodeVariableDeclarationStatement("Program", "p", new CodeObjectCreateExpression("Program")));
            mainStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("p"), "InitializeModules"));
            mainStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("p"), "ProgramStarted"));
            mainStatements.Add(new CodeCommentStatement("Starts Dispatcher"));
            mainStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("p"), "Run"));
        }
        private void GenerateModulesDeclaration(CodeTypeDeclaration programClass, GadgeteerModel model)
        {
            foreach (Module module in model.Modules)
            {
                CodeMemberField memberField = new CodeMemberField(module.ModuleType, module.Name);
                memberField.UserData["WithEvents"] = true;

                string connectionsText = "(not connected)";
                if (module.SocketUses.Count < 1)
                {
                    connectionsText = "(no sockets)";
                }
                else if (module.Connected && module.SocketUses.Any(u => u.Socket != null))
                {
                    var connections = from use in module.SocketUses
                                      where use.Socket != null
                                      group use by use.Socket.GadgeteerHardware into byHardware
                                      select new { Hardware = byHardware.Key, Sockets = byHardware.Select(u => u.Socket), Count = byHardware.Count() };


                    connectionsText = "using " + string.Join(" and ", from connection in connections
                                                                      select string.Format("socket{0} {1}{2} of {3}",
                                                                           connection.Count > 1 ? "s" : "",
                                                                           connection.Count > 1 ? string.Join(", ", connection.Sockets.Select(s => s.Label).Take(connection.Count - 1)) + " and " : "",
                                                                           connection.Sockets.Last().Label,
                                                                           connection.Hardware is Module ? ((Module)connection.Hardware).Name : "the mainboard"));

                }

                memberField.Comments.Add(new CodeCommentStatement(string.Format("<summary>The {0} module {1}.</summary>", module.GadgeteerPartDefinition.Name, connectionsText), true));
                programClass.Members.Add(memberField);
            }
        }
        private void GenerateModulesInitialization(CodeStatementCollection initializeModuleStatements, GadgeteerModel model)
        {
            List<string> instantiatedModules = new List<string>();

            IEnumerable<GadgeteerModel.Issue> issues;
            var modules = model.SortModulesInCodeGenerationOrder(out issues);

            foreach (var issue in issues)
            {
                GenerateIssueStatements(issue, initializeModuleStatements);
            }

            foreach (Module module in modules)
                if (module.Connected)
                {
                    string missingName = module.FindFirstMissingModuleName(instantiatedModules);
                    if (missingName == null)
                    {
                        CodeFieldReferenceExpression moduleReference = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), module.Name);

                        CodeAssignStatement statement = new CodeAssignStatement(
                            moduleReference,
                            new CodeObjectCreateExpression(module.AliasedTypeName, module.GenerateConstructorParameters()));

                        initializeModuleStatements.Add(statement);
                        instantiatedModules.Add(module.Name);
                    }
                    else
                    {
                        GenerateIssueStatements(new GadgeteerModel.Issue(GadgeteerModel.IssueLevel.Warning, "The module '{0}' requires the module '{1}' to be connected, so it will be null.", module.Name, missingName), initializeModuleStatements);
                    }
                }
                else
                {
                    //Socket dependentSocket = module.Sockets.FirstOrDefault(s => s.SocketUse != null && s.SocketUse.Module != null);

                    //if (dependentSocket != null)
                    //    GenerateIssueStatements(new GadgeteerModel.Issue(EventLevel.Error, "The {0} was not connected in the designer, and is required by {1}.", module.Name, dependentSocket.SocketUse.Module.Name), initializeModuleStatements);
                    //else
                        GenerateIssueStatements(new GadgeteerModel.Issue(GadgeteerModel.IssueLevel.Warning, "The module '{0}' was not connected in the designer and will be null.", module.Name), initializeModuleStatements);
                }
        }

        private void GenerateMainboardProperty(CodeMemberProperty mainboard, GadgeteerModel model)
        {
            CodePropertyReferenceExpression baseMainboardReference = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("Gadgeteer.Program"), "Mainboard");
            CodeTypeReference mainboardTypeReference = new CodeTypeReference("Gadgeteer.Mainboard");

            if (model.Mainboard != null)
                mainboardTypeReference = new CodeTypeReference(model.Mainboard.MainboardDefinitionTypeName);

            mainboard.GetStatements.Add(
                new CodeMethodReturnStatement(
                    new CodeCastExpression(mainboardTypeReference, baseMainboardReference)));

            mainboard.SetStatements.Add(
                new CodeAssignStatement(
                    baseMainboardReference, new CodeVariableReferenceExpression("value")));

            mainboard.Type = mainboardTypeReference;
        }

        private void GenerateIssueStatements(GadgeteerModel.Issue issue, CodeStatementCollection statements)
        {
            switch (issue.Level)
            {
                case GadgeteerModel.IssueLevel.Critical:
                    if (CodeProvider.FileExtension == "cs")
                        statements.Add(new CodeSnippetStatement("#error " + issue.Message));
                    else
                        statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression("System.Exception", new CodePrimitiveExpression(issue.Message))));
                    break;

                case GadgeteerModel.IssueLevel.Error:
                    statements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression("System.Exception", new CodePrimitiveExpression(issue.Message))));
                    break;

                default:
                    statements.Add(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("Microsoft.SPOT.Debug"), "Print", new CodePrimitiveExpression(issue.Message)));
                    break;
            }
        }

        /// <summary>
        /// Finds the associated DocData for the given file using the running document table service. The file needs to be opened for this to work.
        /// </summary>
        private GadgeteerDSLDocData GetDocData(string fileName)
        {
            var rdt = this.GlobalServiceProvider.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
            if (rdt == null)
                return null;

            IVsHierarchy pHier;
            uint itemId, cookie;
            IntPtr pDocData = IntPtr.Zero;

            try
            {
                if (VSConstants.S_OK != rdt.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, fileName, out pHier, out itemId, out pDocData, out cookie))
                    return null;

                return Marshal.GetObjectForIUnknown(pDocData) as GadgeteerDSLDocData;
            }
            finally
            {
                if (pDocData != IntPtr.Zero)
                    Marshal.Release(pDocData);
            }
        }

        public override string GetDefaultExtension()
        {
            return ".generated." + CodeProvider.FileExtension;
        }
        protected string GetDefaultNamespace(string fileName)
        {
            string ns = FileNamespace;

            // Visual Basic projects do not seem to pass the root namespace as promised at http://msdn.microsoft.com/en-us/library/bb165757.aspx

            if (string.IsNullOrEmpty(ns))
            {
                var dte = this.GlobalServiceProvider.GetService(typeof(SDTE)) as EnvDTE.DTE;
                if (dte.Solution != null)
                {
                    EnvDTE.ProjectItem item = dte.Solution.FindProjectItem(fileName);
                    if (item != null)
                    {
                        EnvDTE.Property nsProp = item.ContainingProject.Properties.Item("DefaultNamespace");
                        if (nsProp != null)
                            return nsProp.Value as string;
                    }
                }
            }

            return ns;
        }

        protected void OnError(string message, params object[] args)
        {
            GeneratorErrorCallback(false, 0, string.Format(message, args), 0, 0);
            Log.WriteError(message);
        }

        private CodeDomProvider codeDomProvider = null;
        protected CodeDomProvider CodeProvider
        {
            get
            {
                if (codeDomProvider == null)
                {
                    IVSMDCodeDomProvider provider = GetService(typeof(SVSMDCodeDomProvider)) as IVSMDCodeDomProvider;

                    if (provider != null)
                        codeDomProvider = provider.CodeDomProvider as CodeDomProvider;
                    else
                        codeDomProvider = CodeDomProvider.CreateProvider("C#");
                }

                return codeDomProvider;
            }
        }

        protected EnvDTE.Project Project
        {
            get
            {
                var dte = this.Dte;
                if (dte != null)
                {
                    var solution = dte.Solution;
                    if (solution != null)
                    {
                        var item = solution.FindProjectItem(this.InputFilePath);
                        if (item != null)
                            return item.ContainingProject;
                    }
                }

                return null;
            }
        }

        protected string MFVersion
        {
            get
            {
                var project = this.Project;
                if (project != null)
                {
                    var moniker = project.Properties.Item("TargetFrameworkMoniker");
                    if (moniker != null)
                    {
                        var value = moniker.Value as string;
                        return ParseTargetVersion(value);
                    }
                }

                return null;
            }
        }


        private const string VersionMonikerPrefix = "Version=v";
        private static string ParseTargetVersion(string targetMoniker)
        {
            //The moniker looks like this:   ".NETMicroFramework,Version=v4.1"

            if (string.IsNullOrWhiteSpace(targetMoniker))
                return string.Empty;

            int i = targetMoniker.IndexOf(VersionMonikerPrefix);

            if (i < 0)
                return string.Empty;

            return targetMoniker.Substring(i + VersionMonikerPrefix.Length);
        }
    }
}

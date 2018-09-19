// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using VSLangProj;

namespace Microsoft.Gadgeteer.Designer
{
    /// <summary>
    /// Custom tool that inherits from the TemplatedCodeGenerator
    /// class.
    /// </summary>
    /// <remarks>
    /// This class creates a custom tool based on the normal T4
    /// "TextTemplatingFileGenerator". However, this custom tool is 
    /// designed to run directly against the model file, instead of
    /// of against a .tt file. The content of the .tt file is loaded
    /// from an embedded resource instead.
    /// </remarks>
    [System.Runtime.InteropServices.Guid("C9EC42CC-D634-4665-8123-889F7F468D14")]
    public class CodeGenerationCustomTool : TemplatedCodeGenerator
    {   
        /// <summary>
        /// Generates and returns the generated output
        /// </summary>
        /// <param name="inputFileName">The name of the file the custom tool is being run against</param>
        /// <param name="inputFileContent">The contents of the file the custom tool is being run against</param>
        /// <returns></returns>
        protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
        {            
            // The "inputFileContent" parameter the contains the contents of the 
            // file the custom tool is being run against. In this case, this means 
            // the contents of the model file.

            // However, we want to load the text template code from a resource
            // file instead. We need to specify the model file that should be
            // loaded when the template is run, so the text template code contains
            // a marker - we'll replace this marker with the name of the model file.

            const string modelFileNameMarker = "%MODELFILENAME%";
            const string classNameMarker = "%CLASSNAME%";
            const string classFileNameMarker = "%CLASSFILENAME%";
            const string projectNamespaceMarker = "%PROJECTNAMESPACE%";
            
            Tuple<string,string> projectNamespaceAndKind = GetNamespaceAndProjectKind(inputFileName);
            string projectNamespace = projectNamespaceAndKind.Item1;
            string projectKind = projectNamespaceAndKind.Item2;
            
             
            // Load the text template from the embedded resource
            string templateCode = null;
           
            if (PrjKind.prjKindCSharpProject == projectKind)
                templateCode = Encoding.UTF8.GetString(CodeGenerationResource.CustomToolTemplateFile);
            else if (PrjKind.prjKindVBProject == projectKind)
                templateCode = Encoding.UTF8.GetString(CodeGenerationResource.CustomToolTemplateFileVB);
            else
            {
                Log.WriteError("Unsupported project kind: " + projectKind);
                return null;
            }

#if DEBUG
            //This is handy when authoring/debugging templates, but leaving it commented out to avoid creating a confusing situation
            //for somebody else
            //string t4Path = @"F:\tfs\GadgeteerNew\NETMF_42\GadgeteerCore\VisualStudio\Designer\DslPackage\Resources\CustomToolTemplateFile" + (PrjKind.prjKindVBProject == projectKind ? "VB" : "") + ".t4";
            //if (File.Exists(t4Path))
            //    templateCode = File.ReadAllText(t4Path);
#endif

            Log.WriteErrorIf(!templateCode.Contains(modelFileNameMarker), "Error - the template code does not contain the expected model file name marker");

            //The template contains two pseudo-parameters that we need to fill in before processing it: 
            //
            // a) The model file (%MODELFILENAME%). This is the actual .gadgeteer file if the document is saved or a temp file that we write with the current
            // editor contents if the document is dirty. This allows us to generate code for unsaved models.
            // b) The project namespace (%PROJECTNAMESPACE%). We need to pass this in because the template can't figure it out if we are using a temp file because
            // such file is not part of the project            
            string tempFile = null;
            string preProcessedTemplate = null;

            try
            {
                var dd = GetDocData(inputFileName);
                Log.WriteWarningIf(dd == null, "Could not find DocData for {0}. Was the editor closing?", inputFileName);

                //If this gets called when the docdata is dirty, save to a temp file and use that for 
                //codegen                    
                if (dd != null && dd.IsDocDirty)
                {
                    //generate a file in the temp directory
                    tempFile = Path.Combine(Path.GetTempPath(), Path.GetFileName(inputFileName));
                    if (0 == dd.Save(tempFile, 0 /*don't retain this as the file name*/, 0/*unused*/))
                    {
                        //The Save above turns off the dirty bit, but we need to turn it back on
                        //because the document has only been saved to a temp location
                        dd.SetDocDataDirty(1);
                        // Substitute the temp model file name into the template code
                        preProcessedTemplate = templateCode.Replace(modelFileNameMarker, tempFile);
                    }
                }

                if (preProcessedTemplate == null)
                {
                    // Substitute the real model file name into the template code
                    preProcessedTemplate = templateCode.Replace(modelFileNameMarker, inputFileName);
                }
                
                Log.WriteErrorIf(projectNamespace == null, "Could not get the default namespace for {0}", inputFileName);

                //Fill in the namespace and class name
                preProcessedTemplate = preProcessedTemplate.Replace(projectNamespaceMarker, projectNamespace);
                preProcessedTemplate = preProcessedTemplate.Replace(classNameMarker, Path.GetFileNameWithoutExtension(inputFileName));
                preProcessedTemplate = preProcessedTemplate.Replace(classFileNameMarker, Path.Combine(Path.GetDirectoryName(inputFileName),
                                                                                         Path.GetFileNameWithoutExtension(inputFileName)));                                

                // Delegate the rest of the work to the base class.
                // This will run the T4 transformation and return the
                // result.
                return base.GenerateCode(inputFileName, preProcessedTemplate);
            }
            finally
            {
                if (tempFile != null)
                    File.Delete(tempFile);
            }
        }

        /// <summary>
        /// Get the default namespace out of the project hierarchy
        /// </summary>
        private Tuple<string,string> GetNamespaceAndProjectKind(string fileName)
        {
            var dte = this.GlobalServiceProvider.GetService(typeof(SDTE)) as DTE;          
            if(dte.Solution!=null)
            {
                ProjectItem item = dte.Solution.FindProjectItem(fileName);
                if (item != null)
                {                                        
                    Property nsProp = item.ContainingProject.Properties.Item("DefaultNamespace");
                    if (nsProp != null)
                        return new Tuple<string,string>(nsProp.Value as string, item.ContainingProject.Kind);
                }
            }
            return null;
        }

        /// <summary>
        /// Finds the associated DocData for the given file using the running document table service. The file needs to be opened for this to work.
        /// </summary>
        GadgeteerDSLDocData GetDocData(string fileName)
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
    }
    
}

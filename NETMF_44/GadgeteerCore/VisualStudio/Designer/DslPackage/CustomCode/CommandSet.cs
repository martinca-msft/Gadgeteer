// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;
using System.Xml.Xsl;
using System.Xml;

namespace Microsoft.Gadgeteer.Designer
{
    internal partial class GadgeteerDSLCommandSet
    {        
        private const string OpenFileCommand = "File.OpenFile";

        private const string XsltUrlParam = "moreInfoUrl";

        // For more about setting up commands, see http://msdn.microsoft.com/library/dd820681.aspx

        // IDs of my commands, duplicated in Commands.vsct:
        private const int ConnectAllCommand = 0x801;
        private const int GoToCodeCommand   = 0x802;
		private const int DeleteCommand = 0x803;
		private const int HelpCommand = 0x804;
        private const int PowerEstimateCommand = 0x805;

        private static readonly Guid CommandSetId = new Guid("A2736037-EF55-4996-AFD3-CE80BD7DE26F");

        /// <summary>
        /// Initialize the list of menu commands.
        /// DynamicStatusMenuCommands are those that can be made
        /// visible and enabled depending on state and selected object.
        /// 
        /// These commands are labelled and positioned on the menu in Commands.vsct.
        /// </summary>
        /// <returns></returns>
        protected override IList<MenuCommand> GetMenuCommands()
        {                        


            IList<MenuCommand> commands = base.GetMenuCommands();

            commands.Add(new DynamicStatusMenuCommand(
                new EventHandler(OnStatusConnectAll),
                new EventHandler(OnMenuConnectAll),
                new CommandID(CommandSetId,
                    ConnectAllCommand)));

            commands.Add(new DynamicStatusMenuCommand(
                new EventHandler(OnStatusGoToCode),
                new EventHandler(OnMenuGoToCode),
                new CommandID(CommandSetId,
                    GoToCodeCommand)));

			commands.Add(new DynamicStatusMenuCommand(
				new EventHandler(OnStatusDelete),
				new EventHandler(OnMenuDelete),
				new CommandID(CommandSetId, DeleteCommand)));

			commands.Add(new DynamicStatusMenuCommand(
				new EventHandler(OnStatusHelp),
				new EventHandler(OnMenuHelp),
				new CommandID(CommandSetId, HelpCommand)));

            commands.Add(new DynamicStatusMenuCommand(
                new EventHandler(OnStatusPowerEstimate),
                new EventHandler(OnMenuPowerEstimate),
                new CommandID(CommandSetId, PowerEstimateCommand)));

			return commands;
        }

		internal void OnStatusHelp(object sender, EventArgs e)
		{
			var command = sender as MenuCommand;
			if (command == null)
				return;

            command.Visible = false;

            var shape = this.SingleSelection as GadgeteerHardwareShape;
            if (shape == null)
                return;

            command.Visible = true;

            if (!shape.HasHelp.HasValue)
            {
                Tuple<string, string> urls = GetHelpUrls();
                shape.HasHelp = urls!=null && !string.IsNullOrWhiteSpace(urls.Item1);
            }

		    command.Enabled = shape.HasHelp.Value;
		}

		internal void OnMenuHelp(object sender, EventArgs e)
		{
            Tuple<string,string> urls = GetHelpUrls();
            if(urls==null)
                return;

		    string helpUrl = urls.Item1;

            if (!string.IsNullOrWhiteSpace(helpUrl))
            {
                try
                {

                    if (helpUrl.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase)
                        && File.Exists(helpUrl)) //Check if it's an actual file too
                    {
                        //It's a local XML file, so transform it to HTML using the XSL
                        var transform = new XslCompiledTransform(true);
                        string tempFile = Path.Combine(Path.GetTempPath(),
                                                       string.Concat(Path.GetFileNameWithoutExtension(helpUrl), ".html"));

                        //The XSL is an embedded reasource
                        //Don't use two nested usings to avoid CA2202: Do not dispose objects multiple times
                        StringReader sr = null;
                        try
                        {
                            sr = new StringReader(CodeGenerationResource.xmldoc2html);
                            using (var xr = XmlReader.Create(sr))
                            {
                                sr = null;
                                transform.Load(xr);
                            }
                        }
                        finally 
                        {
                            if(sr!=null)
                                sr.Dispose();
                        }
                        

                        var args = new XsltArgumentList();
                        if(!string.IsNullOrWhiteSpace(urls.Item2))
                            args.AddParam(XsltUrlParam, string.Empty, urls.Item2);

                        using(var stream = new FileStream(tempFile, FileMode.Create))
                            transform.Transform(helpUrl, args, stream);

                        helpUrl = tempFile;
                    }

                    var dte = this.ServiceProvider.GetService(typeof (SDTE)) as DTE;

                    if (dte != null)
                        dte.ItemOperations.Navigate(helpUrl);
                }
                catch (IOException ex){Log.WriteError(ex);}
                catch (XmlException ex) { Log.WriteError(ex); }
                catch (XsltException ex) { Log.WriteError(ex); }
                catch (UriFormatException ex) { Log.WriteError(ex); }
                catch (System.Net.WebException ex) { Log.WriteError(ex); }
            }
		}

        internal void OnStatusPowerEstimate(object sender, EventArgs e)
        {
            var command = sender as MenuCommand;
            if (command == null)
                return;
            
            GadgeteerDSLDocView docView = this.CurrentDocView as GadgeteerDSLDocView;
            if (docView == null)
            {
                command.Visible = false;
            }
            else
            {
                command.Visible = true;
                command.Checked = docView.IsPowerOverlayVisible;
                command.Enabled = true; // docView.IsPowerOverlayAvailable;
            }
        }

        internal void OnMenuPowerEstimate(object sender, EventArgs e)
        {
            GadgeteerDSLDocView docView = this.CurrentDocView as GadgeteerDSLDocView;
            if (docView != null)
            {
                docView.IsPowerOverlayVisible ^= true;
            }
        }


        /// <summary>
        /// Retrieves the content to be displayed by help for the selected element (F1). It can be either
        /// a local XML doc file or an external URL
        /// </summary>        
        private Tuple<string,string> GetHelpUrls()
        {
            var docData = this.CurrentDocData as GadgeteerDSLDocData;
            var shape = this.SingleSelection as GadgeteerHardwareShape;
            if (shape != null && docData != null)
            {
                var modelElement = shape.ModelElement as GadgeteerHardware;
                if (modelElement != null)
                {
                    //Look through the references required by this model element for XML help files
                    var references = docData.GetAssemblyReferences(modelElement);
                    foreach (Reference reference in references)
                    {                     
                        string directory = Path.GetDirectoryName(reference.Path);
                        if (!string.IsNullOrWhiteSpace(directory))
                        {
                            string helpFilePath = Path.Combine(directory,
                                                                string.Concat(Path.GetFileNameWithoutExtension(reference.Path), ".xml"));

                            if (File.Exists(helpFilePath))
                            {
                                return new Tuple<string, string>(helpFilePath, modelElement.GadgeteerPartDefinition.HelpUrl);
                            }
                        }
                    }

                    return new Tuple<string, string>(modelElement.GadgeteerPartDefinition.HelpUrl, null);                    
                }
            }
            return null;
        }


		internal void OnStatusDelete(object sender, EventArgs e)
		{
			var command = sender as MenuCommand;
			if (command == null)
				return;
			
			command.Visible = false;
			command.Enabled = false;

			if (this.IsSingleSelection())
			{
				var ssb = this.SingleSelection as SocketShapeBase;
				if (ssb == null)
					return;

				var sb = ssb.ModelElement as SocketBase;
				if (sb == null)
					return;

				command.Visible = true;
				command.Enabled = sb.IsConnected;
			}

		}

        internal void OnMenuDelete(object sender, EventArgs e)
        {
            var command = sender as MenuCommand;
            if (command == null)
                return;

            var store = this.CurrentGadgeteerDSLDocData.Store;

            var ssb = this.SingleSelection as SocketShapeBase;
			if (ssb == null)
			    return;

            var sb = ssb.ModelElement as SocketBase;
            if (sb != null)
            {
                using (var tx = store.TransactionManager.BeginTransaction())
                {
                    sb.Disconnect();
                    tx.Commit();
                }
            }

        }


        internal void OnStatusConnectAll(object sender, EventArgs e)
        {
            var command = sender as MenuCommand;
            if (command == null) return;

            command.Visible = true;

            Store store = this.CurrentGadgeteerDSLDocData.Store;
            if (store == null)
                command.Enabled = false;
            else
                command.Enabled = store.ElementDirectory.FindElements<Module>().Count > 0 &&
                                  store.ElementDirectory.FindElements<Mainboard>().Count > 0;
        }

        internal void OnMenuConnectAll(object sender, EventArgs e)        
        {            
            Store store = this.CurrentGadgeteerDSLDocData.Store;
            if (store == null)
                return;
    
            Mainboard board = store.ElementDirectory.FindElements<Mainboard>().FirstOrDefault();
            if(board==null) return;

            IEnumerable<Module> modules = store.ElementDirectory.FindElements<Module>();

            
            using (var tx = store.TransactionManager.BeginTransaction())
            {
                bool areModulesAlreadyConnected = board.Sockets.Any(s => s.SocketUse != null);

                //First try to move forward using the already existing connections				
                var socketUses = from o in modules
                                 from su in o.SocketUses
                                 select su;

                var sockets = (from o in modules
                               from s in o.Sockets
                               select s).Concat(board.Sockets);

                try
                {
                    bool success = AutoConnect.Solve(sockets, socketUses);
                    string message = string.Empty;

                    //If the above doesn't work, try again from scratch
                    if (!success && areModulesAlreadyConnected)
                    {
                        //Disconnect all sockets and start over 
                        foreach (var socket in board.Sockets)
                            socket.Disconnect();

                        success = AutoConnect.Solve(sockets, socketUses);
                    }

                    //3. Finally try with just the required sockets
                    if (!success)
                    {
                        //Disconnect all sockets and start over 
                        foreach (var socket in board.Sockets)
                            socket.Disconnect();

                        var requiredSocketUses = from su in socketUses
                                                 where su.Optional == false
                                                 select su;

                        success = AutoConnect.Solve(sockets, requiredSocketUses);
                        message = Resources.PackageUI.AutoConnectRequiredSockets;
                    }

                    if (success)
                    {
                        tx.Commit();
                        if (!string.IsNullOrEmpty(message))
                            MessageBox.Show(message, Resources.PackageUI.ErrorDialogTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(Resources.PackageUI.AutoConnectFailed, //message
                                        Resources.PackageUI.ErrorDialogTitle, //caption
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                        tx.Rollback();
                    }
                }
                catch (OperationCanceledException)
                {
                    tx.Rollback();
                }
            }
        }

        internal void OnStatusGoToCode(object sender, EventArgs e)
        {
            var command = sender as MenuCommand;
            if (command == null) return;
            if (this.CurrentGadgeteerDSLDocData == null) return;
            string path = this.CurrentGadgeteerDSLDocData.GetCodeFileName(); 

            //If the designer name is Program.gadgeteer, try to find Program.cs
            command.Enabled = File.Exists(path);
        }

        internal void OnMenuGoToCode(object sender, EventArgs e)
        {
            string path = this.CurrentGadgeteerDSLDocData.GetCodeFileName(); 

            var dte = this.ServiceProvider.GetService(typeof(SDTE)) as DTE;            

            //Use only the file name here. In Express, before the project is saved the Program.cs is
            //in a temporary directory so if we use the full path the OpenFile command fails. 
            if (dte != null) 
                dte.ExecuteCommand(OpenFileCommand, Path.GetFileName(path));
        }
       
	}

    // disabling cut/copy/paste for Gadgeteer designer right click
    internal partial class GadgeteerDSLClipboardCommandSet
    {
        protected override void ProcessOnStatusCutCommand(MenuCommand cmd)
        {
            if (cmd == null) return;
            cmd.Visible = false;
        }

        protected override void ProcessOnStatusCopyCommand(MenuCommand cmd)
        {
            if (cmd == null) return;
            cmd.Visible = false;
        }

        protected override void ProcessOnStatusPasteCommand(MenuCommand cmd)
        {
            if (cmd == null) return;
            cmd.Visible = false;
        }
    }


}

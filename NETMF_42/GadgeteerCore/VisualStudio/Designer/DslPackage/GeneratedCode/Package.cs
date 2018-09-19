﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using VSShellInterop = global::Microsoft.VisualStudio.Shell.Interop;
using VSShell = global::Microsoft.VisualStudio.Shell;
using DslShell = global::Microsoft.VisualStudio.Modeling.Shell;
using DslDesign = global::Microsoft.VisualStudio.Modeling.Design;
using DslModeling = global::Microsoft.VisualStudio.Modeling;
using VSTextTemplatingHost = global::Microsoft.VisualStudio.TextTemplating.VSHost;
using System;
using System.Diagnostics;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
	
namespace Microsoft.Gadgeteer.Designer
{
	/// <summary>
	/// This class implements the VS package that integrates this DSL into Visual Studio.
	/// </summary>
	[VSShell::DefaultRegistryRoot("Software\\Microsoft\\VisualStudio\\10.0")]
	[VSShell::PackageRegistration(RegisterUsing = VSShell::RegistrationMethod.Assembly, UseManagedResourcesOnly = true)]
	[VSShell::ProvideToolWindow(typeof(GadgeteerDSLExplorerToolWindow), MultiInstances = false, Style = VSShell::VsDockStyle.Tabbed, Orientation = VSShell::ToolWindowOrientation.Right, Window = "{3AE79031-E1BC-11D0-8F78-00A0C9110057}")]
	[VSShell::ProvideToolWindowVisibility(typeof(GadgeteerDSLExplorerToolWindow), Constants.GadgeteerDSLEditorFactoryId)]
	[VSShell::ProvideStaticToolboxGroup("@GadgeteerToolboxTab;Microsoft.Gadgeteer.Designer.Dsl.dll", "Microsoft.Gadgeteer.Designer.GadgeteerToolboxTab")]
	[VSShell::ProvideStaticToolboxItem("Microsoft.Gadgeteer.Designer.GadgeteerToolboxTab",
					"@SocketConnectionToolToolboxItem;Microsoft.Gadgeteer.Designer.Dsl.dll", 
					"Microsoft.Gadgeteer.Designer.SocketConnectionToolToolboxItem", 
					"CF_TOOLBOXITEMCONTAINER,CF_TOOLBOXITEMCONTAINER_HASH,CF_TOOLBOXITEMCONTAINER_CONTENTS", 
					"SocketConnectionTool", 
					"@SocketConnectionToolToolboxBitmap;Microsoft.Gadgeteer.Designer.Dsl.dll", 
					0xff00ff)]
	[VSShell::ProvideStaticToolboxItem("Microsoft.Gadgeteer.Designer.GadgeteerToolboxTab",
					"@CommentToolboxItem;Microsoft.Gadgeteer.Designer.Dsl.dll", 
					"Microsoft.Gadgeteer.Designer.CommentToolboxItem", 
					"CF_TOOLBOXITEMCONTAINER,CF_TOOLBOXITEMCONTAINER_HASH,CF_TOOLBOXITEMCONTAINER_CONTENTS", 
					"ConnectCommentF1Keyword", 
					"@CommentToolboxBitmap;Microsoft.Gadgeteer.Designer.Dsl.dll", 
					0xff00ff)]
	[VSShell::ProvideEditorFactory(typeof(GadgeteerDSLEditorFactory), 103, TrustLevel = VSShellInterop::__VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
	[VSShell::ProvideEditorExtension(typeof(GadgeteerDSLEditorFactory), "." + Constants.DesignerFileExtension, 50)]
	[DslShell::ProvideRelatedFile("." + Constants.DesignerFileExtension, Constants.DefaultDiagramExtension,
		ProjectSystem = DslShell::ProvideRelatedFileAttribute.CSharpProjectGuid,
		FileOptions = DslShell::RelatedFileType.FileName)]
	[DslShell::ProvideRelatedFile("." + Constants.DesignerFileExtension, Constants.DefaultDiagramExtension,
		ProjectSystem = DslShell::ProvideRelatedFileAttribute.VisualBasicProjectGuid,
		FileOptions = DslShell::RelatedFileType.FileName)]
	[DslShell::RegisterAsDslToolsEditor]
	[global::System.Runtime.InteropServices.ComVisible(true)]
	[DslShell::ProvideBindingPath]
	[DslShell::ProvideXmlEditorChooserBlockSxSWithXmlEditor(@"GadgeteerDSL", typeof(GadgeteerDSLEditorFactory))]
	internal abstract partial class GadgeteerDSLPackageBase : DslShell::ModelingPackage
	{
		protected global::Microsoft.Gadgeteer.Designer.GadgeteerDSLToolboxHelper toolboxHelper;	
		
		/// <summary>
		/// Initialization method called by the package base class when this package is loaded.
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();

			// Register the editor factory used to create the DSL editor.
			this.RegisterEditorFactory(new GadgeteerDSLEditorFactory(this));
			
			// Initialize the toolbox helper
			toolboxHelper = new global::Microsoft.Gadgeteer.Designer.GadgeteerDSLToolboxHelper(this);

			// Create the command set that handles menu commands provided by this package.
			GadgeteerDSLCommandSet commandSet = new GadgeteerDSLCommandSet(this);
			commandSet.Initialize();
			
			// Create the command set that handles cut/copy/paste commands provided by this package.
			GadgeteerDSLClipboardCommandSet clipboardCommandSet = new GadgeteerDSLClipboardCommandSet(this);
			clipboardCommandSet.Initialize();
			
			// Register the model explorer tool window for this DSL.
			this.AddToolWindow(typeof(GadgeteerDSLExplorerToolWindow));

			// Initialize Extension Registars
			// this is a partial method call
			this.InitializeExtensions();

			// Add dynamic toolbox items
			this.SetupDynamicToolbox();
		}

		/// <summary>
		/// Partial method to initialize ExtensionRegistrars (if any) in the DslPackage
		/// </summary>
		partial void InitializeExtensions();
		
		/// <summary>
		/// Returns any dynamic tool items for the designer
		/// </summary>
		/// <remarks>The default implementation is to return the list of items from the generated toolbox helper.</remarks>
		protected override global::System.Collections.Generic.IList<DslDesign::ModelingToolboxItem> CreateToolboxItems()
		{
			try
			{
				Debug.Assert(toolboxHelper != null, "Toolbox helper is not initialized");
				return toolboxHelper.CreateToolboxItems();
			}
			catch(global::System.Exception e)
			{
				global::System.Diagnostics.Debug.Fail("Exception thrown during toolbox item creation.  This may result in Package Load Failure:\r\n\r\n" + e);
				throw;
			}
		}
		
		
		/// <summary>
		/// Given a toolbox item "unique ID" and a data format identifier, returns the content of
		/// the data format. 
		/// </summary>
		/// <param name="itemId">The unique ToolboxItem to retrieve data for</param>
		/// <param name="format">The desired format of the resulting data</param>
		protected override object GetToolboxItemData(string itemId, DataFormats.Format format)
		{
			Debug.Assert(toolboxHelper != null, "Toolbox helper is not initialized");
		
			// Retrieve the specified ToolboxItem from the DSL
			return toolboxHelper.GetToolboxItemData(itemId, format);
		}
	}

}

//
// Package attributes which may need to change are placed on the partial class below, rather than in the main include file.
//
namespace Microsoft.Gadgeteer.Designer
{
	/// <summary>
	/// Double-derived class to allow easier code customization.
	/// </summary>
	[VSShell::ProvideMenuResource("1000.ctmenu", 7)]
	[VSShell::ProvideToolboxItems(1)]
	[VSTextTemplatingHost::ProvideDirectiveProcessor(typeof(global::Microsoft.Gadgeteer.Designer.GadgeteerDSLDirectiveProcessor), global::Microsoft.Gadgeteer.Designer.GadgeteerDSLDirectiveProcessor.GadgeteerDSLDirectiveProcessorName, "A directive processor that provides access to GadgeteerDSL files")]
	[global::System.Runtime.InteropServices.Guid(Constants.GadgeteerDSLPackageId)]
	//The PLK is now set via the PLK.pkgdef and PLK_VBExpress.pkgdef files since C# Express and VB Express require different keys
	//[VSShell::ProvideLoadKey("Standard", "1.0.0.0", "GadgeteerDSL", "Microsoft", 105)]
	internal sealed partial class GadgeteerDSLPackage : GadgeteerDSLPackageBase
	{
	}
}
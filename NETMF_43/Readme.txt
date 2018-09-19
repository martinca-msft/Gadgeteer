=====================================================================================================

PREREQUISITES

To build GadgeteerCore, you need to have:

Visual Studio 2015
  plus VS SDK and VS Modeling SDK

- Visual Studio 2012
  Visual Studio Express 2012 for Windows Desktop   http://go.microsoft.com/?linkid=9816758  
  Visual Studio 2010                               http://www.microsoft.com/en-us/download/details.aspx?id=12752 (Ultimate trial)
  Visual C# 2010 Express                           http://go.microsoft.com/?linkid=9709939
  Visual Basic 2010 Express                        http://go.microsoft.com/?linkid=9709929
  (these are not prerequisites but if you want it to test, then install it before .NET MF below)

  Full versions of Visual Studio and Service Packs are available internally at \\products\Public\Products\Developers\.

- Visual Studio 2012 updates available through the Extensions Manager (not a prerequisite)
  Visual Studio 2010 SP1                           http://www.microsoft.com/download/en/details.aspx?id=23691 (required for 2010, install this first)

- Microsoft .NET Micro Framework 4.3               http://netmf.codeplex.com/ 

- WiX  (http://wixtoolset.org/)                

=====================================================================================================

DIRECTORY LAYOUT OF THE REPOSITORY

The repository is designed to mirror the CodePlex repository, which means the structure includes spaces for third party modules/mainboards/etc.

NETMF_4X branch 
\ GadgeteerCore
	GadgeteerCore.sln - the main SLN file for Gadgeteer Core (inc installer)
	GadgeteerBuilderTemplates.sln - the SLN file for the Gadgeteer builder templates (inc installer)
	VSDesigner.sln - the SLN for the Visual Studio designer
	\ ReleaseNotes.txt (release notes)
	\ GadgeteerHardware.xsd (schema for Gadgeteer hardware descriptions)
	\ AssemblyInfoGlobal.cs - global assemblyinfo file that should be used to synchronize version numbers across csprojs distributed in GadgeteerCore
	\ Docs\Templates 
		Template csprojs including ModuleTemplate, MainboardTemplate and KitTemplate (shipped in GadgeteerBuilderTemplates.msi)
	\ Installers
		Installer wixprojs including GadgeteerCoreMSI (the core installer) and GadgeteerBuilderTemplates
	\ VisualStudio
		Visual Studio extensions for Gadgeteer including Designer, which includes a Designer Template
	\ Gadgeteer 
		Gadgeteer.csproj and the core library source

\ Modules - every Gadgeteer module has its own space under this directory
	\ ManufacturerName
		\ ModuleName
			\ Software
				See the module template (in GadgeteerBuilderTemplates.msi) for details of structure under here
			\ Hardware 
				\ Altium 
				\ Datasheets

\ Mainboards - similar to modules

\ Kits 
	\ DistributorName - NB a distributor does not have to be the manufacturer of the modules/mainboards
		\ KitName
			\ Software
				See the kit template (in GadgeteerBuilderTemplates.msi) for details of structure under here


\ Projects - for example projects 
	\ CreatorName
		\ ProjectName
			\ Software
				sln file
				\ ProjectName
					Application csproj
			\ Hardware

\ Builds - builds of GadgeteerCore and GadgeteerBuilderTemplates
	\ Release Drops - record of external releases
	\ Development Drops - drops of msis for internal distribution/use

\ Events - for material used in external events
\ Extensions - for developing new functionality, e.g. the Gadgeteer Cloud Portal 

=====================================================================================================

ADDING NEW MODULES/MAINBOARDS/KITS

It is slightly tricky to get the directory structure right when adding a new module/mainboard/kit.  

See http://gadgeteer.codeplex.com/wikipage?title=HOWTO%3a%20Get%20the%20directory%20structure%20right%20with%20new%20modules&referringTitle=Documentation
for some help

=====================================================================================================

END USER PC LOCATIONS 

This is where files end up on the end user's PC.  Some files come from the GadegteerCore installer, some from GadgeteerBuilderTemplates.
NB our installers are only targeting "all users" at the moment, so need elevated privileges to install.

[Program Files]
	\ Microsoft .NET Gadgeteer\
		\ Core
			Release Notes.txt
			File.ico - icon for .gadgeteer files
			\ Assemblies
				\ .NET Micro Framework 4.X
					Gadgeteer.dll/xml/pdb
					\ be and \ le
						Gadgeteer.dll/pdbx/pe
				\ .NET Framework 4.0 
					Microsoft.Gadgeteer.HardwareWizard.dll/pdb

	\ ManufacturerName
		\ Microsoft .NET Gadgeteer\
			\ Mainboards
				\ MainboardName 
					GadgeteerHardware.xml
					\ NETMF 4.X
						ManufacturerName.Gadgeteer.MainboardName.dll/xml/pdb
						\ be and \ le
							ManufacturerName.Gadgeteer.MainboardName.dll/pdbx/pe
					\ Resources
						- Image.jpg
						- CAD Model? (future work)
			\ Modules
				(as for mainboards)

			\ Kits
				\ KitName
					\ Docs
						\ GettingStartedGuide
							htm and image files for getting started guide

[VSXX_ROOT]\Common7\IDE\{Extensions,VCSExpressExtensions,VBExpressExtensions,WDExpressExtensions} 
	\ Microsoft 
		\ Gadgeteer - App templates (for each language and compatible NETMF version)
		\ GadgeteerBT - Mainboard, module, kit templates 
		\ GadgeteerDSL - Designer 

[START MENU]
	\ ManufacturerName
		\ KitName
			GettingStarted.htm shortcut

[REGISTRY HKLM]\SOFTWARE\Microsoft\
	\ .NETMicroFramework\v4.X\AssemblyFoldersEx
		\ Microsoft .NET Gadgeteer
			Assembly location for Gadgeteer core libraries [Program Files]\Microsoft .NET Gadgeteer\Core\Assemblies\.NET Micro Framework 4.X\
		\ GTM.ManufacturerName.ModuleName
			Assembly location for module dll [Program Files]\ManufacturerName\Microsoft .NET Gadgeteer\Modules\ModuleName\NETMF 4.X\
		\ ManufacturerName.Gadgeteer.MainboardName
			Assembly location for mainboard dll [Program Files]\ManufacturerName\Microsoft .NET Gadgeteer\Mainboards\MainboardName\NETMF 4.X\

	\ .NETGadgeteer
		\v2
			\HardwareDefinitionFolders
				\ GTM.ManufacturerName.ModuleName
					Location of module's GadgeteerHardware.xml [Program Files]\ManufacturerName\Microsoft .NET Gadgeteer\Modules\ModuleName\
				\ ManufacturerName.Gadgeteer.MainboardName
					Location of mainboard's GadgeteerHardware.xml [Program Files]\ManufacturerName\Microsoft .NET Gadgeteer\Mainboards\MainboardName\


=====================================================================================================

GADGETEERCORE: NOTES ON DESIGNER

The designer is written using T4 text templates http://msdn.microsoft.com/en-us/library/bb126445.aspx for code generation.
The source TT files are compiled to get CS files. Building shouldn�t require you to check everything out, but sometimes the text template generation gets triggered when building. 
This tries to write all the .cs files under GeneratedCode (for both projects) so they need to be checked out.  Or, use Build | "Transform All T4 Templates" and this will auto-check-out the files.
To test the designer, set DslPackage as a startup project and hit F5 - this launches an experimental instance of VS which has the package installed.

Additional information on the designer can be found in GadgeteerCore\VisualStudio\VSDesignerDevGuide.docx

=====================================================================================================

BUILDING GADGETEER CORE/BUILDER TEMPLATES

This is a little complex :)

We have TWO msi outputs - GadgeteerCore and GadgeteerBuilderTemplates.
We have THREE sln files - GadgeteerCore, GadgeteerBuilderTemplates and VSDesigner (all in the GadgeteerCore directory)

GadgeteerCore.msi is built by the GadgeteerCoreMSI wix project in GadgeteerCore.sln, and relies on:
	- The projects in that SLN file
	- The VSDesigner's InstallerModule.msm file (location specified in Components.wxi) 
	- The version of the DLLs/MSI is controlled using AssemblyInfoGlobal.cs AND version.wxi (keep both in sync)
	- NB the dlls are NOT signed/signable since NETMF dlls are NOT strong named
	- This installer shows a license dialog, configured in ui\CommonDialogs.wxi to be one of two:
		- For prerelease versions (to be shared with manufacturers under NDA), we use an evaluation license
		- For release versions we use Apache 2.0.  

The VSDesigner's InstallerModule MSM project is found in VSDesigner.sln, and relies on:
	- SIGNED dlls for the Dsl, DslPackage and VSModuleManager projects (location specified in MergeModule.wxs)
		- The BuildAndSign.cmd kicks off signing of these projects using https://codesign/
		- You will need to have the three DLLs declared as trusted using "sn -Vr <dll name>" to build DslPackage
        - If using Modeling SDK 2015, you need to manually remove the offending Index= named parameters from Package.cs in order to target VS 2010.
	- The version of the DLLs AND the MSM is controlled using version.txt

GadgeteerBuilderTemplates.msi is built by the BuilderTemplateInstaller wix project in GadgeteerBuilderTemplates.sln, and relies on:
	- The projects in that SLN file
	- A SIGNED dll for the HardwareTemplateWizard project (location specified in Product.wxs)
	- The version of the MSI is controlled using version.wxi (shared with GadgeteerCore.sln)

Copies of installers are placed in $\Builds - follow the pattern there, and often in \\gadgeteer\share\drops or \\gadgeteer\share\releases
Version number schema is documented in GadgeteerCore\AssemblyInfoGlobal.cs

To build and sign the two MSIs (after the previous two DLL signing stages are already done, and the version numbers are all the same), use GadgeteerCore\SignMSIs.cmd.

BUILDING AND TESTING WIZARDS

- update the Components.wxi to not use the cached version
- either use Temporary.snk in the wizard's project properties and sn -Vr it or reset the cache (http://blogs.msdn.com/b/junfeng/archive/2006/03/11/549363.aspx)
- in the AppTemplateCS/VB's AppTemplate.vstemplate, change the public key token of wizard extension

=====================================================================================================

GADGETEERCORE: KNOWN ISSUES

-- Pathname length problem with VS templates
The VS templates live at [ProgramFilesFolder]Microsoft Visual Studio 11.0\Common7\IDE\{VCSExpress}Extensions\[Manufacturer]\Gadgeteer\ProjectTemplates\CSharp\Gadgeteer\1033\*.zip 
Unfortunately these paths+filenames turns out to be TOO LONG for VS to parse (bug in VS?).  So instead of the source filename e.g. GadgeteerSimpleGraphicsAppTemplate.zip, the installer uses short target names, e.g. SG.zip
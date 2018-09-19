=====================================================================================================

PREREQUISITES

To build GadgeteerCore, you need to have Visual Studio 2010 and the following packages.
- Visual C# 2010 Express and Visual Basic 2010 Express (this is not a prerequisite but if you want it to test, then install it before .NET MF below) http://www.microsoft.com/visualstudio/en-us/products/2010-editions/visual-csharp-express
- Visual Studio 2010 SP1 http://www.microsoft.com/download/en/details.aspx?id=23691 (install this first)
- Microsoft .NET Micro Framework 4.2 
- Visual Studio 2010 SP1 SDK http://www.microsoft.com/downloads/en/details.aspx?FamilyID=21307c23-f0ff-4ef2-a0a4-dca54ddb1e21 (NB this used to be the Visual Studio 2010 SDK - there's a new one for SP1)
- Microsoft Visual Studio 2010 Visualization & Modeling SDK http://www.microsoft.com/downloads/en/details.aspx?FamilyID=0def949d-2933-49c3-ac50-e884e0ff08a7
- WiX v3.5 http://wix.codeplex.com/releases/view/60102

=====================================================================================================

DIRECTORY LAYOUT OF THE REPOSITORY

The repository is designed to mirror the CodePlex repository, which means the structure includes spaces for third party modules/mainboards/etc.

NETMF_42 branch (this is the "head" and will be folded back into MAIN at some point)
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
				See the kit template  (in GadgeteerBuilderTemplates.msi) for details of structure under here


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
				\ .NET Micro Framework 4.X (4.1 and 4.2)
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
					\ NETMF 4.X (4.1 and 4.2)
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

[VS2010_ROOT_FOLDER]\Common7\IDE\{Extensions,VCSExpressExtensions,VBExpressExtensions} 
	\ Microsoft 
		\ Gadgeteer - App templates (for each language and compatible NETMF version)
		\ GadgeteerBT - Mainboard, module, kit templates 
		\ GadgeteerDSL - Designer 

[START MENU]
	\ ManufacturerName
		\ KitName
			GettingStarted.htm shortcut

[REGISTRY HKLM]\SOFTWARE\Microsoft\
	\ .NETMicroFramework\v4.X\AssemblyFoldersEx (4.1 and 4.2)
		\ Microsoft .NET Gadgeteer
			Assembly location for Gadgeteer core libraries [Program Files]\Microsoft .NET Gadgeteer\Core\Assemblies\.NET Micro Framework 4.1\
		\ GTM.ManufacturerName.ModuleName
			Assembly location for module dll [Program Files]\ManufacturerName\Microsoft .NET Gadgeteer\Modules\ModuleName\NETMF 4.1\
		\ ManufacturerName.Gadgeteer.MainboardName
			Assembly location for mainboard dll [Program Files]\ManufacturerName\Microsoft .NET Gadgeteer\Mainboards\MainboardName\NETMF 4.1\

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
This tries to write all the .cs files under GeneratedCode (for both projects) so they need to be checked out.  Or, hit the button under Solution Explorer's toolbar which does a "Transform All Templates" and this will auto-check-out the files.
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
	- The version of the DLLs AND the MSM is controlled using version.txt

GadgeteerBuilderTemplates.msi is built by the BuilderTemplateInstaller wix project in GadgeteerBuilderTemplates.sln, and relies on:
	- The projects in that SLN file
	- A SIGNED dll for the HardwareTemplateWizard project (location specified in Product.wxs)
		- The SignWizardAssembly.cmd kicks off signing of these projects using https://codesign/
	- The version of the MSI is controlled using version.wxi (shared with GadgeteerCore.sln)

Copies of installers are placed in $\Builds - follow the pattern there, and often in \\gadgeteer\share\drops or \\gadgeteer\share\releases
Version number schema is documented in GadgeteerCore\AssemblyInfoGlobal.cs

To build and sign the two MSIs (after the previous two DLL signing stages are already done, and the version numbers are all the same), use GadgeteerCore\SignMSIs.cmd.

=====================================================================================================

GADGETEERCORE: KNOWN ISSUES

-- Pathname length problem with VS templates
The VS templates live at [ProgramFilesFolder]Microsoft Visual Studio 10.0\Common7\IDE\{VCSExpress}Extensions\[Manufacturer]\Gadgeteer\ProjectTemplates\CSharp\Gadgeteer\1033\*.zip 
Unfortunately these paths+filenames turns out to be TOO LONG for VS to parse (bug in VS?).  So instead of the source filename e.g. GadgeteerSimpleGraphicsAppTemplate.zip, the installer uses short target names, e.g. SG.zip
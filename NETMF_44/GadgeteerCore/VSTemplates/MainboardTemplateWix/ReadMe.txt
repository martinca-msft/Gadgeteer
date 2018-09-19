﻿This is a Microsoft .NET Gadgeteer mainboard project created using the Mainboard template in .NET Gadgeteer Builder Templates.
This template was most recently changed on 2015-11-03.

This template provides a simple way to build software for a custom Microsoft .NET Gadgeteer mainboard
that is compliant with the Microsoft .NET Gadgeteer Mainboard Builder's Guide specifications found at http://gadgeteer.codeplex.com/ 
Using this template auto-generates an installer (MSI) that can easily be distributed to end users, and an 
installation merge module (MSM) that can be used to make kit installers including many modules/mainboards/templates/etc.

We recommend that you read the Mainboard Builder's Guide at http://gadgeteer.codeplex.com/ for full details.

==============================================

SYSTEM REQUIREMENT

To build the installer MSI automatically, this template requires WiX 3.10 or newer to be installed:
http://wix.codeplex.com/releases/view/617257

Recommended installation:
 Visual Studio 2015 (community edition or higher) and .NET Micro Framework v4.4 and .NET Micro Framework v4.3
    This allows you to build .NET Gadgeteer mainboard installer for all NETMF versions v4.1 - v4.4

Other installation possibilities:
 Visual Studio 2015 (community edition or higher) and .NET Micro Framework v4.4 -- targets NETMF v4.4 only 
 Visual Studio 2013/2012 (any edition) and .NET Micro Framework v4.3 -- targets NETMF v4.3/4.2/4.1
 Visual Studio Express 2013/2012 for Windows Desktop and .NET Micro Framework v4.3 -- targets NETMF v4.3/4.2/4.1
 Visual Studio 2010 (any edition) and .NET Micro Framework v4.2 -- targets NETMF v4.2/4.1
 Visual C# 2010 Express and .NET Micro Framework v4.2 -- targets NETMF v4.2/4.1

NB only C# templates are provided, though Visual Basic would work. 

==============================================

BUILD NOTES

Building with the Release configuration generates an MSI installer, which includes your code, in the output directory 
of the project (bin/Release/Installer). This takes a little time, and Visual Studio may be unresponsive during the build.  
To avoid this delay, build with the Debug configuration. 

Visual C# 2010 Express normally builds in Release configuration only. In order to turn off the installer build to speed up the build process,
you can go to Menu->Project->[MainboardName] Properties->Build tab and tick the "Define DEBUG constant" box.

If you see the error "The system cannot find the file..." try "Rebuild" rather than "Build"

==============================================

MAINBOARD TEMPLATE DETAILS

This template supports NETMF 4.1, NETMF 4.2, NETMF 4.3 and NETMF 4.4 - so mainboards can provide a driver for all versions of NETMF.
Note that you at least need Visual Studio 2012 (or VS Express 2012 for Windows Desktop) to support NETMF 4.3.

The template is split into three projects each starting with the name of the mainboard:
 [MainboardName]_41 : the NETMF 4.1 driver for the mainboard
 [MainboardName]_42 : the NETMF 4.2 driver for the mainboard
 [MainboardName]_43 : the NETMF 4.3 driver for the mainboard
 [MainboardName]_44 : the NETMF 4.4 driver for the mainboard
 [MainboardName] : the installer for the mainboard

For the NETMF projects, the files are similar:
1) [MainboardName]_4x.cs - software implementation of the mainboard's "device driver".
2) Properties\AssemblyInfo.cs - a cs file including the version number of the mainboard driver, see MAKING CHANGES for when to change this.

In the installer project (with no suffix), the files are:
1) ReadMe.txt - this file
2) GadgeteerHardware.xml - defines some parameters about your mainboard.
3) Image.jpg - placeholder for an image representing the mainboard.
4) common.wxi - WiX (installer) configuration file that specifies parameters for the installer, including the version number. 
5) en-us.wxl - WiX (installer) localization file that specifies text strings that are displayed during installation.
6) msm.wxs - WiX (installer) script that generates an installation "merge module".
7) msi.wxs - WiX (installer) script that generates an installer (msi file) using the merge module.
8) G.ico - G graphic used by the installer.

==============================================

MAINBOARD TEMPLATE USE INSTRUCTIONS

1)  Edit the [MainboardName].cs files to implement software for your mainboard.
    If you only want to support some version(s) of NETMF then you only need to edit the relevant CS file(s), nonetheless, it is recommended
    not to delete the other project(s) in case you wish to add support for the other version(s) of NETMF in future. 
    There are comments and examples in the cs files to assist you with this process.

2)  Test your mainboard. Mainboards cannot be run directly, since they are class libraries (dll) not executables (exe).   
    Testing is most easily accomplished by adding a new Gadgeteer project to the same Visual Studio/Visual C# Express solution. 
    You will need to create a separate test project for each supported NETMF version.
    With the new Program.cs file open, use the menu item Project->Add Reference, and, in the Projects tab, choose your mainboard.
    Since the designer generates code instantiating the mainboard, we have to disable it for testing.
    Copy the content from Program.Generated.cs to the Program.cs file, and delete the Program.Generated.cs file and the .Gadgeteer file
    of the designer.  Then make the Program.cs file use your new mainboard type [Manufacturer Name].Gadgeteer.[Mainboard Name].
    You will have to manually add modules (add reference and instantiate) to test with modules.  
    (Note - to run your test program, using the keyboard shortcut "F5" sometimes deploys the wrong app if there are >1 NETMF apps in a solution,
    right-clicking on the project name in Solution Explorer and choosing Debug->Start New Instance should always work.)
                                                                    
3)  Edit the GadgeteerHardware.xml file to specify information about your mainboard, as described in that file.

4)  Change Image.jpg to a good quality top-down image of the mainboard with the socket side facing up,
    cropped tight (no margin), in the same orientation as the width and height specified in GadgeteerHardware.xml (not rotated).   

5)  Build in Release configuration to build the mainboard installer in the installer project's bin\Release\Installer directory!
    Install this and check that it appears correctly in the Control Panel's Add/Remove Programs window.

6)  Now open a new Gadgeteer App (for the supported version(s) of NETMF) and your mainboard should appear in the designer.
    It is recommended to re-run tests using the designer, as well as checking the appearance of the mainboard in the designer.

==============================================

RELEASING THE MAINBOARD SOFTWARE, INDIVIDUALLY OR IN A KIT

The MSI installer generated in the installer project's bin\Release\Installer directory can be distributed to end users.
The MSM merge module in the installer project's bin\Release\Installer directory can be used to build other installers such as "kit" installers 
that incorporate multiple module(s)/mainboard(s).  This will install/remove correctly - e.g. if two kits including a Foo Mainboard 
are both installed, there will be one copy of the Foo mainboard (the most up-to-date version) and if either kit is removed, 
the Foo mainboard will remain installed, because the other kit requires it.

==============================================

MAKING CHANGES

If you make want to release a new version of your mainboard, make sure to change the version number in common.wxi. 
Otherwise, the auto-generated installer will not be able to upgrade the older version correctly (an error message will result).
It is also necessary to change the versions in ALL Properties\AssemblyInfo.cs files and often a good idea to 
keep these synchronized with your common.wxi version.  

If you have are using the kit template to make an installer, then we you may wish to synchronize the version numbers with the kit.
This process is documented in the Kit readme.txt file; it requires one-off changes to the AssemblyInfo.cs and common.wxi files.

If you want to change the long or short name of your mainboard, or the long or short manufacturer name, it can be tricky to
find all references to those names.  Do a global search/replace on the whole solution, but in addition, look in the  
NETMF projects' project properties (search/replace doesn't find this!) under Assembly Name and Default Namespace.

==============================================

UPGRADING FROM AN OLDER TEMPLATE 

1) Create a new mainboard template with the same name as the old one.  
2) Edit the new GadgeteerHardware.xml file referring to your old file (dont just copy over the top as you may miss necessary changes)
3) Copy over the Image.jpg
4) For each NETMF version you want to support
   - Copy your old cs files (including the Properties\AssemblyInfo.cs file) to the new projects of the same name 
   - Copy them also to newly-supported versions, fixing any compiler errors that might occur due to SDK changes
5) Before building the installer, you need to copy over TWO GUIDs in order to make the upgrade work.  
   In common.wxi, just copy over the GUIDs for guid_msi_upgrade_code and guid_msi_package_id from the old version.
6) Make sure to use a higher version number than the old one - you must change each Properties\AssemblyInfo.cs file and common.wxi 

If you follow these steps, the new installer should upgrade correctly over the old installer.

==============================================

NOTES ON VISUAL STUDIO BEHAVIOR

These notes document how you can affect how your hardware is represented in the Visual Studio designer and app wizard.
With point (2) particularly, this will cause some modules ported over from 2.41.500 builder templates (or older) to NOT WORK with NETMF 4.2 (or newer)
unless changes are made.

1) [Added in 2.42.600] Only hardware supporting the current project's framework version (i.e. having least one Assembly for that version) will be shown.
   When porting from 4.1, you must add <Assembly MFVersion="4.2" Name="drivername"/> in the <Assemblies> element (4.3 and 4.4 respectively).

2) [Added in 2.42.600] A few components of Gadgeteer have been split into separate assemblies to make it possible to exclude them when not required.
   This saves RAM and flash space when deployed. The designer can automatically include such core "fragments" when including a module
   that relies on them or is commonly used with them.  The fragment libraries should be listed in the Assemblies section of the module
   alongside that module's driver, so that the designer automatically includes a reference to them when that module is included.
   The fragments are: Gadgeteer.SPI, Gadgeteer.Serial, Gadgeteer.DaisyLink (inc. SoftwareI2C), Gadgeteer.WebClient and Gadgeteer.WebServer
   If you use any of these in your module, then it will NOT work in 4.2 and higher unless you add an Assemblies entry in GadgeteerHardware.xml

3) [Added in 2.42.600] A mainboard with built-in modules should nonetheless provide separate module classes for each type of functionality.  
   These modules should be named in a way that makes it clear that they only apply to one mainboard, e.g. ModulenameForMainboardname.
   The mainboard should list its driver under LibrariesProvided; the module should list the mainboard driver in ExtraLibrariesRequired.
   The ErrorMessage attribute of the module will be shown if the module is used with any other mainboard, and should make it 
   clear why that does not work, e.g.: "This module is built-in to MainboardName and cannot be used with any other mainboard."

4) [Added in 2.42.600] A mainboard with custom firmware support libraries that modules might rely directly (so that the modules do not work with all mainboards)
   should list the assembly names in LibrariesProvided.  A module requiring a custom mainboard firmware library should list that under ExtraLibrariesRequired.

5) [Added in 2.42.600] The MinimumGadgeteerCoreVersion attribute is supported, and the designer will present the user with an error message if the installed
   version of GadgeteerCore is not high enough, pointing them at http://gadgeteer.codeplex.com/ to get a newer version.

6) [Added in 2.42.700] The .NET Gadgeteer Application wizard allows the user to choose from the NETMF versions supported by a mainboard.  The default choice
   is the first one listed in GadgeteerHardware.xml, so you can control this.  This wizard also uses the URL found in the GadgeteerHardware.xml HelpURL attribute 
   to help users get past version problems, so it is helpful to fill that in.
   
7) [Added in 2.42.700] The VS designer can show the estimated power consumption of the hardware. This uses <Power> elements from GadgeteerHardware.xml.  
   Read that file for instructions on how to enable this for your hardware.      
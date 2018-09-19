# Microsoft .Net Gadgeteer

Microsoft .Net Gadgeteer is a rapid-prototyping system for building small electronic devices targetting the .Net MicroFramework. This release is a reference and starting point for the .Net MicroFramework community. It is not a build-ready source tree.

While the .Net MicroFramework and parts of Gadgeteer have been open-source for some time, the Designer has not. Sometime in 2016, Microsoft ceased making new releases of the Gadgeteer Designer and there is no version for Visual Studio 2017. In order to allow the community to more easily carry Gadgeteer forward, and as a general learning resource, a group of Microsoft people previously involved with Gadgeteer decided it would be best to release the designer code under an open source license.  This is that release.

## Code of Conduct
This project has adopted the [Microsoft Open Source Code of
Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct
FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com)
with any additional questions or comments.

## Missing Components
This is a reference release of the source code for the Microsoft Gadgeteer Designer and related components.  **This repo does not build in it's current form.** For legal reasons, a few items have been removed from the repository before it was made public here on GitHub. None of these are irreplacable - they just couldn't be redistributed as is, so you will need to find replacements on the public Internet. 

Items removed include:
* Binary (dll) components of the Microsoft Modeling SDK and Text Templating were deleted before publication here. These were in directories named 'ModelingCache'. Those directories now contain a text file called MissingFiles.txt. We cannot use this GitHub repo to re-distribute Microsoft binaries. For Visual Studio 2017, you can install the missing components from [here](https://blogs.msdn.microsoft.com/devops/2016/12/12/the-visual-studio-modeling-sdk-is-now-available-with-visual-studio-2017/). You may need to either alter the build scripts or copy the installed files into the ModelingCache directories.
* Installer CustomActions.dll files were deleted prior to publication. These existed here only in binary form and were not available for redistribution under a open source license. The needed functions are described in the customaction.wxi files and should be simple to reproduce.
* 3rd party code has been removed prior to publication.  We could not re-release code that outside companies gave us in support of their components under a potentially different license. Much of this code exists in the CodePlex archive found [here](https://archive.codeplex.com/?p=gadgeteer).  To build a full release that includes mainboard and module support, you may need to copy the Mainboard and Modules subdirectories from that CodePlex archive.
* Code signing of selected components is required for installation in the .Net Global Assembly Cache (GAC) during installation. The final setup packages (msi, vsix) were also signed. Because thse signing steps relied on certificates, servers, and processes that are internal to Microsoft, those signing scripts were removed from this release. You will need to add signing (.snk files or equivalent post-build steps) before you will be able to create a working install package.
* Images containing trademarks of Microsoft or 3rd parties have been removed, though may still be referenced by setup scripts (wxi, wxs files).

## Layout
You should probably look at the NETMF_44 subdirectory.  This is the release that went with .Net MicroFramework 44, and in spite of its name, it contains the components for Gadgeteer in .Net MicroFramework versions 4.1, 4.2, 4.3, and 4.4.  The NETMF_41, NETMF_42, and NETMF_43 directories are present only for historical reference.  This format may seem a bit odd, but this is how the repository was represented during internal development and we chose to preseve it as-is.  That said, the projects in NETMF_44 and additional external components (see above) should be all you need to build a working release.

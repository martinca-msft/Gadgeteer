This installer (GadgeteerBuilderTemplates.msi) includes three templates
- Module Template
- Mainboard Template
- Kit Template

It also includes the HardwareTemplateWizard assembly which provides a project wizard for the module and mainboard templates.

This installer augments the App Templates found in the GadgeteerCore.msi installer (which is a prerequisite for this installer).

The HardwareTemplateWizard output dll needs strong name signing or else the MSI will fail (since it tries to add the assembly to the GAC). 
The SignWizardAssembly.cmd command (in the HardwareTemplateWizardSigning directory) kicks this off programatically, or this can be done manually using https://codesign/ 
The BuilderTemplateInstaller MSI by default uses a cached version of the signed DLL.
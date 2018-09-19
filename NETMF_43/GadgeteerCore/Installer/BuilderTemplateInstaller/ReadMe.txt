This installer (GadgeteerBuilderTemplates.msi) includes three templates
- Module Template
- Mainboard Template
- Kit Template

It also includes the HardwareTemplateWizard assembly which provides a project wizard for the module and mainboard templates.

This installer augments the App Templates found in the GadgeteerCore.msi installer (which is a prerequisite for this installer).

The HardwareTemplateWizard output dll needs strong name signing or else the MSI will fail (since it tries to add the assembly to the GAC). 
The BuilderTemplateInstaller MSI by default uses a cached version of the signed DLL.

[Note: the signing process previously used a Microsoft-internal signing workflow. Those scripts and references to that workflow have been
removed from this public release. You can still use self-signed certificates or CA-signed certificates to sign the dlls yourself.]
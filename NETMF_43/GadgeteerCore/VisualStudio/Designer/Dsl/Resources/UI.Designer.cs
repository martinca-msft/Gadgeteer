﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18213
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.Gadgeteer.Designer.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class UI {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal UI() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.Gadgeteer.Designer.Resources.UI", typeof(UI).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Auto-connect is taking a long time. Do you want to keep waiting?.
        /// </summary>
        internal static string AutoConnectTimeout {
            get {
                return ResourceManager.GetString("AutoConnectTimeout", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This item can&apos;t be removed because module &apos;{0}&apos; depends on it..
        /// </summary>
        internal static string BrokenDependencies {
            get {
                return ResourceManager.GetString("BrokenDependencies", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Micro Framework version of this project could not be determined..
        /// </summary>
        internal static string CannotDetermineMFVersion {
            get {
                return ResourceManager.GetString("CannotDetermineMFVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Double click here to add a comment..
        /// </summary>
        internal static string CommentInitialText {
            get {
                return ResourceManager.GetString("CommentInitialText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Configuration.
        /// </summary>
        internal static string DesignPropertyDefaultCategory {
            get {
                return ResourceManager.GetString("DesignPropertyDefaultCategory", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The module {0} is connected in a loop. Try different connection in the designer..
        /// </summary>
        internal static string GenerateLoop {
            get {
                return ResourceManager.GetString("GenerateLoop", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Microsoft .NET Gadgeteer.
        /// </summary>
        internal static string MessageBoxTitle {
            get {
                return ResourceManager.GetString("MessageBoxTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This {0} {1} requires a newer version of .NET Gadgeteer Core (version {2}) than the one installed.  You can get the latest version from http://gadgeteer.codeplex.com/. .
        /// </summary>
        internal static string MinimumGadgeteerVersionNotMet {
            get {
                return ResourceManager.GetString("MinimumGadgeteerVersionNotMet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A required library could not be found: {0}.
        /// </summary>
        internal static string MissingRequiredLibrary {
            get {
                return ResourceManager.GetString("MissingRequiredLibrary", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The module could not be added:
        ///{0}.
        /// </summary>
        internal static string ModuleCouldNotBeAdded {
            get {
                return ResourceManager.GetString("ModuleCouldNotBeAdded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Would you like to replace the current mainboard? Any existing module connections will be removed..
        /// </summary>
        internal static string ReplaceMainboardPrompt {
            get {
                return ResourceManager.GetString("ReplaceMainboardPrompt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tip: To have the designer connect all modules for you, right-click the diagram, and then click &apos;Connect all modules&apos;..
        /// </summary>
        internal static string StartTip {
            get {
                return ResourceManager.GetString("StartTip", resourceCulture);
            }
        }
    }
}

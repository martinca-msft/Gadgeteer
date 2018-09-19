// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using System.CodeDom.Compiler;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Modeling;

namespace Microsoft.Gadgeteer.Designer
{
    /// <summary>
    /// Validate the name of the module to make sure it's a valid C# identifier. This class will correct the name or prevent the transaction
    /// from committing if the removal of invalid characters resulted in an empty string. 
    /// </summary>
    [RuleOn(typeof(Module), FireTime = TimeToFire.TopLevelCommit)]
    class ModuleNameChangedRule : ChangeRule
    {
        CommitBlocker commitBlocker;                

        public override void ElementPropertyChanged(ElementPropertyChangedEventArgs e)
        {
            base.ElementPropertyChanged(e);

            if (e.DomainProperty.Id == Module.NameDomainPropertyId)
            {                
                var module = (Module)e.ModelElement;
                module.Name = Module.FixName((string)e.NewValue);

                if (commitBlocker == null)
                    commitBlocker = new CommitBlocker(e.ModelElement.Store);

                commitBlocker.BlockCommitIf(string.IsNullOrWhiteSpace(module.Name) || module.CountModulesWithName(module.Name)>1);
            }
        }
        
    }
}

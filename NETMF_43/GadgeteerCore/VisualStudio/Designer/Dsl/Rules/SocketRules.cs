// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using Microsoft.VisualStudio.Modeling;

namespace Microsoft.Gadgeteer.Designer
{

    //This rule makes sure the user doesn't delete individual sockets. But if they are deleted as part of a module delete for instance
    //(ChangeSource=Propagate) we let that through.    
    [RuleOn(typeof(GadgeteerHardwareHasSockets), FireTime = TimeToFire.TopLevelCommit)]
    class SocketRemovedRule : DeleteRule
    {
        CommitBlocker commitBlocker;

        public override void ElementDeleted(ElementDeletedEventArgs e)
        {            
            //Only prevent commit of the source is Normal
            if (e.ChangeSource == ChangeSource.Normal)
            {
                //Note we don't want to simply abort the transaction, as other code may be expecting the transaction
                //to still be alive

                //The first time register the CanCommitCallback
                if (commitBlocker == null)
                    commitBlocker = new CommitBlocker(e.ModelElement.Store); 

                commitBlocker.BlockCommitIf(true);
            }
        }
    }    
}

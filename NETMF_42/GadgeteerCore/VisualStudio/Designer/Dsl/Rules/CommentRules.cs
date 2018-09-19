// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using Microsoft.VisualStudio.Modeling;

namespace Microsoft.Gadgeteer.Designer
{
    /// <summary>
    /// Rule to provide initial helper text for comment boxes
    /// </summary>
    [RuleOn(typeof(Comment), FireTime = TimeToFire.TopLevelCommit)]
    class CommentAddedRule : GadgeteerAddRule<Comment>
    {
        public CommentAddedRule() : base(true) {}

        public override void ElementAdded(Comment element)
        {
            if (string.IsNullOrWhiteSpace(element.Text))
            {
                element.Text = Resources.UI.CommentInitialText;
            }
        } 
    }
}

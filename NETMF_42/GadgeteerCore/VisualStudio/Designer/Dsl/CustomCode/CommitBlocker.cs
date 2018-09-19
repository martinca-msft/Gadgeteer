// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the Apache v.2 license.
using Microsoft.VisualStudio.Modeling;

namespace Microsoft.Gadgeteer.Designer
{
    /// <summary>
    /// A helper class to prevent transactions from committing under certain conditions
    /// </summary>
    class CommitBlocker
    {
        private readonly CanCommitCallback canCommitCallback;
        private readonly object preventCommitCookie;
        private readonly Store store;

        public CommitBlocker(Store store)
        {
            this.store = store;
            preventCommitCookie = new object();
            canCommitCallback = new CanCommitCallback(CanCommit);
            store.TransactionManager.AddCanCommitCallback(canCommitCallback);
        }

        /// <summary>
        /// Adds a flag (a cookie in the context) to the current top level transaction to prevent it from committing.
        /// </summary>        
        public void BlockCommitIf(bool condition)
        {
            if (condition && store.TransactionManager.CurrentTransaction!=null)
            {
                var tx = store.TransactionManager.CurrentTransaction.TopLevelTransaction;
                tx.Context.Add(preventCommitCookie, true);
            }
        }

        //CanCommit callback implementation that prevents the commit if BlockCommitIf(true) was previously called
        private CanCommitResult CanCommit(Transaction tx)
        {
            return tx.Context.Contains(preventCommitCookie) ? CanCommitResult.Rollback : CanCommitResult.Commit;
        }
    }
}

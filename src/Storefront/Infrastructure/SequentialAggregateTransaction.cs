// -----------------------------------------------------------------------
// <copyright file="SequentialAggregateTransaction.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// An aggregate business transaction which executes one or more other transactions sequentially. This is NOT thread safe. Use different
    /// instances for each thread.
    /// </summary>
    public class SequentialAggregateTransaction : IBusinessTransaction
    {
        /// <summary>
        /// The children transactions this aggregate holds.
        /// </summary>
        private readonly IBusinessTransaction[] childTransactions;

        /// <summary>
        /// The index of the last child transaction which was executed.
        /// </summary>
        private int lastExecutedTransactionIndex = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequentialAggregateTransaction"/> class.
        /// </summary>
        /// <param name="childTransactions">The children transaction to be held as part of this aggregate.</param>
        public SequentialAggregateTransaction(IEnumerable<IBusinessTransaction> childTransactions)
        {
            childTransactions.AssertNotNull(nameof(childTransactions));

            foreach (var transaction in childTransactions)
            {
                transaction.AssertNotNull("childTransactions contains a null transaction");
            }

            this.childTransactions = childTransactions.ToArray();
        }

        /// <summary>
        /// Executes the transaction.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task ExecuteAsync()
        {
            for (this.lastExecutedTransactionIndex = 0; this.lastExecutedTransactionIndex < this.childTransactions.Length; ++this.lastExecutedTransactionIndex)
            {
                await this.childTransactions[this.lastExecutedTransactionIndex].ExecuteAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Rolls back the transaction.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task RollbackAsync()
        {
            // rollback the transactions in reverse
            for (int i = this.lastExecutedTransactionIndex - 1; i >= 0; --i)
            {
                try
                {
                    await this.childTransactions[i].RollbackAsync().ConfigureAwait(false);
                }
                catch (Exception rollbackProblem)
                {
                    if (rollbackProblem.IsFatal())
                    {
                        throw;
                    }

                    Trace.TraceError("sequentialAggregateTransaction.RollbackAsync failed: {0}", rollbackProblem);
                }
            }

            this.lastExecutedTransactionIndex = -1;
        }
    }
}
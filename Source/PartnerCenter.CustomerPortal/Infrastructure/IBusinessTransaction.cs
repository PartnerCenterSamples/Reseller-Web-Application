// -----------------------------------------------------------------------
// <copyright file="IBusinessTransaction.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Infrastructure
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an atomic business transaction which can be rolled back in case there was a failure.
    /// </summary>
    public interface IBusinessTransaction
    {
        /// <summary>
        /// Executes the transaction.
        /// </summary>
        /// <returns>A task.</returns>
        Task ExecuteAsync();

        /// <summary>
        /// Rolls back the transaction.
        /// </summary>
        /// <returns>A task.</returns>
        Task RollbackAsync();
    }
}
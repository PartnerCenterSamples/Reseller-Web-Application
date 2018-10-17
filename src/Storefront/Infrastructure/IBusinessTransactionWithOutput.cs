// -----------------------------------------------------------------------
// <copyright file="IBusinessTransactionWithOutput.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Infrastructure
{
    /// <summary>
    /// A transaction that has an output.
    /// </summary>
    /// <typeparam name="TOutput">The type of data returned when the transaction is executed.</typeparam>
    public interface IBusinessTransactionWithOutput<out TOutput> : IBusinessTransaction
    {
        /// <summary>
        /// Gets the transaction execution result.
        /// </summary>
        TOutput Result { get; }
    }
}
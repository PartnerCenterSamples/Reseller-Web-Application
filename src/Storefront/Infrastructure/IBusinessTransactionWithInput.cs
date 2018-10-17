// -----------------------------------------------------------------------
// <copyright file="IBusinessTransactionWithInput.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Infrastructure
{
    using System;

    /// <summary>
    /// A transaction that needs an input to execute.
    /// </summary>
    /// <typeparam name="TInput">The type of input.</typeparam>
    public interface IBusinessTransactionWithInput<out TInput> : IBusinessTransaction
    {
        /// <summary>
        /// Gets a callback function which will be invoked to retrieve the input.
        /// </summary>
        Func<TInput> AcquireInput { get; }
    }
}
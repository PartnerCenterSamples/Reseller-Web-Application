// -----------------------------------------------------------------------
// <copyright file="TransactionResult.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Holds summary information for a commerce transaction result.
    /// </summary>
    public class TransactionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionResult"/> class.
        /// </summary>        
        /// <param name="lineItems">A collection of line items bundled in the transaction.</param>
        /// <param name="timeStamp">The time at which the transaction took place.</param>
        public TransactionResult(IEnumerable<TransactionResultLineItem> lineItems, DateTime timeStamp)
        {
            // we don't validate amount charged since a transaction may result in a negative amount
            if (lineItems == null || !lineItems.Any())
            {
                throw new ArgumentException("lineItems must at least have one line item", nameof(lineItems));
            }

            foreach (TransactionResultLineItem lineItem in lineItems)
            {
                lineItem.AssertNotNull("lineItems has an empty entry");
            }

            LineItems = lineItems;
            TimeStamp = timeStamp;
        }

        /// <summary>
        /// Gets the result line items associated with the transaction.
        /// </summary>
        public IEnumerable<TransactionResultLineItem> LineItems { get; private set; }

        /// <summary>
        /// Gets the time at which the transaction took place.
        /// </summary>
        public DateTime TimeStamp { get; private set; }
    }
}
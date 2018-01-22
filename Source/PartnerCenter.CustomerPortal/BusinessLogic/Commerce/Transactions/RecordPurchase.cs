// -----------------------------------------------------------------------
// <copyright file="RecordPurchase.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.BusinessLogic.Commerce.Transactions
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Infrastructure;
    using Models;

    /// <summary>
    /// Records a purchase the customer just made. A purchase can result in a new
    /// subscription or extending an existing one or adding additional seats.
    /// </summary>
    public class RecordPurchase : IBusinessTransactionWithOutput<CustomerPurchaseEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordPurchase"/> class.
        /// </summary>
        /// <param name="repository">A customer purchases repository which manages customer purchases persistence.</param>
        /// <param name="newPurchaseRecord">The new customer purchase to record.</param>
        public RecordPurchase(CustomerPurchasesRepository repository, CustomerPurchaseEntity newPurchaseRecord)
        {
            repository.AssertNotNull(nameof(repository));
            newPurchaseRecord.AssertNotNull(nameof(newPurchaseRecord));

            this.CustomerPurchasesRepository = repository;
            this.CustomerPurchaseToPersist = newPurchaseRecord;
        }

        /// <summary>
        /// Gets the customer purchase repository used to persist the purchase.
        /// </summary>
        public CustomerPurchasesRepository CustomerPurchasesRepository { get; private set; }

        /// <summary>
        /// Gets the customer purchase entity to persist.
        /// </summary>
        public CustomerPurchaseEntity CustomerPurchaseToPersist { get; private set; }

        /// <summary>
        /// Gets the resulting customer purchase entity.
        /// </summary>
        public CustomerPurchaseEntity Result { get; private set; }

        /// <summary>
        /// Persists the purchase.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task ExecuteAsync()
        {
            this.Result = await this.CustomerPurchasesRepository.AddAsync(this.CustomerPurchaseToPersist);
        }

        /// <summary>
        /// Removes the purchase from persistence.
        /// </summary>
        /// <returns>A task.</returns>
        public async Task RollbackAsync()
        {
            if (this.Result != null)
            {
                try
                {
                    // delete the inserted row
                    await this.CustomerPurchasesRepository.DeleteAsync(this.Result);
                }
                catch (Exception deletionProblem)
                {
                    if (deletionProblem.IsFatal())
                    {
                        throw;
                    }

                    Trace.TraceError(
                        "RecordPurchase.RollbackAsync failed: {0}, Customer ID: {1}, ID: {2}",
                        deletionProblem,
                        this.Result.CustomerId,
                        this.Result.Id);

                    // TODO: Notify the system integrity recovery component
                }

                this.Result = null;
            }
        }
    }
}
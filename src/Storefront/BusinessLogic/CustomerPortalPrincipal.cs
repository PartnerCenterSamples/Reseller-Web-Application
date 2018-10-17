// -----------------------------------------------------------------------
// <copyright file="CustomerPortalPrincipal.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic
{
    using System.Security.Claims;
    using Configuration;

    /// <summary>
    /// Encapsulates relevant information about the logged in user.
    /// </summary>
    public class CustomerPortalPrincipal : ClaimsPrincipal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerPortalPrincipal"/> class.
        /// </summary>
        /// <param name="userClaimsPrincipal">A user claims principal created by AAD.</param>
        public CustomerPortalPrincipal(ClaimsPrincipal userClaimsPrincipal) : base(userClaimsPrincipal)
        {
            TenantId = userClaimsPrincipal.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid")?.Value;
            Name = userClaimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
            Email = userClaimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;

            // the customer ID will be empty in the case where a new prospective customer signs in with their existing Org ID or when a partner user signs in
            PartnerCenterCustomerId = userClaimsPrincipal.FindFirst("PartnerCenterCustomerID")?.Value;
        }

        /// <summary>
        /// Gets the AAD tenant ID of the signed in user.
        /// </summary>
        public string TenantId { get; private set; }

        /// <summary>
        /// Gets the name of the signed in user.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the email of the signed in user.
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the signed in user is a portal administrator or not.
        /// </summary>
        public bool IsPortalAdmin
        {
            get
            {
                // TODO: later on, we may want to implement RBAC but as of now, all users signed in from the portal's tenant are considered admins
                return TenantId == ApplicationConfiguration.ActiveDirectoryTenantId;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the sign in user is a current Partner Center customer or not.
        /// </summary>
        public bool IsPartnerCenterCustomer
        {
            get
            {
                return !string.IsNullOrEmpty(PartnerCenterCustomerId);
            }
        } 

        /// <summary>
        /// Gets the Partner Center customer ID associated with the sign in user (if any).
        /// </summary>
        public string PartnerCenterCustomerId { get; private set; }
    }
}
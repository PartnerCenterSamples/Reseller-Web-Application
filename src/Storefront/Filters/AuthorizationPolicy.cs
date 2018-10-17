// -----------------------------------------------------------------------
// <copyright file="AuthorizationPolicy.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Filters
{
    using BusinessLogic;

    /// <summary>
    /// Implements authorization policy based on the required role and the logged in user.
    /// </summary>
    public class AuthorizationPolicy
    {
        /// <summary>
        /// Determines if the given user principal is authorized based on the require user role or not.
        /// </summary>
        /// <param name="principal">The logged in user.</param>
        /// <param name="requiredUserRole">The required user role.</param>
        /// <returns>True if authorized, false otherwise.</returns>
        public bool IsAuthorized(CustomerPortalPrincipal principal, UserRole requiredUserRole)
        {
            principal.AssertNotNull(nameof(principal));

            bool isAuthorized = false;

            switch (requiredUserRole)
            {
                case UserRole.Customer:
                    isAuthorized = principal.IsPartnerCenterCustomer && !principal.IsPortalAdmin;
                    break;
                case UserRole.Partner:
                    isAuthorized = !principal.IsPartnerCenterCustomer && principal.IsPortalAdmin;
                    break;
                case UserRole.Any:
                    isAuthorized = principal.IsPartnerCenterCustomer || principal.IsPortalAdmin;
                    break;
                case UserRole.None:
                    isAuthorized = !principal.IsPartnerCenterCustomer && !principal.IsPortalAdmin;
                    break;
            }

            return isAuthorized;
        }
    }
}
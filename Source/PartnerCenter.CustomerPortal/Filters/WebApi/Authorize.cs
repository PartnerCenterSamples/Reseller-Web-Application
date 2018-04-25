// -----------------------------------------------------------------------
// <copyright file="Authorize.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Filters.WebApi
{
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using BusinessLogic;

    /// <summary>
    /// Implements portal authorization for Web API controllers.
    /// </summary>
    public class Authorize : AuthorizeAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Authorize"/> class.
        /// </summary>
        /// <param name="userRole">The user role to give access to.</param>
        public Authorize(UserRole userRole = UserRole.Any)
        {
            this.UserRole = userRole;
        }

        /// <summary>
        /// Gets or sets the user role which is allowed access.
        /// </summary>
        public UserRole UserRole { get; set; }

        /// <summary>
        /// Authorizes an incoming request based on the user role.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        /// <returns>True if authorized, false otherwise.</returns>
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var principal = actionContext.RequestContext.Principal as CustomerPortalPrincipal;
            return new AuthorizationPolicy().IsAuthorized(principal, this.UserRole);
        }
    }
}
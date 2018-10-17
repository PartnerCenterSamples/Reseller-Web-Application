// -----------------------------------------------------------------------
// <copyright file="PortalAuthorizeAttribute.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Filters.Mvc
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using BusinessLogic;

    /// <summary>
    /// Implements portal authorization for MVC controllers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class PortalAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortalAuthorizeAttribute"/> class.
        /// </summary>
        /// <param name="userRole">The user role to give access to.</param>
        public PortalAuthorizeAttribute(UserRole userRole = UserRole.Any)
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
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>True if authorized, false otherwise.</returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var principal = httpContext.User as CustomerPortalPrincipal;
            return new AuthorizationPolicy().IsAuthorized(principal, this.UserRole);
        }

        /// <summary>
        /// Deals with unauthorized requests.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                // send back a 403
                filterContext.HttpContext.Response.StatusCode = 403;
            }
            else
            {
                // use the default handling for web pages
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}
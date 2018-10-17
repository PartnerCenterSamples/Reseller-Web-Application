// -----------------------------------------------------------------------
// <copyright file="AuthenticationFilterAttribute.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Filters.Mvc
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Filters;
    using BusinessLogic;

    /// <summary>
    /// Augments MVC authentication by replacing the principal with a more usable customer portal principal object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class AuthenticationFilterAttribute : ActionFilterAttribute, IAuthenticationFilter
    {
        /// <summary>
        /// Authenticates an incoming request.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public void OnAuthentication(AuthenticationContext filterContext)
        {
            filterContext.Principal = new CustomerPortalPrincipal(HttpContext.Current.User as System.Security.Claims.ClaimsPrincipal);
        }

        /// <summary>
        /// Challenges the caller.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            // Do nothing
        }
    }
}
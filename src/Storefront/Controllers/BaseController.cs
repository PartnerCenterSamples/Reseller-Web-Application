// -----------------------------------------------------------------------
// <copyright file="BaseController.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Controllers
{
    using System.Web;
    using System.Web.Http;
    using BusinessLogic;

    /// <summary>
    /// The base web API controller. All web API controllers should inherit from this class.
    /// </summary>
    public class BaseController : ApiController
    {
        /// <summary>
        /// Gets the signed in user principal.
        /// </summary>
        protected CustomerPortalPrincipal Principal => HttpContext.Current.User as CustomerPortalPrincipal;
    }
}
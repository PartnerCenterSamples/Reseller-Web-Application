// -----------------------------------------------------------------------
// <copyright file="AiHandleErrorAttribute.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Filters.Mvc
{
    using System;
    using System.Web.Mvc;
    using BusinessLogic;

    /// <summary>
    /// Attribute used to track exceptions using Application Insights if it is configured.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AiHandleErrorAttribute : HandleErrorAttribute
    {
        /// <summary>
        /// Called when an exception occurs.
        /// </summary>
        /// <param name="filterContext">The action-filter context.</param>
        /// <remarks>If customerError is set to Off then the exception will be reported through telemetry.</remarks>
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext?.HttpContext != null && filterContext.Exception != null)
            {
                if (filterContext.HttpContext.IsCustomErrorEnabled)
                {
                    ApplicationDomain.Instance.TelemetryService.Provider.TrackException(filterContext.Exception);
                }
            }

            base.OnException(filterContext);
        }
    }
}
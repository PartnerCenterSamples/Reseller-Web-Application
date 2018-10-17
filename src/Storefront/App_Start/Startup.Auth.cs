// -----------------------------------------------------------------------
// <copyright file="Startup.Auth.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web;
    using BusinessLogic;
    using Configuration;
    using Exceptions;
    using global::Owin;
    using IdentityModel.Tokens;
    using Models;
    using Owin.Security;
    using Owin.Security.Cookies;
    using Owin.Security.OpenIdConnect;

    /// <summary>
    /// Application start up class.
    /// </summary>
    public partial class Startup
    {
        /// <summary>
        /// The Azure global admin user role.
        /// </summary>
        private const string GlobalAdminUserRole = "Company Administrator";

        /// <summary>
        /// Configures application authentication.
        /// </summary>
        /// <param name="app">The application to configure.</param>
        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions { });

            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = ApplicationConfiguration.ActiveDirectoryClientID,
                    Authority = ApplicationConfiguration.ActiveDirectoryEndPoint + "common",
                    TokenValidationParameters = new TokenValidationParameters()
                    {
                        // instead of using the default validation (validating against a single issuer value, as we do in line of business apps),
                        // we inject our own multitenant validation logic
                        ValidateIssuer = false,
                    },
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        RedirectToIdentityProvider = (context) =>
                        {
                            context.ProtocolMessage.Parameters.Add("lc", Resources.Culture.LCID.ToString(CultureInfo.InvariantCulture));
                            return Task.FromResult(0);
                        },
                        AuthorizationCodeReceived = async (context) =>
                        {
                            string userTenantId = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
                            string signedInUserObjectId = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

                            IGraphClient graphClient = new GraphClient(
                                userTenantId,
                                context.Code,
                                 new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)));

                            List<RoleModel> roles = await graphClient.GetDirectoryRolesAsync(signedInUserObjectId).ConfigureAwait(false);

                            foreach (RoleModel role in roles)
                            {
                                context.AuthenticationTicket.Identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role.DisplayName));
                            }
                            if (userTenantId != ApplicationConfiguration.ActiveDirectoryTenantId)
                            {
                                string partnerCenterCustomerId = string.Empty;

                                // Check to see if this login came from the tenant of a customer of the partner
                                PartnerCenter.Models.Customers.Customer customerDetails = await ApplicationDomain.Instance.PartnerCenterClient.Customers.ById(userTenantId).GetAsync().ConfigureAwait(false);

                                // indeed a customer
                                partnerCenterCustomerId = customerDetails.Id;

                                if (!string.IsNullOrWhiteSpace(partnerCenterCustomerId))
                                {
                                    // add the customer ID to the claims
                                    context.AuthenticationTicket.Identity.AddClaim(new System.Security.Claims.Claim("PartnerCenterCustomerID", partnerCenterCustomerId));

                                    // fire off call to retrieve this customer's subscriptions and populate the CustomerSubscriptions Repository.
                                }
                            }
                            else
                            {
                                if (context.AuthenticationTicket.Identity.FindFirst(System.Security.Claims.ClaimTypes.Role).Value != Startup.GlobalAdminUserRole)
                                {
                                    // this login came from the partner's tenant, only allow admins to access the site, non admins will only
                                    // see the unauthenticated experience but they can't configure the portal nor can purchase
                                    Trace.TraceInformation("Blocked log in from non admin partner user: {0}", signedInUserObjectId);

                                    throw new UnauthorizedException(Resources.NonAdminUnauthorizedMessage, HttpStatusCode.Unauthorized);
                                }
                            }
                        },
                        AuthenticationFailed = (context) =>
                        {
                            // redirect to the error page
                            string errorMessage = (context.Exception.InnerException == null) ?
                                context.Exception.Message : context.Exception.InnerException.Message;
                            context.OwinContext.Response.Redirect($"/Home/Error?errorMessage={errorMessage}");

                            context.HandleResponse();
                            return Task.FromResult(0);
                        }
                    }
                });
        }
    }
}
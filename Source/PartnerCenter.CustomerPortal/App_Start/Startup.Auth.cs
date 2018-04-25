// -----------------------------------------------------------------------
// <copyright file="Startup.Auth.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Web;
    using Azure.ActiveDirectory.GraphClient;
    using BusinessLogic;
    using Configuration;
    using IdentityModel.Clients.ActiveDirectory;
    using IdentityModel.Tokens;
    using global::Owin;
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
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        // instead of using the default validation (validating against a single issuer value, as we do in line of business apps), 
                        // we inject our own multitenant validation logic
                        ValidateIssuer = false,
                    },
                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        RedirectToIdentityProvider = (context) =>
                        {
                            context.ProtocolMessage.Parameters.Add("lc", Resources.Culture.LCID.ToString());
                            return Task.FromResult(0);
                        },
                        AuthorizationCodeReceived = async (context) =>
                        {
                            string userTenantId = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
                            string signedInUserObjectId = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;

                            // login to the user AD tenant
                            ClientCredential webPortalcredentials = new ClientCredential(ApplicationConfiguration.ActiveDirectoryClientID, ApplicationConfiguration.ActiveDirectoryClientSecret);
                            AuthenticationContext userAuthContext = new AuthenticationContext(ApplicationConfiguration.ActiveDirectoryEndPoint + userTenantId);
                            AuthenticationResult userAuthResult = userAuthContext.AcquireTokenByAuthorizationCodeAsync(
                                context.Code,
                                new Uri(
                                    HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path)),
                                    webPortalcredentials,
                                    ApplicationConfiguration.ActiveDirectoryGraphEndPoint).Result;

                            // acquire a graph token to manage the user tenant
                            Uri serviceRoot = new Uri(new Uri(ApplicationConfiguration.ActiveDirectoryGraphEndPoint), userTenantId);
                            ActiveDirectoryClient userAdClient = new ActiveDirectoryClient(serviceRoot, async () => await Task.FromResult(userAuthResult.AccessToken));

                            // add the user roles to the claims
                            var userMemberships = userAdClient.Users.GetByObjectId(signedInUserObjectId).MemberOf.ExecuteAsync().Result;

                            foreach (var membership in userMemberships.CurrentPage)
                            {
                                DirectoryRole role = membership as DirectoryRole;

                                if (role != null)
                                {
                                    context.AuthenticationTicket.Identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role.DisplayName));
                                }
                            }

                            if (userTenantId != ApplicationConfiguration.ActiveDirectoryTenantId)
                            {
                                string partnerCenterCustomerId = string.Empty;

                                // Check to see if this login came from the tenant of a customer of the partner
                                var customerDetails = await ApplicationDomain.Instance.PartnerCenterClient.Customers.ById(userTenantId).GetAsync();

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

                                    throw new AuthorizationException(System.Net.HttpStatusCode.Unauthorized, Resources.NonAdminUnauthorizedMessage);
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
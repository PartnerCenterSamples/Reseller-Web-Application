// -----------------------------------------------------------------------
// <copyright file="AuthenticationProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Security
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using BusinessLogic;
    using Configuration;
    using Graph;
    using IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// Authentication provider for the Microsoft Graph service client.
    /// </summary>
    /// <seealso cref="IAuthenticationProvider" />
    public class AuthenticationProvider : IAuthenticationProvider
    {
        /// <summary>
        /// Name of the authentication header to be utilized. 
        /// </summary>
        private const string AuthHeaderName = "Authorization";

        /// <summary>
        /// The type of token being utilized for the authentication request.
        /// </summary>
        private const string TokenType = "Bearer";

        /// <summary>
        /// Provides the ability to request access tokens.
        /// </summary>
        private readonly IAccessTokenProvider tokenProvider;

        /// <summary>
        /// Address to return to upon receiving a response from the authority.
        /// </summary>
        private readonly Uri redirectUri;

        /// <summary>
        /// The authorization code received from service authorization endpoint.
        /// </summary>
        private readonly string authorizationCode;

        /// <summary>
        /// The customer identifier utilized to scope the Microsoft Graph requests.
        /// </summary>
        private readonly string customerId;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationProvider"/> class.
        /// </summary>
        /// <param name="customerId">Identifier for customer whose resources are being accessed.</param>
        /// <param name="authorizationCode">The authorization code received from service authorization endpoint.</param>
        /// <param name="redirectUri">Address to return to upon receiving a response from the authority.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="customerId"/> is empty or null.
        /// or 
        /// <paramref name="authorizationCode"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="redirectUri"/> is null.
        /// </exception>
        public AuthenticationProvider(string customerId, string authorizationCode, Uri redirectUri)
        {
            customerId.AssertNotEmpty(nameof(customerId));
            authorizationCode.AssertNotNull(nameof(authorizationCode));
            redirectUri.AssertNotNull(nameof(redirectUri));

            this.customerId = customerId;
            this.authorizationCode = authorizationCode;
            this.redirectUri = redirectUri;

            tokenProvider = new AccessTokenProvider();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationProvider"/> class.
        /// </summary>
        /// <param name="tokenProvider">Provides the ability to request access tokens.</param>
        /// <param name="customerId">Identifier for customer whose resources are being accessed.</param>
        /// <param name="authorizationCode">The authorization code received from service authorization endpoint.</param>
        /// <param name="redirectUri">Address to return to upon receiving a response from the authority.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="customerId"/> is empty or null.
        /// or 
        /// <paramref name="authorizationCode"/> is empty or null.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="tokenProvider"/> is null.
        /// or
        /// <paramref name="redirectUri"/> is null.
        /// </exception>
        public AuthenticationProvider(IAccessTokenProvider tokenProvider, string customerId, string authorizationCode, Uri redirectUri)
        {
            tokenProvider.AssertNotNull(nameof(tokenProvider));
            customerId.AssertNotEmpty(nameof(customerId));
            authorizationCode.AssertNotEmpty(nameof(authorizationCode));
            redirectUri.AssertNotNull(nameof(redirectUri));

            this.customerId = customerId;
            this.authorizationCode = authorizationCode;
            this.redirectUri = redirectUri;
            this.tokenProvider = tokenProvider;
        }

        /// <summary>
        /// Performs the necessary authentication and injects the required header.
        /// </summary>
        /// <param name="request">The request being made to the Microsoft Graph API.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            AuthenticationResult token = await tokenProvider.GetAccessTokenAsync(
                $"{ApplicationConfiguration.ActiveDirectoryEndPoint}{customerId}",
                authorizationCode,
                redirectUri,
                new ClientCredential(
                    ApplicationConfiguration.ActiveDirectoryClientID,
                    ApplicationConfiguration.ActiveDirectoryClientSecret),
                "https://graph.microsoft.com").ConfigureAwait(false);

            request.Headers.Add(AuthHeaderName, $"{TokenType} {token.AccessToken}");
        }
    }
}
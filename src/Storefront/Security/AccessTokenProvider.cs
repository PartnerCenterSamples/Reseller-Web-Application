// -----------------------------------------------------------------------
// <copyright file="AccessTokenProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Security
{
    using System;
    using System.Threading.Tasks;
    using IdentityModel.Clients.ActiveDirectory;

    public class AccessTokenProvider : IAccessTokenProvider
    {
        public async Task<AuthenticationResult> GetAccessTokenAsync(string authority, string authorizationCode, Uri redirectUri, ClientCredential clientCredential, string resource)
        {
            AuthenticationContext authContext;
            AuthenticationResult authResult;

            authority.AssertNotEmpty(nameof(authority));
            authorizationCode.AssertNotEmpty(nameof(authorizationCode));
            redirectUri.AssertNotNull(nameof(redirectUri));
            clientCredential.AssertNotNull(nameof(clientCredential));

            authContext = new AuthenticationContext(authority);
            authResult = await authContext.AcquireTokenByAuthorizationCodeAsync(
               authorizationCode,
               redirectUri,
               clientCredential,
               resource).ConfigureAwait(false);

            return authResult;
        }
    }
}
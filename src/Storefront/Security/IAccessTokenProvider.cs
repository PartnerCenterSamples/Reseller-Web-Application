// -----------------------------------------------------------------------
// <copyright file="IAccessTokenProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.Security
{
    using System;
    using System.Threading.Tasks;
    using IdentityModel.Clients.ActiveDirectory;

    public interface IAccessTokenProvider
    {
        Task<AuthenticationResult> GetAccessTokenAsync(string authority, string authorizationCode, Uri redirectUri, ClientCredential clientCredential, string resource);

    }
}
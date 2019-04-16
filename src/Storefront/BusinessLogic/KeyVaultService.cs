// -----------------------------------------------------------------------
// <copyright file="IVaultService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic
{
    using System;
    using System.Security;
    using System.Threading.Tasks;
    using Azure.KeyVault;
    using Azure.KeyVault.Models;
    using Azure.Services.AppAuthentication;

    public class KeyVaultService : IVaultService
    {
        /// <summary>
        /// Error code returned when a secret is not found in the vault.
        /// </summary>
        private const string NotFoundErrorCode = "SecretNotFound";

        /// <summary>
        /// The Azure KeyVault endpoint address.
        /// </summary>
        private readonly string endpoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyVaultService" /> class.
        /// </summary>
        /// <param name="endpoint">The Azure KeyVault endpoint address.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="endpoint"/> is empty or null.
        /// </exception>
        public KeyVaultService(string endpoint)
        {
            endpoint.AssertNotEmpty(nameof(endpoint));

            this.endpoint = endpoint;
        }

        /// <summary>
        /// Gets the specified entity from the vault. 
        /// </summary>
        /// <param name="identifier">Identifier of the entity to be retrieved.</param>
        /// <returns>The value retrieved from the vault.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="identifier"/> is empty or null.
        /// </exception>
        public async Task<SecureString> GetAsync(string identifier)
        {
            SecretBundle bundle;

            identifier.AssertNotEmpty(nameof(identifier));


            using (IKeyVaultClient client = GetAzureKeyVaultClient())
            {
                try
                {
                    bundle = await client.GetSecretAsync(endpoint, identifier).ConfigureAwait(false);
                }
                catch (KeyVaultErrorException ex)
                {
                    if (ex.Body.Error.Code.Equals(NotFoundErrorCode, StringComparison.CurrentCultureIgnoreCase))
                    {
                        bundle = null;
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return bundle?.Value.ToSecureString();
        }

        /// <summary>
        /// Gets an aptly configured instance of the <see cref="KeyVaultClient"/> class.
        /// </summary>
        /// <returns>An aptly populated instane of the <see cref="KeyVaultClient"/> class.</returns>
        private static KeyVaultClient GetAzureKeyVaultClient()
        {
            return new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback));
        }
    }
}
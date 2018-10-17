// -----------------------------------------------------------------------
// <copyright file="CachingService.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic
{
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using StackExchange.Redis;

    /// <summary>
    /// Facilitates caching frequently used objects to enhance the portal's performance.
    /// </summary>
    public class CachingService : DomainObject
    {
        /// <summary>
        /// The cache connection string.
        /// </summary>
        private readonly string cacheConnectionString;

        /// <summary>
        /// Indicates whether caching is enabled or not.
        /// </summary>
        private readonly bool isCashingEnabled;

        /// <summary>
        /// The cache connection.
        /// </summary>
        private IConnectionMultiplexer cacheConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingService"/> class.
        /// </summary>
        /// <param name="applicationDomain">An application domain instance.</param>
        /// <param name="cacheConnectionString">The cache connection string.</param>
        public CachingService(ApplicationDomain applicationDomain, string cacheConnectionString) : base(applicationDomain)
        {
            this.cacheConnectionString = cacheConnectionString;
            this.isCashingEnabled = !string.IsNullOrWhiteSpace(this.cacheConnectionString);
        }

        /// <summary>
        /// Stores an object in the cache.
        /// </summary>
        /// <typeparam name="TEntity">The object type.</typeparam>
        /// <param name="key">The object's key in the cache.</param>
        /// <param name="objectToCache">The object to cache.</param>
        /// <param name="expiresAfter">An optional expiry time.</param>
        /// <returns>A task.</returns>
        public async Task StoreAsync<TEntity>(string key, TEntity objectToCache, TimeSpan? expiresAfter = null)
        {
            key.AssertNotEmpty(nameof(key));
            objectToCache.AssertNotNull(nameof(objectToCache));

            if (isCashingEnabled)
            {
                IDatabase cache = await GetCacheReferenceAsync().ConfigureAwait(false);
                await cache.StringSetAsync(key, JsonConvert.SerializeObject(objectToCache), expiresAfter).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Retrieves an object from the cache.
        /// </summary>
        /// <typeparam name="TEntity">The object type.</typeparam>
        /// <param name="key">The object's key in the cache.</param>
        /// <returns>The object if found, null if not found.</returns>
        public async Task<TEntity> FetchAsync<TEntity>(string key) where TEntity : class
        {
            key.AssertNotEmpty(nameof(key));

            if (isCashingEnabled)
            {
                IDatabase cache = await GetCacheReferenceAsync().ConfigureAwait(false);
                RedisValue objectValue = await cache.StringGetAsync(key).ConfigureAwait(false);

                if (objectValue.IsNullOrEmpty)
                {
                    return null;
                }

                return JsonConvert.DeserializeObject<TEntity>(objectValue);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Removes an object from the cache.
        /// </summary>
        /// <param name="key">The object's key in the cache.</param>
        /// <returns>A task.</returns>
        public async Task ClearAsync(string key)
        {
            key.AssertNotEmpty(nameof(key));

            if (isCashingEnabled)
            {
                IDatabase cache = await GetCacheReferenceAsync().ConfigureAwait(false);
                await cache.KeyDeleteAsync(key).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Establishes connection with the cache.
        /// </summary>
        /// <returns>The cache reference.</returns>
        private async Task<IDatabase> GetCacheReferenceAsync()
        {
            if (!isCashingEnabled)
            {
                throw new InvalidOperationException("Caching is disabled");
            }

            if (cacheConnection == null)
            {
                cacheConnection = await ConnectionMultiplexer.ConnectAsync(cacheConnectionString).ConfigureAwait(false);
            }

            return cacheConnection.GetDatabase();
        }
    }
}
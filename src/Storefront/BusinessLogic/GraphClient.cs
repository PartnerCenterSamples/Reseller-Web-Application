// -----------------------------------------------------------------------
// <copyright file="GraphClient.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.Storefront.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Graph;
    using Models;
    using Security;

    /// <summary>
    /// Provides the ability to interact with the Microsoft Graph.
    /// </summary>
    /// <seealso cref="IGraphClient" />
    public class GraphClient : IGraphClient
    {
        /// <summary>
        /// Static instance of the <see cref="HttpProvider" /> class.
        /// </summary>
        private static HttpProvider httpProvider = new HttpProvider(new HttpClientHandler(), false);

        /// <summary>
        /// Provides access to the Microsoft Graph.
        /// </summary>
        private readonly IGraphServiceClient client;

        /// <summary>
        /// Identifier of the customer.
        /// </summary>
        private readonly string customerId;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphClient"/> class.
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
        /// </exception>"
        public GraphClient(string customerId, string authorizationCode, Uri redirectUri)
        {
            customerId.AssertNotEmpty(nameof(customerId));
            authorizationCode.AssertNotEmpty(nameof(authorizationCode));

            this.customerId = customerId;

            client = new GraphServiceClient(
                new AuthenticationProvider(
                    customerId,
                    authorizationCode,
                    redirectUri),
                httpProvider);
        }


        /// <summary>
        /// Gets a list of roles assigned to the specified object identifier.
        /// </summary>
        /// <param name="objectId">Object identifier for the object to be checked.</param>
        /// <returns>A list of roles that that are associated with the specified object identifier.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="objectId"/> is empty or null.
        /// </exception>
        public async Task<List<RoleModel>> GetDirectoryRolesAsync(string objectId)
        {
            DateTime executionTime;
            Dictionary<string, double> eventMeasurements;
            Dictionary<string, string> eventProperties;
            IUserMemberOfCollectionWithReferencesPage directoryGroups;
            List<RoleModel> roles;
            List<DirectoryRole> directoryRoles;
            bool morePages;

            objectId.AssertNotEmpty(nameof(objectId));

            executionTime = DateTime.Now;

            directoryGroups = await client.Users[objectId].MemberOf.Request().GetAsync().ConfigureAwait(false);
            roles = new List<RoleModel>();

            do
            {
                directoryRoles = directoryGroups.CurrentPage.OfType<DirectoryRole>().ToList();

                if (directoryRoles.Count > 0)
                {
                    roles.AddRange(directoryRoles.Select(r => new RoleModel
                    {
                        Description = r.Description,
                        DisplayName = r.DisplayName
                    }));
                }

                morePages = directoryGroups.NextPageRequest != null;

                if (morePages)
                {
                    directoryGroups = await directoryGroups.NextPageRequest.GetAsync().ConfigureAwait(false);
                }
            }
            while (morePages);

            // Capture the request for the customer summary for analysis.
            eventProperties = new Dictionary<string, string>
                {
                    { "CustomerId", customerId },
                    { "ObjectId", objectId }
                };

            // Track the event measurements for analysis.
            eventMeasurements = new Dictionary<string, double>
                {
                    { "ElapsedMilliseconds", DateTime.Now.Subtract(executionTime).TotalMilliseconds },
                    { "NumberOfRoles", roles.Count }
                };

            ApplicationDomain.Instance.TelemetryService.Provider.TrackEvent(nameof(GetDirectoryRolesAsync), eventProperties, eventMeasurements);

            return roles;
        }
    }
}
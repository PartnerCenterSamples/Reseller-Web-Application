// -----------------------------------------------------------------------
// <copyright file="TelemetryService.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.BusinessLogic
{
    using System.Threading.Tasks;
    using Telemetry;

    /// <summary>
    /// Provides the ability to capture telemetry for portal.
    /// </summary>
    public class TelemetryService : DomainObject
    {
        /// <summary>
        /// An instance of the  appropriate telemetry provider for the portal.
        /// </summary>
        private static ITelemetryProvider telemetryProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryService"/> class.
        /// </summary>
        /// <param name="applicationDomain">An application domain instance.</param>
        public TelemetryService(ApplicationDomain applicationDomain) : base(applicationDomain)
        {
        }

        /// <summary>
        /// Gets the instrumentation key from the portal configuration.
        /// </summary>
        public string InstrumentationKey { get; private set; }

        /// <summary>
        /// Gets an instance of the appropriate telemetry provider.
        /// </summary>
        public ITelemetryProvider Provider
        {
            get
            {
                if (string.IsNullOrEmpty(this.InstrumentationKey))
                {
                    telemetryProvider = new EmptyTelemetryProvider();
                }
                else
                {
                    telemetryProvider = new ApplicationInsightsTelemetryProvider();
                }

                return telemetryProvider;
            }
        }

        /// <summary>
        /// Initializes the telemetry provider based upon the portal configuration.
        /// </summary>
        /// <returns>A task for asynchronous purposes.</returns>
        public async Task InitializeAsync()
        {
            this.InstrumentationKey = (await ApplicationDomain.Instance.PortalBranding.RetrieveAsync()).InstrumentationKey;

            if (!string.IsNullOrEmpty(this.InstrumentationKey))
            {
                ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey =
                    this.InstrumentationKey;
            }
        }
    }
}
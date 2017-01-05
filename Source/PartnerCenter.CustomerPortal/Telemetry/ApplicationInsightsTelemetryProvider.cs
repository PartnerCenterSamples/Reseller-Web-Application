// -----------------------------------------------------------------------
// <copyright file="ApplicationInsightsTelemetryProvider.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Telemetry
{
    using System;
    using System.Collections.Generic;
    using ApplicationInsights;

    /// <summary>
    /// Provides the ability to capture telemetry data using Application Insights.
    /// </summary>
    public class ApplicationInsightsTelemetryProvider : ITelemetryProvider
    {
        /// <summary>
        /// Used to log telemetry to Application Insights.
        /// </summary>
        private readonly TelemetryClient telemetry;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationInsightsTelemetryProvider" /> class.
        /// </summary>
        public ApplicationInsightsTelemetryProvider()
        {
            this.telemetry = new TelemetryClient();
        }

        /// <summary>
        /// Sends an event for display in the diagnostic search and aggregation in the metrics explorer.
        /// </summary>
        /// <param name="eventName">A name for the event.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        /// <param name="metrics">Measurements associated with this event.</param>
        public void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            this.telemetry.TrackEvent(eventName, properties, metrics);
        }

        /// <summary>
        /// Sends an exception for display the in diagnostic search.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="properties">Named string values you can use to classify and search for this exception.</param>
        /// <param name="metrics">Additional values associated with this exception.</param>
        public void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            this.telemetry.TrackException(exception, properties, metrics);
        }

        /// <summary>
        /// Send a trace message for display in the diagnostic search.
        /// </summary>
        /// <param name="message">The message to display</param>
        public void TrackTrace(string message)
        {
            this.telemetry.TrackTrace(message);
        }
    }
}
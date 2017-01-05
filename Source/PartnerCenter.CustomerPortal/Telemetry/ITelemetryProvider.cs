// -----------------------------------------------------------------------
// <copyright file="ITelemetryProvider.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Store.PartnerCenter.CustomerPortal.Telemetry
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a telemetry provider utilized capturing and logging data.
    /// </summary>
    public interface ITelemetryProvider
    {
        /// <summary>
        /// Sends an event for display in the diagnostic search and aggregation in the metrics explorer.
        /// </summary>
        /// <param name="eventName">A name for the event.</param>
        /// <param name="properties">Named string values you can use to search and classify events.</param>
        /// <param name="metrics">Measurements associated with this event.</param>
        void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null);

        /// <summary>
        /// Sends an exception for display the in diagnostic search.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="properties">Named string values you can use to classify and search for this exception.</param>
        /// <param name="metrics">Additional values associated with this exception.</param>
        void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null);

        /// <summary>
        /// Send a trace message for display in the diagnostic search.
        /// </summary>
        /// <param name="message">The message to display</param>
        void TrackTrace(string message);
    }
}
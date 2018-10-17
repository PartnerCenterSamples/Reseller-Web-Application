/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Infrastructure.Diagnostics = function (webPortal) {
    /// <summary>
    /// Provides logging to the browser and batched logging to the configured endpoint. Currently supported logging
    /// levels are: info, warning and error.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>

    if (!webPortal) {
        throw new Error("Microsoft.WebPortal.Infrastructure.Diagnostics.Constructor: Invalid web portal instance.");
    }

    this.webPortal = webPortal;

    this.information = function (message) {
        /// <summary>
        /// Logs a piece of information. This will write to the browser log and queue the message to be sent to the configured endpoint.
        /// </summary>
        /// <param name="message">The message to log.</param>

        this.informationLocal(message);
        this.informationRemote(message);
    }

    /*
        Logs a piece of information to the browser's log.

        @Param message The message to log.
    */
    this.informationLocal = function (message) {
        /// <summary>
        /// Logs a piece of information to the browser's log.
        /// </summary>
        /// <param name="message">The message to log.</param>

        this.logLocal(message, this.webPortal.Configuration.Diagnostics.Level.Info);
    }

    this.informationRemote = function (message) {
        /// <summary>
        /// Queues an information message to be sent to the configured endpoint.
        /// </summary>
        /// <param name="message">The message to log.</param>

        this.logRemote(message, this.webPortal.Configuration.Diagnostics.Level.Info);
    }

    this.warning = function (message) {
        /// <summary>
        /// Logs a warning. This will write to the browser log and queue the message to be sent to the configured endpoint.
        /// </summary>
        /// <param name="message">The warning message to log.</param>

        this.warningLocal(message);
        this.warningRemote(message);
    }

    this.warningLocal = function (message) {
        /// <summary>
        /// Logs a warning to the browser's log.
        /// </summary>
        /// <param name="message">The warning message to log.</param>

        this.logLocal(message, this.webPortal.Configuration.Diagnostics.Level.Warning);
    }

    this.warningRemote = function (message) {
        /// <summary>
        /// Queues a warning message to be sent to the configured endpoint.
        /// </summary>
        /// <param name="message">The warning message to log.</param>

        this.logRemote(message, this.webPortal.Configuration.Diagnostics.Level.Warning);
    }

    this.error = function (message) {
        /// <summary>
        /// Logs an error. This will write to the browser log and queue the message to be sent to the configured endpoint.
        /// </summary>
        /// <param name="message">The error message to log.</param>

        this.errorLocal(message);
        this.errorRemote(message);
    }

    this.errorLocal = function (message) {
        /// <summary>
        /// Logs an error to the browser's log.
        /// </summary>
        /// <param name="message">The error message to log.</param>

        this.logLocal(message, this.webPortal.Configuration.Diagnostics.Level.Error);
    }

    this.errorRemote = function (message) {
        /// <summary>
        /// Queues an error message to be sent to the configured endpoint.
        /// </summary>
        /// <param name="message">The error message to log.</param>

        this.logRemote(message, this.webPortal.Configuration.Diagnostics.Level.Error);
    }

    this.logLocal = function (message, level) {
        /// <summary>
        /// Logs a message to the browser's log.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="level">The logging level. Use Microsoft.WebPortal.Configuration.Diagnostics.Level enumeration. Default is Info.</param>

        if (!message) {
            throw new Error("Please provide a message to log.");
        }

        level = level || this.webPortal.Configuration.Diagnostics.Level.Info;
        message = level + ": " + message;

        try {
            // try logging normally
            console.log(message);
        } catch (exception) {
            try {
                // try opera API
                opera.postError(message);
            }
            catch (operaException) {
                // there is nothing we can do now, this browser does not support logging
            }
        }
    }

    this.logRemote = function (message, level) {
        /// <summary>
        /// Batches messages to be sent to the configured endpoint.
        /// </summary>
        /// <param name="message">The message to queue.</param>
        /// <param name="level">The logging level. Use Microsoft.WebPortal.Configuration.Diagnostics.Level enumeration. Default is Info</param>

        // TODO: when implemented, this should use a buffer that has a max size which gets flushed regularily to the server.
        // We don't want to hit the server for each message and at the same time we do not want to overload the browser's memory!
        // Please make sure the flush interval and the maximum buffer size (in terms of message count) is configurable through
        // this.webPortal.Configuration class.
    }
}
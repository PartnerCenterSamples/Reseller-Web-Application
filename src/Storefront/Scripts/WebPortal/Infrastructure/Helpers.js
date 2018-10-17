/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Infrastructure.Helpers = function (webPortal) {
    /// <summary>
    /// Contains common functionality used across the web portal.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>

    if (!webPortal) {
        throw new Error("Microsoft.WebPortal.Infrastructure.Helpers.Constructor: Invalid web portal instance.");
    }

    this.webPortal = webPortal;
}

Microsoft.WebPortal.Infrastructure.Helpers.prototype.throwIfNotSet = function (argument, argumentName, fullyQualifiedMethod) {
    /// <summary>
    /// Checks if the given argument is set or not. Throws an error and logs an error if not set.
    /// </summary>
    /// <param name="argument">The argument value.</param>
    /// <param name="argumentName">The argumnet name to use in the error and the logs.</param>
    /// <param name="fullyQualifiedMethod">Used in the logs to indicate the method that had the problem.
    /// e.g: Microsoft.WebPortal.AsyncOperationSerializer.queue</param>

    if (!argument) {
        argumentName = argumentName || "Unknown argument";
        fullyQualifiedMethod = fullyQualifiedMethod || "Unknown method";
        this.webPortal.Diagnostics.error(fullyQualifiedMethod + ": " + argumentName + " is not set.");
        throw new Error(argumentName + " must be set.");
    }
}

Microsoft.WebPortal.Infrastructure.Helpers.prototype.displayRetryCancelErrorNotification = function (notification, errorMessage, retryMessage, retryCallback, cancelCallback) {
    /// <summary>
    /// Displays an error notification with retry and cancel buttons. Handles retries and reuses the same notification to display retry progress.
    /// </summary>
    /// <param name="notification">An optional notification object. The method creates a new notification if nothing is passed or reuses the passed notification.</param>
    /// <param name="errorMessage">The error message to display in the notification.</param>
    /// <param name="retryMessage">The message to display when the user hits the retry button.</param>
    /// <param name="retryCallback">The function to call when the user hits the retry button.</param>
    /// <param name="cancelCallback">The function to call when the user hits the cancel button.</param>

    // TODO: Review this

    var retryButton = Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.RETRY, "RetryButton",
        function (button, notification, event) {
            // re-attempt, convert this error notification into a progress one
            notification.type(Microsoft.WebPortal.Services.Notification.NotificationType.Progress);
            notification.message(retryMessage);
            notification.buttons([]);

            if (event) {
                event.stopPropagation();
            }

            retryCallback(notification)
        });

    var cancelButton = Microsoft.WebPortal.Services.Button.create(
        Microsoft.WebPortal.Services.Button.StandardButtons.CANCEL, "CancelButton",
        function (button, notification) {
            notification.dismiss();

            if (cancelCallback) {
                cancelCallback(button, notification);
            }
        }
    );

    if (!notification) {
        // create a new notification if this is the first failure
        notification = new Microsoft.WebPortal.Services.Notification(
            Microsoft.WebPortal.Services.Notification.NotificationType.Error, errorMessage, [retryButton, cancelButton])

        this.webPortal.Services.Notifications.add(notification);
    } else {
        // recycle the same notification
        notification.type(Microsoft.WebPortal.Services.Notification.NotificationType.Error);
        notification.message(errorMessage);
        notification.buttons([retryButton, cancelButton]);
    }
}

Microsoft.WebPortal.Infrastructure.Helpers.prototype.random = function (min, max) {
    /// <summary>
    /// Generates a random integer.
    /// </summary>
    /// <param name="min">The minimum number to generate. Default is 0.</param>
    /// <param name="max">The maximum number to generate. Default is 100000000.</param>
    /// <returns type="Integer">A random number in the given range.</returns>

    min = min || 0;
    max = max || 100000000;

    return Math.floor(Math.random() * (max - min + 1)) + min;
}

Microsoft.WebPortal.Infrastructure.Helpers.prototype.getBaseUri = function () {
    /// <summary>
    /// Return portal base uri.
    /// </summary>
    /// <returns type="String">The portal URI.</returns>

    if (window.location.href.indexOf("#") > 0) {
        return window.location.href.split("#")[0];
    }

    return window.location.href;
}

Microsoft.WebPortal.Infrastructure.Helpers.prototype.inherit = function (subclass, baseClass) {
    /// <summary>
    /// Performs prototypal inheritance.
    /// </summary>
    /// <param name="subclass">The sub class.</param>
    /// <param name="baseClass">The base class.</param>

    subclass.prototype = Object.create(baseClass.prototype);
    subclass.prototype.constructor = subclass;
    subclass.prototype.base = baseClass.prototype;
}

Microsoft.WebPortal.Infrastructure.Helpers.prototype.throttle = function (targetFunction, threshold, scope) {
    /// <summary>
    /// Throttles calling a function using the given threshold. Returns the throttling function.
    /// </summary>
    /// <param name="targetFunction">The function to be throttled.</param>
    /// <param name="threshold">The period in milliseconds by which the calls are clipped.</param>
    /// <param name="scope">The 'this' pointer passed to the target function.</param>

    threshold || (threshold = this.webPortal.Configuration.DefaultThrottlingDuration);
    var last = null, deferTimer = null;

    return function () {
        var context = scope || this;
        clearTimeout(deferTimer);

        deferTimer = setTimeout(function () {
            targetFunction.apply(context, arguments);
        }, threshold);
    };
}

Microsoft.WebPortal.Infrastructure.Helpers.prototype.ajaxCall = function (url, method, data, contentType, timeout) {
    /// <summary>
    /// Builds an AJAX request function.
    /// </summary>
    /// <param name="url">The server end point to hit. Mandatory.</param>
    /// <param name="method">The HTTP method to use. Default is POST. Use one of Microsoft.WebPortal.HttpMethod values.</param>
    /// <param name="data">The request data. Optional. If the content type is set to JSON then the data will be automatically serialized to JSON.</param>
    /// <param name="contentType">The content type. Optional. Use one of Microsoft.WebPortal.ContentType values.</param>
    /// <param name="timeout">Optional timeout. Default is applied if not provided.</param>

    // the url is mandatory
    this.throwIfNotSet(url, "url", "Microsoft.WebPortal.Infrastructure.Helpers.ajaxCall");

    // default the http method to POST if not provided
    method = method || Microsoft.WebPortal.HttpMethod.Post;

    // set the timeout to the default timeout value if not provided
    timeout = timeout || this.webPortal.Configuration.Timeout.Default;

    // ensure timeout is within the allowed range
    timeout = Math.min(timeout, this.webPortal.Configuration.Timeout.Max);
    timeout = Math.max(timeout, this.webPortal.Configuration.Timeout.Min);

    // build the AJAX request object
    var ajaxRequest = {
        url: encodeURI(url.trim()),
        type: method,
        timeout: timeout
    };

    if (contentType) {
        // configure the content type if provided
        switch (contentType) {
            case Microsoft.WebPortal.ContentType.Json:
                ajaxRequest.contentdataType = "json";
                ajaxRequest.contentType = "application/json; charset=utf-8";
                break;
            default:
                this.webPortal.Diagnostics.error("Microsoft.WebPortal.Infrastructure.Helpers.ajaxCall: Unsupported content type.");
                throw new Error("Unsupported content type. Please refer to Microsoft.WebPortal.ContentType.");
        }
    }

    if (data) {
        // set the data if provided, automatically serialize it to JSON if the content type was JSON
        ajaxRequest.data = (contentType === Microsoft.WebPortal.ContentType.Json) ? JSON.stringify(data) : data;
    }

    // return a function that when invoked, issues the configured AJAX request
    return function () {
        return $.ajax(ajaxRequest);
    }
}
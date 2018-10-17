/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Utilities.Throttler = function (throttlingPeriod, resourceName) {
    /// <summary>
    /// Throttles access to resources based on a given period. Any incoming requests within the throttling period since the last request
    /// will be denied.
    /// </summary>
    /// <param name="throttlingPeriod">The period in which to deny incoming requests after accepting a request.</param>
    /// <param name="resourceName">The resource name used in logging.</param>

    this.reset();

    this.throttlingPeriod = throttlingPeriod || $WebPortal.Configuration.DefaultThrottlingDuration;
    this.resourceName = resourceName || "Unknown resource";
}

Microsoft.WebPortal.Utilities.Throttler.prototype.throttle = function (callbackFunction, callbackObject) {
    /// <summary>
    /// Throttles incoming requests.
    /// </summary>
    /// <param name="callbackFunction">The function to invoke. You can pass by optional arguments to the callback function after the first 2 arguments.
    /// e.g. tp call your function and send it 1 and "Hello" as arguments you can do something like: throttler.throttle(yourFunction, yourObject, 1, "Hello");
    /// </param>
    /// <param name="callbackObject">The object on which to invoke the function. Optional.</param>

    $WebPortal.Helpers.throwIfNotSet(callbackFunction, "callbackFunction", "Microsoft.WebPortal.Utilities.Throttler.throttle");

    var currentTime = new Date();

    if (currentTime - this.lastProcessedRequestTimeStamp < this.throttlingPeriod) {
        $WebPortal.Diagnostics.warningLocal(this.resourceName + ": throttled an incoming request.");
    } else {
        this.lastProcessedRequestTimeStamp = currentTime;

        // convert the arguments into an array
        var args = Array.prototype.slice.call(arguments);

        // call the function and pass the rest of the arguments to it
        return callbackFunction.call(callbackObject, args.slice(2));
    }
}

Microsoft.WebPortal.Utilities.Throttler.prototype.reset = function () {
    /// <summary>
    /// Resets the throttler.
    /// </summary>

    // reset the time stamp of the last request accepted
    this.lastProcessedRequestTimeStamp = new Date(1983, 8, 8);
}

//@ sourceURL=Throttler.js
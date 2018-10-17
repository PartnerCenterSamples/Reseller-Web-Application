/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Core.ServerCallManager = function (webPortal) {
    /// <summary>
    /// Manages the creation and destruction of retryable server calls. This class will automically cancel any pending AJAX calls
    /// if their features (presenters) are no longer active. Developers should use this class instead of manually creating 
    /// RetryableServerCall objects if they want to limit their calls to the current active feature. This is useful in scenarios where
    /// the UI needs to be updated for instance. If you choose manual creation, then you will be responsible for checking that your feature is still
    /// active when the AJAX call returns. Manual creation is useful is the call is not tied to a feature or in case the call was a server update/delete/insert which
    /// does not affect the UI.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>

    if(!webPortal) {
        throw new Error("Microsoft.WebPortal.Core.ServerCallManager.Constructor: Invalid web portal instance.");
    }

    this.webPortal = webPortal;

    // a hash table that tracks the active AJAX calls for each feature
    this.featureServerOperations = {};

    // listen to feature deactivate and destroy events so that we cancel their pending AJAX calls
    this.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.FeatureDeactivated, this._onFeatureDeactivated, this);
    this.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.FeatureDestroyed, this._onFeatureDestroyed, this);
}

Microsoft.WebPortal.Core.ServerCallManager.prototype.create = function (feature, operation, name, retryPolicy) {
    /// <summary>
    /// Creates a RetryableServerCall to be used by callers. This server call will be automatically cancelled if the feature it
    /// belongs to is no longer active.
    /// </summary>
    /// <param name="feature">The feature this operation belongs to. Use the Microsoft.WebPortal.Feature enumeration.</param>
    /// <param name="operation">A function that executes a server call. $WebPortal.Helpers.ajaxCall() to create Ajax calls.</param>
    /// <param name="name">The server call name. Used for logging purposes. Optional.</param>
    /// <param name="retryPolicy">An optional retry policy. Pass an array of integers to specify the retry back off intervals in milliseconds.</param>
    /// <returns type="Microsoft.WebPortal.Utilities.RetryableServerCall">A configured and managed RetryableServerCall object ready to execute.</returns>

    this.webPortal.Helpers.throwIfNotSet(feature, "feature", "Microsoft.WebPortal.Core.ServerCallManager.create");
    this.webPortal.Helpers.throwIfNotSet(operation, "operation", "Microsoft.WebPortal.Core.ServerCallManager.create");

    var serverOperation = new Microsoft.WebPortal.Utilities.RetryableServerCall(operation, name, retryPolicy);

    if (!this.featureServerOperations[feature.name]) {
        // the feature does not have any ongoing server operations, initialize an empty array for those to come
        this.featureServerOperations[feature.name] = [];
    }

    // add the server operation to the feature hash table
    this.featureServerOperations[feature.name].push(serverOperation);

    this.webPortal.Diagnostics.information("ServerCallManager: created server operation for: " + feature.name + "." + serverOperation.name);

    return serverOperation;
}

Microsoft.WebPortal.Core.ServerCallManager.prototype.cleanup = function () {
    /// <summary>
    /// Cancels and cleans up all server operations.
    /// </summary>

    for (feature in this.featureServerOperations) {
        this._cancelFeatureRequests(feature, true);
    }
}

Microsoft.WebPortal.Core.ServerCallManager.prototype._onFeatureDeactivated = function(eventId, context, broadcaster) {
    /// <summary>
    /// Called when a feature has been deactivated.
    /// </summary>
    /// <param name="eventId">The event Id.</param>
    /// <param name="context">The context parameter.</param>
    /// <param name="broadcaster">The event broadcaster.</param>

    if (context && context.Feature) {
        this.webPortal.Diagnostics.information("ServerCallManager: " + context.Feature.name + " deactivated. Cancelling feature Ajax requests.");

        // cancel the feature's pending server calls
        this._cancelFeatureRequests(context.Feature);
    } else {
        this.webPortal.Diagnostics.warning("Microsoft.WebPortal.Core.ServerCallManager._onFeatureDeactivated: received a null feature. Doing nothing.");
    }
}

Microsoft.WebPortal.Core.ServerCallManager.prototype._onFeatureDestroyed = function(eventId, context, broadcaster) {
    /// <summary>
    /// Called when a feature has been destroyed.
    /// </summary>
    /// <param name="eventId">The event Id.</param>
    /// <param name="context">The context parameter.</param>
    /// <param name="broadcaster">The event broadcaster.</param>

    if (context && context.Feature) {
        this.webPortal.Diagnostics.information("ServerCallManager: " + context.Feature.name + " destroyed. Cancelling feature Ajax requests.");

        // cancel the feature's pending server calls and remove them from the hash table
        this._cancelFeatureRequests(context.Feature, true);
    } else {
        this.webPortal.Diagnostics.warning("Microsoft.WebPortal.Core.ServerCallManager._onFeatureDestroyed: received a null feature. Doing nothing.");
    }
}

Microsoft.WebPortal.Core.ServerCallManager.prototype._cancelFeatureRequests = function (feature, cleanUp) {
    /// <summary>
    /// Private method. Cancels any pending server calls for a features and optionally unmanages these server calls.
    /// </summary>
    /// <param name="feature">The feature to cancel its pending server calls.</param>
    /// <param name="cleanUp">Optional. Defaults to false. Specify true to remove the server calls from the feature's cache. i.e. unmanage them.</param>
    
    if (this.featureServerOperations[feature]) {
        // cancel all the feature's server calls
        for (var i = 0; i < this.featureServerOperations[feature].length; ++i) {
            this.featureServerOperations[feature][i].cancel();
        }

        if (removeReferences === true) {
            // clean up the feature operations if requested
            this.featureServerOperations[feature].length = 0;
        }
    }
}

//@ sourceURL=ServerCallManager.js
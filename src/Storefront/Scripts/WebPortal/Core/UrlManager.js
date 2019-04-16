Microsoft.WebPortal.Core.UrlManager = function (webPortal) {
    /// <summary>
    /// This class manages relative URLs assigned to features. When a feature is activated, this class updates the url with the feature name
    /// and any context that was sent to that feature. This class also handles browser back and forward buttons and applies the correct journey action.
    /// It also handles navigating to a sub Url directly, i.e. passing the URL to another user, that user can navigate to that Url directly.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>

    if (!webPortal) {
        throw new Error("Microsoft.WebPortal.Core.UrlManager.Constructor: Invalid web portal instance.");
    }

    this.webPortal = webPortal;

    // let's get notified when the portal has finished intializing as well as when every feature gets activated
    this.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.PortalInitialized, this.onPortalInitialized, this);
    this.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.FeatureActivated, this.onFeatureActivated, this);

    this.currentActiveFeatureIndex = 0;
}

Microsoft.WebPortal.Core.UrlManager.prototype.onPortalInitialized = function () {
    /// <summary>
    /// Invoked when the portal is initialized.
    /// </summary>

    // we will start the default feature on the default tile by default
    var featureToInvoke = Microsoft.WebPortal.Feature[this.webPortal.activeTile().DefaultFeature];
    var contextToPass = null;

    if (window.location.hash && window.location.hash.length > 0) {
        // a hash is specified in the URL, extract the feature and any context object from the URL
        var featureHash = window.location.hash.slice(1).split("?");        
        featureToInvoke = Microsoft.WebPortal.Feature[featureHash[0]] || featureToInvoke;
        try {
            featureHash = window.location.hash.slice(1).split("?Context=");
            contextToPass = featureHash.length > 1 ? JSON.parse(decodeURI(featureHash[1])) : contextToPass;
        }
        catch (e) {
            contextToPass = null;
        }
    }

    // start a journey with the feature we just determined
    this.webPortal.Journey.start(featureToInvoke, contextToPass);

    var self = this;

    window.onpopstate = function (event) {
        /// <summary>
        /// Called when the browser history is changed.
        /// </summary>
        /// <param name="event">The event object.</param>

        self.historyInitiatedNavigation = true;

        if (!event.state) {
            // This happens on when returning to the first history page, for some reason, FF does not set the state, rectract by one step
            // as a mitigation
            if (!self.webPortal.Journey.retract()) {
                // could not retract, start a new journey with the feature name and context extracted from the URL
                var queryStringIndex = window.location.hash.indexOf("?");
                var featureName = window.location.hash.slice(1, queryStringIndex == -1 ? undefined : queryStringIndex);
                
                var context = null;
                try {
                    context = queryStringIndex == -1 ? null : JSON.parse(decodeURI(window.location.hash.slice(queryStringIndex)));
                }
                catch (e) {
                    context = null;
                }

                if (self.webPortal.Journey.journey()[self.webPortal.Journey.journey().length - 1].feature.name != featureName) {
                    self.webPortal.Journey.start(Microsoft.WebPortal.Feature[featureName], context);
                } else {
                    // Chrome fires a pop state event when the first page is loaded. In this case, we do not want to start a new journey. This
                    // should NOT be considered as a history navigation.
                    self.historyInitiatedNavigation = false;
                }
            }

            self.currentActiveFeatureIndex = 1;
            return;
        }

        // serialize the context into JSON if any
        var context = event.state.context ? JSON.parse(event.state.context) : null;
        
        if (self.currentActiveFeatureIndex > event.state.index) {
            // the user is trying to navigate backwards, try retracting within the current journey
            var retracted = self.webPortal.Journey.retract(self.currentActiveFeatureIndex - event.state.index, context);

            if (!retracted) {
                // retract was not performed due to out of range index, start a new journey instead
                self.webPortal.Journey.start(event.state.feature, context);
            }
        } else {
            // this is a forward request
            if (event.state.isJourneyHead) {
                // this feature was the start of a new journey
                self.webPortal.Journey.start(event.state.feature, context);
            } else {
                // this feature lived inside a journey, advance to it within the current journey
                self.webPortal.Journey.advance(event.state.feature, context);
            }
        }

        self.currentActiveFeatureIndex = event.state.index;
    }
}

Microsoft.WebPortal.Core.UrlManager.prototype.onFeatureActivated = function (event, featureInformation) {
    /// <summary>
    /// Invoked whenever a feature gets activated.
    /// </summary>
    /// <param name="event">The event Id.</param>
    /// <param name="featureInformation">The activated feature information.</param>

    if (this.historyInitiatedNavigation == true) {
        // this activation has been triggered due to a browser history navigation, we do not want to modify the history entries
        this.historyInitiatedNavigation = false;
        return;
    }

    // the feature state object will be stored in the history and will be useful in reconstructing feature presenters upon
    // the user clicking back and forward in the browser
    var featureState = {
        index: this.currentActiveFeatureIndex + 1,
        context: JSON.stringify(featureInformation.context),
        feature: featureInformation.feature, 
        isJourneyHead: this.webPortal.Journey.journey().length < 2
    }

    // let's build the hash Url path for the feature
    var hashUrlPath = "#" + featureInformation.feature.name;

    if (featureInformation.context) {
        // add the context object if provided
        hashUrlPath += "?Context=" + featureState.context;
    }

    if (this.currentActiveFeatureIndex == 0) {
        // this is the initial feature load, modify the initial history state
        window.history.replaceState(featureState, null, hashUrlPath);
    } else {
        // push a new history state for this feature
        window.history.pushState(featureState, null, hashUrlPath);
    }

    this.currentActiveFeatureIndex++;
}
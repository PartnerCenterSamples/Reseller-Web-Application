/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Core.Journey = function (webPortal) {
    /// <summary>
    /// This class manages the sequence of features the users engage with in the portal. The user can go back to previous
    /// steps he was in or start a whole new journey.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>

    if (!webPortal) {
        throw new Error("Microsoft.WebPortal.Core.Journey.Constructor: Invalid web portal instance.");
    }

    this.webPortal = webPortal;
    this.journey = ko.observableArray([]);

    this.journeyStartAnimation = this.webPortal.Configuration.Journey.getJourneyStartAnimation();
    this.journeyAdvanceAnimation = this.webPortal.Configuration.Journey.getJourneyAdvanceAnimation();
    this.journeyRetractAnimation = this.webPortal.Configuration.Journey.getJourneyRetractAnimation();

    // journey operations will be throttled in order to protect it from excessive navigation requests due to double clicking for instance
    this.throttler = new Microsoft.WebPortal.Utilities.Throttler(700, "Journey");
}

Microsoft.WebPortal.Core.Journey.prototype.start = function (feature, context) {
    /// <summary>
    /// Starts a new journey. All exisiting journey features will be destroyed.
    /// </summary>
    /// <param name="feature">The feature to activate. Use Microsoft.WebPortal.Features enum.</param>
    /// <param name="context">A context object to pass to the feature presenter.</param>

    this.throttler.throttle(function () {
        this.webPortal.Helpers.throwIfNotSet(feature, "feature", "Microsoft.WebPortal.Core.Journey.start");

        // grab the feature's presenter
        var presenterClass = this.webPortal.getFeaturePresenter(feature);

        // set the content panel effect
        this.webPortal.ContentPanel.setAnimation(this.journeyStartAnimation);

        var journey = this.journey();
        var self = this;

        this.webPortal.ContentPanel.clear($.Deferred().done(function () {
            // destroy the current journey's presenters in reverse order
            for (var i = journey.length - 1; i >= 0; --i) {
                journey[i].destroy(context);
            }

            // cleanup existing journey
            self.journey.removeAll();

            // create the feature presenter
            var presenter = new presenterClass(self.webPortal, feature, context);

            // add the new new feature presenter to the journey
            self.journey.push(presenter);

            // activate the presenter
            presenter.activate(context);
        }));
    }, this);
}

Microsoft.WebPortal.Core.Journey.prototype.advance = function (feature, context) {
    /// <summary>
    /// Advances the journey to a new feature. Keeps the current journey features in memory.
    /// </summary>
    /// <param name="feature">The feature to activate.</param>
    /// <param name="context">A context object to pass to the feature presenter.</param>

    this.throttler.throttle(function () {
        this.webPortal.Helpers.throwIfNotSet(feature, "feature", "Microsoft.WebPortal.Core.Journey.advance");

        // create the feature presenter
        var presenterClass = this.webPortal.getFeaturePresenter(feature);
        var presenter = new presenterClass(this.webPortal, feature, context);

        var journey = this.journey();

        if (journey.length > 0) {
            this.webPortal.ContentPanel.setAnimation(this.journeyAdvanceAnimation);
            var self = this;

            this.webPortal.ContentPanel.clear($.Deferred().done(function () {
                // deactivate the current presenter
                journey[journey.length - 1].deactivate(context);

                // add the new feature presenter
                self.journey.push(presenter);

                // load the presenter
                presenter.activate(context);
            }));
        }
    }, this);
}

Microsoft.WebPortal.Core.Journey.prototype.retract = function (index, context) {
    /// <summary>
    /// Steps back in the current journey. Destroys unneeded features along the way.
    /// </summary>
    /// <param name="index">The index of the feature to rectract to in the journey. If not provided, the journey rectracts one step back.</param>
    /// <param name="context">A context object to pass to the feature presenter.</param>

    if (index === null || index === undefined) {
        // if no index was set, retract one step back
        index = this.journey().length - 2;
    }

    if (index < 0 || index >= this.journey().length - 1) {
        // we can't retract forward or go negative
        this.webPortal.Diagnostics.warningLocal("Microsoft.WebPortal.Core.Journey.retract: invalid index provided: " + index);
        return false;
    }

    return this.throttler.throttle(function () {
        var journey = this.journey();

        this.webPortal.ContentPanel.setAnimation(this.journeyRetractAnimation);
        var self = this;

        this.webPortal.ContentPanel.clear($.Deferred().done(function () {
            // destroy the current journey's presenters in reverse order
            for (var i = journey.length - 1; i > index; --i) {
                journey[journey.length - 1].destroy(context);
                self.journey.pop();
            }

            // activate the tail of the journey
            var featurePresenterToReactivate = journey[journey.length - 1];
            featurePresenterToReactivate.activate(context);
        }));

        return true;
    }, this);
}

//@ sourceURL=Journey.js
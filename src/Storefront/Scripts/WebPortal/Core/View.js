Microsoft.WebPortal.Core.View = function (webPortal, elementSelector, isShown, childViews, animation) {
    /// <summary>
    /// The base view class. All views should inherit from this class. A view is an abstraction that shows or hides UX to users. It maintains the data and the view
    /// state and takes care of disposing the resources when it is no longer needed. One or more views are typically utilized by a presenter. Views also support hierarchies,
    /// a view can be composed of a number of child views that will be shown/rendered/hidden with the parent view.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="elementSelector">The JQuery selector for the HTML element to be managed by this view.</param>
    /// <param name="isShown">The initial show state of the view. Default is false for hidden. Specify true to set it to shown.</param>
    /// <param name="childViews">An Optional array of child views.</param>
    /// <param name="animation">The animation to use when showing or hiding the view. The default is in the portal configuration.</param>

    if (!webPortal) {
        throw new Error("Microsoft.WebPortal.Core.View.constructor: Invalid web portal instance.");
    }

    this.webPortal = webPortal;

    this.webPortal.Helpers.throwIfNotSet(elementSelector, "elementSelector", "Microsoft.WebPortal.Core.View.constructor");
    this.elementSelector = elementSelector;

    this.childViews = ko.observableArray(childViews || []);
    this.animation = animation || this.webPortal.Configuration.getDefaultViewAnimation();

    // generate the view id
    this.id = "view" + this.webPortal.Helpers.random();

    // the state of the view
    this.state = Microsoft.WebPortal.Core.View.State.Created;

    // the view will be togglable
    Microsoft.WebPortal.Utilities.Toggler.injectToggling(this, this._show, this._hide, isShown);
};

Microsoft.WebPortal.Core.View.prototype.render = function () {
    /// <summary>
    /// Renders the view.
    /// </summary>

    if (this.onRender) {
        // call the on render hook
        this.onRender();
    }

    // render the child views
    for (var i in this.childViews()) {
        this.childViews()[i].render();
    }

    this.state = Microsoft.WebPortal.Core.View.State.Rendered;
};

Microsoft.WebPortal.Core.View.prototype._show = function (showProgress) {
    /// <summary>
    /// Private method. This will be called by the toggler. Clients of the view should use the show() method which returns a JQuery deferred object
    /// which gets notified once showing is complete. This method will show the view using the configured animation. This will also show the child views.
    /// The sub class can provide an onShowing(true) method that gets called before the view is shown and an onShown(true) method that is called once showing is complete.
    /// </summary>
    /// <param name="showProgress">A Jquery deferred object to resolve when showing is complete.</param>

    if (!this.isRendered()) {
        // render the view first
        this.render();
    }

    if (this.onShowing) {
        // call the on showing hook to perform any logic needed before executing the show animation
        this.onShowing(true);
    }

    var self = this;

    // show the view
    this.animation.show(this.elementSelector).always(function () {
        // once parent is shown, show its child views
        var showingProgressArray = [];

        for (var i in self.childViews()) {
            showingProgressArray.push(self.childViews()[i].show());
        }

        $.when(showingProgressArray).then(function () {
            // show is complete
            showProgress.resolve();

            // all children are shown, we are done
            if (self.onShown) {
                // call the on shown hook to perform any custom post show logic
                self.onShown(true);
            }
        });
    });
};

Microsoft.WebPortal.Core.View.prototype._hide = function (hideProgress) {
    /// <summary>
    /// Private method. This will be called by the toggler. Clients of the view should use the hide() method which returns a JQuery deferred object
    /// which gets notified once hiding is complete. This method will hide the view using the configured animation. This will also hide the child views.
    /// The sub class can provide an onShowing(false) method that gets called before the view is hidden and an onShown(false) method that is called once hiding is complete.
    /// </summary>
    /// <param name="showProgress">A Jquery deferred object to resolve when hiding is complete.</param>

    if (!this.isRendered()) {
        // render the view first
        this.render();
    }

    if (this.onShowing) {
        // call the on showing hook to perform any logic needed before executing the hide animation
        this.onShowing(false);
    }

    // hide the child views
    var hidingProgressArray = [];

    for (var i in this.childViews()) {
        hidingProgressArray.push(this.childViews()[i].hide());
    }

    var self = this;

    $.when(hidingProgressArray).then(function () {
        // all children are hidden, hide the parent
        // hide the view
        self.animation.hide(self.elementSelector).always(function () {
            // hide is complete
            hideProgress.resolve();

            if (self.onShown) {
                // call the on shown hook to perform any custom post hide logic
                self.onShown(false);
            }
        });
    });
};

Microsoft.WebPortal.Core.View.prototype.destroy = function () {
    /// <summary>
    /// Destroys the view. This will also destroy the child views. The sub classes can provide an onDestroy() hook
    /// that gets called when the view is destroyed.
    /// </summary>

    if (this.onDestroy) {
        // call the on destroy hook
        this.onDestroy();
    }

    // destroy the child views
    for (var i in this.childViews()) {
        this.childViews()[i].destroy();
    }

    this.isShown(false);
    this.state = Microsoft.WebPortal.Core.View.State.Destroyed;
};

Microsoft.WebPortal.Core.View.prototype.isRendered = function () {
    /// <summary>
    /// Tells if the view is rendered or not.
    /// </summary>
    /// <returns type="boolean">True is rendered, false otherwise.</returns>

    return this.state === Microsoft.WebPortal.Core.View.State.Rendered;
};

Microsoft.WebPortal.Core.View.prototype.isDestroyed = function () {
    /// <summary>
    /// Tells if the view is destroyed or not.
    /// </summary>
    /// <returns type="boolean">True is destroyed, false otherwise.</returns>

    return this.state === Microsoft.WebPortal.Core.View.State.Destroyed;
};

/*
    Enumerates the possible view states.
*/
Microsoft.WebPortal.Core.View.State = {
    // view has been created but not yet rendered
    Created: 0,

    // view has been rendered
    Rendered: 1,

    // view has been destroyed
    Destroyed: 2
};

//@ sourceURL=View.js
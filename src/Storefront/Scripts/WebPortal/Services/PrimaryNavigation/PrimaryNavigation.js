Microsoft.WebPortal.Services.PrimaryNavigation = function (webPortal, animation, primaryNavigationTemplate) {
    /// <summary>
    /// The primary navigation service.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="animation">The animation to use in showing and hiding the primary navigation.</param>
    /// <param name="primaryNavigationTemplate">The template used to render the primary navigation.</param>

    this.base.constructor.call(this, webPortal, "PrimaryNavigation");

    this.webPortal.Helpers.throwIfNotSet(primaryNavigationTemplate, "primaryNavigationTemplate", "Microsoft.WebPortal.Services.PrimaryNavigation.Constructor.");
    this.template = ko.observable(primaryNavigationTemplate);
    this.animation = animation || new Microsoft.WebPortal.Utilities.Animation(Microsoft.WebPortal.Effects.SlideDown, 500);

    // primary navigation will be togglable
    Microsoft.WebPortal.Utilities.Toggler.injectToggling(this, this.show, this.hide, false);
};

$WebPortal.Helpers.inherit(Microsoft.WebPortal.Services.PrimaryNavigation, Microsoft.WebPortal.Core.PortalService);

Microsoft.WebPortal.Services.PrimaryNavigation.prototype._runService = function () {
    /// <summary>
    /// Runs primary navigation service.
    /// </summary>

    this.onHideMenus = function (eventId, context, broadcaster) {
        // hide the primary navigation in response to the hide menus event

        if (broadcaster !== this && this.isShown()) {
            this.hide();
        }
    };

    this.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.HideMenus, this.onHideMenus, this);
    ko.applyBindings(this, $(this.webPortal.Settings.Ids.PrimaryNavigation)[0]);
};

Microsoft.WebPortal.Services.PrimaryNavigation.prototype._stopService = function () {
    /// <summary>
    /// Stops the primary navigation service.
    /// </summary>

    this.webPortal.EventSystem.unsubscribe(Microsoft.WebPortal.Event.HideMenus, this.onHideMenus, this);
    var self = this;

    this.hide().always(function () {
        ko.cleanNode($(self.webPortal.Settings.Ids.PrimaryNavigation)[0]);
        $(self.webPortal.Settings.Ids.PrimaryNavigation).empty();
    });
};

Microsoft.WebPortal.Services.PrimaryNavigation.prototype.show = function (showProgress) {
    /// <summary>
    /// Shows primary navigation.
    /// </summary>
    /// <param name="showProgress">A Jquery deferred object to resolve when showing is complete.</param>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The primary navigation service is not running.");
        showProgress.reject();
        return;
    }

    // hide any shown menus
    this.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.HideMenus, null, this);

    // let the world know we are showing
    this.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.PrimaryNavigationShowing, true, this);

    this.animation.show(this.webPortal.Settings.Ids.PrimaryNavigation).always(function () {
        showProgress.resolve();
    });
};

Microsoft.WebPortal.Services.PrimaryNavigation.prototype.hide = function (hideProgress) {
    /// <summary>
    /// Hides primary navigation.
    /// </summary>
    /// <param name="hideProgress">A Jquery deferred object to resolve when hiding is complete.</param>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The primary navigation service is not running.");
        hideProgress.reject();
        return;
    }

    // let the world know we are hiding
    this.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.PrimaryNavigationShowing, false, this);

    this.animation.hide(this.webPortal.Settings.Ids.PrimaryNavigation).always(function () {
        hideProgress.resolve();
    });
};

Microsoft.WebPortal.Services.PrimaryNavigation.prototype.tileSelected = function (primaryNavigationInstance, selectedTile) {
    /// <summary>
    /// Called when a tile is selected from the primary navigation.
    /// </summary>
    /// <param name="primaryNavigationInstance">The primary navigation service instance.</param>
    /// <param name="selectedTile">The clicked tile.</param>

    if (selectedTile.Name !== primaryNavigationInstance.webPortal.activeTile().Name) {
        primaryNavigationInstance.webPortal.Journey.start(Microsoft.WebPortal.Feature[Microsoft.WebPortal.Tile[selectedTile.Name].DefaultFeature]);
    }
};

Microsoft.WebPortal.Services.PrimaryNavigation.prototype.setTemplate = function (primaryNavigationTemplate) {
    /// <summary>
    /// Sets the primary navigation template.
    /// </summary>
    /// <param name="primaryNavigationTemplate">The name of the template.</param>

    this.webPortal.Helpers.throwIfNotSet(primaryNavigationTemplate, "primaryNavigationTemplate", "Microsoft.WebPortal.Services.PrimaryNavigation.Constructor.");
    this.template(primaryNavigationTemplate);
};

//@ sourceURL=PrimaryNavigation.js
Microsoft.WebPortal.Core.Shell = function () {
    /// <summary>
    /// The web portal shell is the main portal core which manages tiles and features and their assigned presenters. The shell controls the web portal as whole.
    /// It defines the available services such as actions and notifications management as well as other framework services such as journeys, eventing system, wizards, etc...
    /// The shell downloads the portal to the browser and enables features to be plugged it and provide them with APIs to interact with the portal.
    /// </summary>

    // assign the server generated portal configuration and resources to our framework
    this.Configuration = portalConfiguration;
    this.Resources = portalResources;
    portalConfiguration = portalResources = undefined;
    
    // read the list of configured tiles
    for (var i in this.Configuration.Tiles) {
        this.Configuration.Tiles[i].Hidden = this.Configuration.Tiles[i].Hidden === true;
    }

    this.tiles = ko.observable(this.Configuration.Tiles);

    // determine the default tile, this is the first item by default
    this.defaultTile = ko.observable(this.findTile(this.Configuration.DefaultTile) || this.Configuration.Tiles[0]);

    // the active tile
    this.activeTile = ko.observable(this.defaultTile());

    // the feature to presenter hash table
    this.featurePresenters = {};

    // the portal services hash table
    this.Services = {};

    // build the tile and feature enumerations from the given configuration
    Microsoft.WebPortal.Tile = {};
    Microsoft.WebPortal.Feature = {};

    for (var i in this.Configuration.Tiles) {
        var currentTile = this.Configuration.Tiles[i];

        // add the tile id as a property to the tiles enum and let it point to the tile information
        Microsoft.WebPortal.Tile[currentTile.Name] = currentTile;

        for (var j in currentTile.Features) {
            // add the tile's features to the feature enum
            Microsoft.WebPortal.Feature[currentTile.Features[j].Name] = {
                name: currentTile.Features[j].Name,
                tile : currentTile
            }
        }
    }

    // initialize the helper methods
    this.Helpers = new Microsoft.WebPortal.Infrastructure.Helpers(this);
}

Microsoft.WebPortal.Core.Shell.prototype.load = function () {
    /// <summary>
    /// Loads the web portal from the server and initializes its services.
    /// </summary>

    // apply the knock out extensions
    Microsoft.WebPortal.Infrastructure.KoExtensions.apply(this);

    // initialize infrastructure services
    this.Settings = Microsoft.WebPortal.Infrastructure.Settings;
    this.Diagnostics = new Microsoft.WebPortal.Infrastructure.Diagnostics(this);

    // create the event system
    this.EventSystem = new Microsoft.WebPortal.Core.EventSystem(this);

    // show the splash screen while we download portal files from the server
    this.SplashScreen = this.Configuration.SplashScreen.getImplementation(this);

    ko.cleanNode($(this.Settings.Ids.SplashScreen)[0]);
    ko.applyBindings(this, $(this.Settings.Ids.SplashScreen)[0]);

    var self = this;

    this.SplashScreen.show(this, this.load).always(function () {
        var getPortalContentServerCall =
        new Microsoft.WebPortal.Utilities.RetryableServerCall(self.Helpers.ajaxCall(self.Configuration.WebApi.WebPortalContent,
        Microsoft.WebPortal.HttpMethod.Get), "GetWebPortalContent");

        var portalAssetsLoadingStartTime = new Date();

        // get the portal files from the server
        getPortalContentServerCall.execute().done(function (webPortalContent) {
            self.Diagnostics.information("Portal assets load time: " + (new Date() - portalAssetsLoadingStartTime));

            // hide the splash screen and start the portal
            self.SplashScreen.hide().always(function () {
                ko.cleanNode($(self.Settings.Ids.SplashScreen)[0]);
                $(self.Settings.Ids.SplashScreen).empty();
                self.SplashScreen = undefined;

                // add the portal content to the HTML body
                self.portalContent = ko.observable(webPortalContent);

                ko.cleanNode($(self.Settings.Ids.PortalContent)[0]);
                ko.applyBindings(self, $(self.Settings.Ids.PortalContent)[0]);

                // run!
                self._run();
            });
        }).fail(function (result, status, error) {
            self.Diagnostics.error("Failed to load the web portal content: " + error + ". Status code: " + result.status);
            self.SplashScreen.handleError(self.Resources.Strings.FailedToLoadPortal);
        });
    });
}

Microsoft.WebPortal.Core.Shell.prototype.registerFeaturePresenter = function (feature, presenterClass) {
    /// <summary>
    /// Assigns a presenter class to manage a feature.
    /// </summary>
    /// <param name="feature">The feature to manage. Use Microsoft.WebPortal.Features enumeration.</param>
    /// <param name="presenterClass">The presenter class from which objects will be created when the feature is activated.</param>

    this.Helpers.throwIfNotSet(feature, "feature", "Microsoft.WebPortal.Core.Shell.registerFeaturePresenter");
    this.Helpers.throwIfNotSet(presenterClass, "presenterClass", "Microsoft.WebPortal.Core.Shell.registerFeaturePresenter");

    this.featurePresenters[feature.name] = presenterClass;
}

Microsoft.WebPortal.Core.Shell.prototype.deregisterFeaturePresenter = function (feature, presenterClass) {
    /// <summary>
    /// Removes a presenter class from managing a feature.
    /// </summary>
    /// <param name="feature">The feature to relinquish. Use Microsoft.WebPortal.Features enumeration.</param>
    /// <param name="presenterClass">The presenter class that is withdrawing.</param>

    this.Helpers.throwIfNotSet(feature, "feature", "Microsoft.WebPortal.Core.Shell.deregisterFeaturePresenter");
    this.Helpers.throwIfNotSet(presenterClass, "presenterClass", "Microsoft.WebPortal.Core.Shell.deregisterFeaturePresenter");

    if (this.featurePresenters[feature.name] === presenterClass) {
        this.featurePresenters[feature.name] = undefined;
    } else {
        this.Diagnostics.error("The provided presenterClass does not manage the presenter. Access denied.");
        throw new Error("The provided presenterClass does not manage the presenter. Access denied.");
    }
}

Microsoft.WebPortal.Core.Shell.prototype.getFeaturePresenter = function (feature) {
    /// <summary>
    /// Returns the presenter class assigned to the given feature.
    /// </summary>
    /// <param name="feature"></param>

    this.Helpers.throwIfNotSet(feature, "feature", "Microsoft.WebPortal.Core.Shell.getFeaturePresenter");
    return this.featurePresenters[feature.name];
}

Microsoft.WebPortal.Core.Shell.prototype.registerPortalService = function (portalService) {
    /// <summary>
    /// Registers a portal service. The service will be accessible through the services property of the web portal instance using its name.
    /// For example, if a service named Navigation is registered, then it can be accessed this way: $WebPortal.Services.Navigation.
    /// </summary>
    /// <param name="portalService">The portal service. Must extend Microsoft.WebPortal.Core.PortalService.</param>

    this.Helpers.throwIfNotSet(portalService, "portalService", "Microsoft.WebPortal.Core.Shell.registerPortalService");

    if (this.Services[portalService.name]) {
        this.Diagnostics.error("A service with the same name is already registered. Please deregister it first.");
        throw new Error("A service with the same name is already registered. Please deregister it first.");
    } else {
        this.Services[portalService.name] = portalService;
        this.Services[portalService.name].run();
    }
}

Microsoft.WebPortal.Core.Shell.prototype.deregisterPortalService = function (portalService) {
    /// <summary>
    /// Deregisters a portal service.
    /// </summary>
    /// <param name="portalService">The portal service to deregister.</param>

    this.Helpers.throwIfNotSet(portalService, "portalService", "Microsoft.WebPortal.Core.Shell.deregisterPortalService");

    if (this.Services[portalService.name]) {
        this.Services[portalService.name].stop();
        this.Services[portalService.name] = undefined;
    }
}

Microsoft.WebPortal.Core.Shell.prototype.findTile = function (tileName) {
    /// <summary>
    /// Helper method to find a tile by its ID.
    /// </summary>
    /// <param name="tileName">The tile name.</param>
    /// <returns type="">The tile or null if none was found.</returns>

    for (var i = 0; i < this.tiles().length; ++i) {
        if (this.tiles()[i].Name === tileName) {
            return this.tiles()[i];
        }
    }

    return null;
}

Microsoft.WebPortal.Core.Shell.prototype._run = function () {
    /// <summary>
    /// Private method. Initializes the portal and runs its services.
    /// </summary>

    this.Journey = new Microsoft.WebPortal.Core.Journey(this);
    this.UrlManager = new Microsoft.WebPortal.Core.UrlManager(this);
    this.ContentPanel = new Microsoft.WebPortal.Core.ContentPanel(this);
    this.ServerCallManager = new Microsoft.WebPortal.Core.ServerCallManager(this);

    this.Session = new Microsoft.WebPortal.Core.SessionManager(this);

    var self = this;

    $("html").click(function () {
        // when the user clicks anywhere, hide all shown menus
        self.EventSystem.broadcast(Microsoft.WebPortal.Event.HideMenus, null, this);
    });

    // handle window resizing
    $(window).resize(function () {
        // broadcast a resizing event first to enable full screen components to fit their selves to the new window dimension
        self.EventSystem.broadcast(Microsoft.WebPortal.Event.OnWindowResizing);

        window.setTimeout(function () {
            // broadcast a resize event to let the components fill the new document
            self.EventSystem.broadcast(Microsoft.WebPortal.Event.OnWindowResized);
        }, 0);
    });

    // configure primary navigation
    var primaryNavigation = this.Configuration.PrimaryNavigation.getImplementation(this);
    this.registerPortalService(primaryNavigation);

    var notificationsPanel = new Microsoft.WebPortal.Services.NotificationsManager(this, this.Configuration.Notifications.getDefaultAnimation(), this.Configuration.Notifications.Template);
    this.registerPortalService(notificationsPanel);

    var dialog = new Microsoft.WebPortal.Services.Dialog(this);
    this.registerPortalService(dialog);

    // configure and register the header bar service
    var headerBar = new Microsoft.WebPortal.Services.HeaderBar(this);

    headerBar.addSection(new Microsoft.WebPortal.Services.TitleSection(this));
    headerBar.addSection(new Microsoft.WebPortal.Services.ActionsSection(this));
    headerBar.addSection(new Microsoft.WebPortal.Services.NotificationsSection(this));
    headerBar.addSection(new Microsoft.WebPortal.Services.LinksSection(this));
    //headerBar.addSection(new Microsoft.WebPortal.Services.UserSection(this));

    this.registerPortalService(headerBar);

    // configure the login service
    this.registerPortalService(new Microsoft.WebPortal.Services.Login(this));  

    this.EventSystem.subscribe(Microsoft.WebPortal.Event.ServiceStarted, function (event, service, broadcaster) {
        if (service === this.Services.Notifications) {
            this.Services.HeaderBar.addSection(new Microsoft.WebPortal.Services.NotificationsSection(this), 2);
        }
    }, this);

    this.EventSystem.subscribe(Microsoft.WebPortal.Event.ServiceStopped, function (event, service, broadcaster) {
        if (service === this.Services.Notifications) {
            this.Services.HeaderBar.removeSection(this.Settings.Ids.NotificationsSection);
        }
    }, this);

    // notify all interested components that the portal is initializing to give them a chance to perform start up tasks
    this.EventSystem.broadcast(Microsoft.WebPortal.Event.PortalInitializing);

    // notify all interested components that the application has finished initializing
    this.EventSystem.broadcast(Microsoft.WebPortal.Event.PortalInitialized);
}

$WebPortal = new Microsoft.WebPortal.Core.Shell();
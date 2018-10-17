/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Services.TitleSection = function (webPortal) {
    /// <summary>
    /// A header bar section that renders the product log, title, active feature and a drop down primary navigation.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>

    this.base.constructor.call(this, webPortal, "TitleSection", "TitleSection-template");

    this.style("padding:0; width: 1px;");
    this.tileSelectorDirection = ko.observable(this.webPortal.Services.PrimaryNavigation && this.webPortal.Services.PrimaryNavigation.isShown() ? "rotate(180deg)" : "rotate(0deg)");

    this.onLogoClicked = function () {
        /// <summary>
        /// Called when the Logo is clicked.
        /// </summary>

        // go to the default feature of the default tile
        this.webPortal.Journey.start(Microsoft.WebPortal.Feature[this.webPortal.defaultTile().DefaultFeature]);
    }

    this.onActiveTileClicked = function () {
        /// <summary>
        /// Called when the user clicks on the active tile icon.
        /// </summary>
        /// <param name="data"></param>

        // load the default tile feature
        this.webPortal.Journey.start(Microsoft.WebPortal.Feature[this.webPortal.activeTile().DefaultFeature]);
    }

    this.onTileSelectorClicked = function () {
        /// <summary>
        /// Called when the user clicks on the tile selector arrow.
        /// </summary>

        // toggle primary navigation
        if (this.webPortal.Services.PrimaryNavigation) {
            this.webPortal.Services.PrimaryNavigation.toggle();
        } else {
            this.webPortal.Diagnostics.warning("HeaderBar.TileSection: Primary navigation service is not running. Toggling disabled.");
        }
    }

    this.onHover = function (elementId, model) {
        $(elementId).css("background-color", model.webPortal.activeTile().AlternateColor);
    }

    this.onUnhover = function (elementId, model) {
        $(elementId).css("background-color", "");
    }

    this.onPrimaryNavigationShowChange = function (eventId, isShown, broadcaster) {
        // rotate the tile selector arrow according to the new state of the primary navigation
        this.tileSelectorDirection(isShown ? "rotate(180deg)" : "rotate(0deg)");
    }

    this.featureActivated = function (eventId, context, broadcaster) {
        /// <summary>
        /// Called when a feature is activated.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="context"></param>
        /// <param name="broadcaster"></param>

        if (context.feature.tile.Name !== this.webPortal.activeTile().Name) {
            // a different tile has been selected, fade out the current header bar and animate the theme color
            var newTile = this.webPortal.findTile(context.feature.tile.Name);

            if (newTile) {
                var self = this;

                $("#ActiveTile").fadeOut(this.webPortal.Configuration.DefaultAnimationDuration, function () {
                    $("#ActiveTile").fadeIn(self.webPortal.Configuration.DefaultAnimationDuration);
                });

                $("#HeaderBar > tbody > tr > td").animate({ backgroundColor: newTile.Color }, this.webPortal.Configuration.DefaultAnimationDuration, function () {
                    self.webPortal.activeTile(newTile);
                });
            }
        }
    }
}

$WebPortal.Helpers.inherit(Microsoft.WebPortal.Services.TitleSection, Microsoft.WebPortal.Services.HeaderBarSection);

Microsoft.WebPortal.Services.TitleSection.prototype.initialize = function () {
    /// <summary>
    /// This function is called to initialize the header bar section.
    /// </summary>

    this.tileSelectorDirection(this.webPortal.Services.PrimaryNavigation && this.webPortal.Services.PrimaryNavigation.isShown() ? "rotate(180deg)" : "rotate(0deg)");

    this.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.PrimaryNavigationShowing, this.onPrimaryNavigationShowChange, this);

    // we are interested in knowing when a feature has been activated
    this.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.FeatureActivated, this.featureActivated, this);
}

Microsoft.WebPortal.Services.TitleSection.prototype.destroy = function () {
    /// <summary>
    /// This function is called to destroy the header bar section.
    /// </summary>

    // stop listening to events
    this.webPortal.EventSystem.unsubscribe(Microsoft.WebPortal.Event.PrimaryNavigationShowing, this.onPrimaryNavigationShowChange, this);
    this.webPortal.EventSystem.unsubscribe(Microsoft.WebPortal.Event.FeatureActivated, this.featureActivated, this);
}

//@ sourceURL=TitleSection.js
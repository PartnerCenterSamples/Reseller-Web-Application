Microsoft.WebPortal.Views.OffersCatalog = function (webPortal, elementSelector, partnerOffers, adapter, isShown, animation) {
    /// <summary>
    /// A view that renders a collection of partner offers into tiles.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="elementSelector">The JQuery selector for the HTML element this view will own.</param>
    /// <param name="partnerOffers">A list of partner offers to render into tiles.</param>
    /// <param name="adapter">An adapter that will receive notifications from the catalog.</param>
    /// <param name="isShown">The initial show state. Optional. Default is false.</param>
    /// <param name="animation">Optional animation to use for showing and hiding the view.</param>

    this.base.constructor.call(this, webPortal, elementSelector, isShown, null, animation);
    this.template = "offersCatalog-template";

    this.webPortal.Helpers.throwIfNotSet(partnerOffers, "partnerOffers");

    this.viewModel = {
        isSelectable: ko.observable(false),
        showBuyLink: ko.observable(false),
        partnerOffers: partnerOffers
    };

    this.adapter = adapter;
    this.tiles = [];
    this.columns = ko.observable(3);

    this.viewModel.isSelectable.subscribe(function () {
        for (var i in this.tiles) {
            this.tiles[i].viewModel.isSelectable(this.viewModel.isSelectable());
        }
    }, this);

    this.viewModel.showBuyLink.subscribe(function () {
        for (var i in this.tiles) {
            this.tiles[i].viewModel.showBuyLink(this.viewModel.showBuyLink());
        }
    }, this);
};

// extend the base view
$WebPortal.Helpers.inherit(Microsoft.WebPortal.Views.OffersCatalog, Microsoft.WebPortal.Core.View);

Microsoft.WebPortal.Views.OffersCatalog.prototype.onRender = function () {
    /// <summary>
    /// Called when the view is rendered.
    /// </summary>

    $(this.elementSelector).attr("data-bind", ", template: { name: '" + this.template + "'}");
    ko.applyBindings(this, $(this.elementSelector)[0]);

    for (var i in this.viewModel.partnerOffers) {
        var divIndex = parseInt(i) + 1;

        // randomize tile animations
        var duration = 300 + 1000 * Math.random();

        this.tiles.push(new Microsoft.WebPortal.Views.OfferTile(
            this.webPortal,
            this.elementSelector + " > div:nth-child(" + divIndex + ") > div",
            this.viewModel.partnerOffers[i],
            this,
            false,
            new Microsoft.WebPortal.Utilities.Animation(Microsoft.WebPortal.Effects.Fade, duration)));

        this.tiles[i].viewModel.isSelectable(this.viewModel.isSelectable());
        this.tiles[i].viewModel.showBuyLink(this.viewModel.showBuyLink());
    }
};

Microsoft.WebPortal.Views.OffersCatalog.prototype.onShown = function (isShowing) {
    /// <summary>
    /// Called when the view is shown.
    /// </summary>

    if (isShowing) {
        // show the tiles
        for (var i in this.tiles) {
            this.tiles[i].show();
        }

        window.setTimeout(function (self) {
            // align the tile heights after they have been displayed
            var maxHeight = 0;

            for (var i in self.tiles) {
                if ($(self.tiles[i].elementSelector).height() > maxHeight) {
                    maxHeight = $(self.tiles[i].elementSelector).height();
                }
            }

            for (var j in self.tiles) {
                $(self.tiles[j].elementSelector).height(maxHeight);
            }

        }, 400, this);
    } else {
        // hide the tiles
        for (var k in this.tiles) {
            this.tiles[k].hide();
        }
    }
};

Microsoft.WebPortal.Views.OffersCatalog.prototype.onDestroy = function () {
    /// <summary>
    /// Called when the journey trail is about to be destroyed.
    /// </summary>

    // destroy child tiles
    for (var i in this.tiles) {
        this.tiles[i].destroy();
    }

    if ($(this.elementSelector)[0]) {
        // if the element is there, clear its bindings and clean up its content
        ko.cleanNode($(this.elementSelector)[0]);
        $(this.elementSelector).empty();
    }
};

Microsoft.WebPortal.Views.OffersCatalog.prototype.getSelectedPartnerOffers = function () {
    /// <summary>
    /// Gets the selected partner offers from the catalog.
    /// </summary>

    var selectedTiles = [];

    for (var i in this.tiles) {
        if (this.tiles[i].viewModel.isSelected()) {
            selectedTiles.push(this.tiles[i]);
        }
    }

    return selectedTiles;
};

Microsoft.WebPortal.Views.OffersCatalog.prototype.onSelectChanged = function () {
    /// <summary>
    /// Called when the user selects or deselects a tile.
    /// </summary>

    if (this.adapter && this.adapter.onSelectChanged) {
        // notify the adapter and send it the selected tiles
        this.adapter.onSelectChanged(this.getSelectedPartnerOffers());
    }
};

Microsoft.WebPortal.Views.OffersCatalog.prototype.onBuyNowClicked = function (tile) {
    /// <summary>
    /// Called when the user clicks buy on a tile.
    /// </summary>
    /// <param name="tile">The tile whose buy link was clicked.</param>

    if (this.adapter && this.adapter.onBuyNowClicked) {
        // notify the adapter
        this.adapter.onBuyNowClicked(tile);
    }
};

//@ sourceURL=OffersCatalog.js
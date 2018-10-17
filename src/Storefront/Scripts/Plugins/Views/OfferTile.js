Microsoft.WebPortal.Views.OfferTile = function (webPortal, elementSelector, partnerOffer, adapter, isShown, animation) {
    /// <summary>
    /// A view that renders a partner offer in a tile.
    /// </summary>
    /// <param name="webPortal">The web portal instance</param>
    /// <param name="elementSelector">The JQuery selector for the HTML element this view will own.</param>
    /// <param name="partnerOffer">The partner offer to display in the tile. Required.</param>
    /// <param name="adapter">An adapter that will receive notifications from the tile.</param>
    /// <param name="isShown">The initial show state. Optional. Default is false.</param>
    /// <param name="animation">Optional animation to use for showing and hiding the view.</param>

    this.base.constructor.call(this, webPortal, elementSelector, isShown, null, animation);
    this.template = "offerTile-template";

    this.webPortal.Helpers.throwIfNotSet(partnerOffer, "partnerOffer");

    this.viewModel = {
        partnerOffer: ko.observable(partnerOffer),
        isSelectable: ko.observable(false),
        showBuyLink: ko.observable(false),
        isSelected: ko.observable(false)
    };

    this.adapter = adapter;

    this.viewModel.isSelected.subscribe(function () {
        if (this.adapter && this.adapter.onSelectChanged) {
            // notify the adapter of the selection change
            this.adapter.onSelectChanged(this);
        }
    }, this);

    this.viewModel.formattedPrice = ko.computed(function () {
        Globalize.culture(webPortal.Resources.Strings.CurrentLocale);
        return Globalize.format(this.viewModel.partnerOffer().Price, "c");
    }, this);

    this.viewModel.Features = ko.computed(function () {
        // concatenates the feature offers into a comma separated list
        if (this.viewModel.partnerOffer().Features && this.viewModel.partnerOffer().Features.length > 0) {
            var featureSummary = "";

            for (var i in this.viewModel.partnerOffer().Features) {
                featureSummary += this.viewModel.partnerOffer().Features[i] + ", ";
            }

            featureSummary = featureSummary.trim().slice(0, -1);

            return featureSummary;
        }
        else {
            return "";
        }
    }, this);
};

// extend the base view
$WebPortal.Helpers.inherit(Microsoft.WebPortal.Views.OfferTile, Microsoft.WebPortal.Core.View);

Microsoft.WebPortal.Views.OfferTile.prototype.onRender = function () {
    /// <summary>
    /// Called when the view is rendered.
    /// </summary>

    $(this.elementSelector).attr("data-bind", "template: { name: '" + this.template + "'}");
    ko.applyBindings(this, $(this.elementSelector)[0]);
};

Microsoft.WebPortal.Views.OfferTile.prototype.onDestroy = function () {
    /// <summary>
    /// Called when the journey trail is about to be destroyed.
    /// </summary>

    if ($(this.elementSelector)[0]) {
        // if the element is there, clear its bindings and clean up its content
        ko.cleanNode($(this.elementSelector)[0]);
        $(this.elementSelector).empty();
    }
};

Microsoft.WebPortal.Views.OfferTile.prototype.onTileClicked = function () {
    /// <summary>
    /// Called when the user clicks on the tile.
    /// </summary>

    if (this.viewModel.isSelectable()) {
        this.viewModel.isSelected(!this.viewModel.isSelected());
    }
};

Microsoft.WebPortal.Views.OfferTile.prototype.onBuyNowClicked = function () {
    /// <summary>
    /// Called when the user clicks the buy now link.
    /// </summary>

    if (this.adapter && this.adapter.onBuyNowClicked) {
        // notify the adapter
        this.adapter.onBuyNowClicked(this);
    }

    return true;
};

//@ sourceURL=OfferTile.js
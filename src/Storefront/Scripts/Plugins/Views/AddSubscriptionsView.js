Microsoft.WebPortal.Views.AddSubscriptionsView = function (webPortal, elementSelector, defaultOffer, isShown, animation) {
    /// <summary>
    /// A view that renders UX showing a list of subscriptions to be added from a drop down list.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="elementSelector">The JQuery selector for the HTML element this view will own.</param>
    /// <param name="defaultOffer">An optional default offer to add to the subscriptions list.</param>
    /// <param name="isShown">The initial show state. Optional. Default is false.</param>
    /// <param name="animation">Optional animation to use for showing and hiding the view.</param>

    this.base.constructor.call(this, webPortal, elementSelector, isShown, null, animation);
    this.template = "addSubscriptions-template";
    var self = this;
    Globalize.culture(self.webPortal.Resources.Strings.CurrentLocale);

    this.offersToAdd = [];

    // configure the subscriptions list
    this.subscriptionsList = new Microsoft.WebPortal.Views.List(this.webPortal, elementSelector + " #SubscriptionsList", this);

    this.subscriptionsList.setColumns([
        new Microsoft.WebPortal.Views.List.Column("Name", null, false, false, null, null, null, "subscriptionEntry-template")
    ]);

    this.subscriptionsList.showHeader(false);
    this.subscriptionsList.setEmptyListUI(this.webPortal.Resources.Strings.Plugins.AddSubscriptionsView.EmptyListCaption);
    this.subscriptionsList.enableStatusBar(false);
    this.subscriptionsList.setSelectionMode(Microsoft.WebPortal.Views.List.SelectionMode.None);

    if (defaultOffer) {
        var quantity = ko.observable(1);
        var totalPrice = defaultOffer.Price * quantity();
        var formattedPrice = Globalize.format(defaultOffer.Price, "c");
        var offerTotalPrice = ko.observable(Globalize.format(totalPrice, "c"));

        this.subscriptionsList.append([{
            offer: defaultOffer,
            quantity: quantity,
            formattedPrice: formattedPrice,
            offerTotalPrice: offerTotalPrice
        }]);

        quantity.subscribe(function (newValue) {
            if (isNaN(parseInt(newValue))) {
                quantity(0);
            } else {
                quantity(parseInt(newValue));
            }

            totalPrice = defaultOffer.Price * quantity();
            offerTotalPrice(Globalize.format(totalPrice, "c"));
        }, this);
    }

    this.AddOfferItemToView = function (offerItem) {
        // add this portalOffer to subcriptionList. 
        var quantity = ko.observable(1);
        var totalPrice = offerItem.viewModel.partnerOffer().Price * quantity();
        var formattedPrice = Globalize.format(offerItem.viewModel.partnerOffer().Price, "c");
        var offerTotalPrice = ko.observable(Globalize.format(totalPrice, "c"));

        this.subscriptionsList.append([{
            offer: offerItem.viewModel.partnerOffer(),
            quantity: quantity,
            formattedPrice: formattedPrice,
            offerTotalPrice: offerTotalPrice
        }]);

        quantity.subscribe(function (newValue) {
            if (isNaN(parseInt(newValue))) {
                quantity(0);
            } else {
                quantity(parseInt(newValue));
            }

            totalPrice = offerItem.viewModel.partnerOffer().Price * quantity();
            offerTotalPrice(Globalize.format(totalPrice, "c"));
        }, this);

        $(elementSelector + " #SubscriptionsList").height($(elementSelector + " #SubscriptionsList table").height());
        webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.OnWindowResizing);
        webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.OnWindowResized);
    };

    this.firstPaymentTotalDisplay = ko.computed(function () {
        var total = 0;
        Globalize.culture(self.webPortal.Resources.Strings.CurrentLocale);

        for (var i in self.subscriptionsList.rows()) {
            total += self.subscriptionsList.rows()[i].quantity() * self.subscriptionsList.rows()[i].offer.Price.toFixed(2);
        }

        // globalize the total using currency format.         
        return Globalize.format(total, "c");
    });

    this.onSelectChanged = function (offers) {
        this.offersToAdd = offers;
    };

};

// extend the base view
$WebPortal.Helpers.inherit(Microsoft.WebPortal.Views.AddSubscriptionsView, Microsoft.WebPortal.Core.View);

Microsoft.WebPortal.Views.AddSubscriptionsView.prototype.onRender = function () {
    /// <summary>
    /// Called when the view is rendered.
    /// </summary>

    $(this.elementSelector).attr("data-bind", "template: { name: '" + this.template + "'}");
    ko.applyBindings(this, $(this.elementSelector)[0]);
};

Microsoft.WebPortal.Views.AddSubscriptionsView.prototype.onShowing = function (isShowing) {
    /// <summary>
    /// Called when the view is about to be shown or hidden.
    /// </summary>
    /// <param name="isShowing">true if showing, false if hiding.</param>

    if (isShowing) {
        this.subscriptionsList.show();
    } else {
        this.subscriptionsList.hide();
    }
};

Microsoft.WebPortal.Views.AddSubscriptionsView.prototype.onShown = function (isShown) {
    /// <summary>
    /// Called when the view is shown or hidden.
    /// </summary>
    /// <param name="isShown">true if shown, false if hidden.</param>

    if (isShown) {
        // resize the list to fit its content
        $(this.elementSelector + " #SubscriptionsList").height($(this.elementSelector + " #SubscriptionsList table").height());

        // force a window resize for the list to resize
        this.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.OnWindowResizing);
        this.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.OnWindowResized);
    }
};

Microsoft.WebPortal.Views.AddSubscriptionsView.prototype.onDestroy = function () {
    /// <summary>
    /// Called when the view is about to be destroyed.
    /// </summary>

    if (this.subscriptionsList) {
        this.subscriptionsList.destroy();
    }

    if ($(this.elementSelector)[0]) {
        // if the element is there, clear its bindings and clean up its content
        ko.cleanNode($(this.elementSelector)[0]);
        $(this.elementSelector).empty();
    }
};

Microsoft.WebPortal.Views.AddSubscriptionsView.prototype.onAddOfferClicked = function () {
    /// <summary>
    /// Called when the user wants to add more offers.
    /// </summary>

    var self = this;

    var okButton = Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.OK, 1, function () {
        // check if there are items in the offersToAdd array and then add them. 
        if (self.offersToAdd && self.offersToAdd.length > 0) {
            for (index = 0; index < self.offersToAdd.length; ++index) {
                self.AddOfferItemToView(self.offersToAdd[index]);
            }
        }
        // clear the offersToAdd.
        self.offersToAdd = [];
        self.webPortal.Services.Dialog.hide();
    });

    var cancelButton = Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.CANCEL, 1, function () {
        // clear the offersToAdd.
        self.offersToAdd = [];
        self.webPortal.Services.Dialog.hide();
    });

    var portaloffersFetchProgress = $.Deferred();
    self.webPortal.Session.fetchPortalOffers(portaloffersFetchProgress);

    portaloffersFetchProgress.done(function (portalOffers) {
        self.portalOffersFetched = portalOffers;
        self.portalOfferDialogViewModel = {
            PartnerOffersCatalog: new Microsoft.WebPortal.Views.OffersCatalog(self.webPortal, "#PartnerOffersCatalog", self.portalOffersFetched, self, false),
            errorMessage: ko.observable("")
        };

        self.portalOfferDialogViewModel.PartnerOffersCatalog.viewModel.isSelectable(true);

        self.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.DialogShown, self.onPortalOfferDialogShown, self);
        self.webPortal.Services.Dialog.show("portalOfferPicker-template", self.webPortal.Session.PortalOffers, [okButton, cancelButton]);
    });
};

Microsoft.WebPortal.Views.AddSubscriptionsView.prototype.onPortalOfferDialogShown = function (eventId, isShown) {
    if (isShown) {
        this.portalOfferDialogViewModel.PartnerOffersCatalog.show();
    }
};

//@ sourceURL=AddSubscriptionsView.js


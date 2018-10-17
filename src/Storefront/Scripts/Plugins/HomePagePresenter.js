Microsoft.WebPortal.HomePagePresenter = function (webPortal, feature) {
    /// <summary>
    /// Manages the home page experience. 
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature for which this presenter is created.</param>
    this.base.constructor.call(this, webPortal, feature, "Home", "/Template/Home/");

    this.viewModel = {
        ShowProgress: ko.observable(true),
        IsSet: ko.observable(false),
        IsPortalConfigured: ko.observable(false)
    };

    this.onBuyNowClicked = function (tile) {
        if (isAuthenticated) {
            // activate the add subscription presenter and pass it the selected partner offer
            webPortal.Journey.advance(Microsoft.WebPortal.Feature.AddSubscriptions, tile.viewModel.partnerOffer());
        } else {
            // activate the customer registration presenter and pass it the selected partner offer
            webPortal.Journey.advance(Microsoft.WebPortal.Feature.CustomerRegistration, tile.viewModel.partnerOffer());
        }
    };

    this.onConfigurePortalClicked = function () {
        // send control to the admin console
        this.webPortal.Journey.start(Microsoft.WebPortal.Feature.AdminConsole);
    };
};

// inherit BasePresenter
$WebPortal.Helpers.inherit(Microsoft.WebPortal.HomePagePresenter, Microsoft.WebPortal.Core.TemplatePresenter);

Microsoft.WebPortal.HomePagePresenter.prototype.onRender = function () {
    /// <summary>
    /// Called when the presenter is rendered but not shown yet.
    /// </summary>

    ko.applyBindings(this, $("#OffersContainer")[0]);
};

Microsoft.WebPortal.HomePagePresenter.prototype.onShow = function () {
    /// <summary>
    /// Called after the content has been shown.
    /// </summary>

    var self = this;

    var fetchPartnerOffers = function () {
        var getPartnerOffersServerCall = self.webPortal.ServerCallManager.create(self.feature,
            self.webPortal.Helpers.ajaxCall("api/partnerOffers", Microsoft.WebPortal.HttpMethod.Get), "GetPartnerOffers");

        self.viewModel.IsSet(false);
        self.viewModel.ShowProgress(true);

        getPartnerOffersServerCall.execute().done(function (partnerOffers) {
            self.viewModel.IsPortalConfigured(partnerOffers.IsPortalConfigured);
            self.viewModel.IsSet(true);

            if (partnerOffers.Offers) {
                self.offersCatalog = new Microsoft.WebPortal.Views.OffersCatalog(self.webPortal, "#PartnerOffersCatalogView", partnerOffers.Offers, self, false);
                self.offersCatalog.viewModel.showBuyLink(true);
                self.offersCatalog.show();
            }
        }).fail(function (result, status, error) {
            var notification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Error,
                self.webPortal.Resources.Strings.Plugins.HomePage.CouldNotRetrievePartnerOffers);

            notification.buttons([
                Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.RETRY, self.webPortal.Resources.Strings.Retry, function () {
                    notification.dismiss();

                    // retry
                    fetchPartnerOffers();
                }),
                Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.CANCEL, self.webPortal.Resources.Strings.Cancel, function () {
                    notification.dismiss();
                })
            ]);

            self.webPortal.Services.Notifications.add(notification);
        }).always(function () {
            // stop showing progress
            self.viewModel.ShowProgress(false);
        });
    };

    fetchPartnerOffers();
};

//@ sourceURL=HomePagePresenter.js
/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.CustomerAccountPresenter = function (webPortal, feature, context) {
    /// <summary>
    /// Manages the offers experience. 
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature for which this presenter is created.</param>
    this.base.constructor.call(this, webPortal, feature, "CustomerAccount", "/Template/CustomerAccount");
    this.onAddSubscriptionsClicked = function () {
        // go to the add subscriptions page
        webPortal.Journey.advance(Microsoft.WebPortal.Feature.AddSubscriptions);
    }
    this.onUpdateSubscriptionClicked = function () {
        var subscriptionItem = {
            SubscriptionId: this.SubscriptionId,
            PortalOfferId: this.PortalOfferId,
            FriendlyName: this.FriendlyName,
            LicensesTotal: this.LicensesTotal,
            SubscriptionProRatedPrice: this.SubscriptionProRatedPrice,
            isUpdateSubscription: true,
            isRenewSubscription: false
        }

        // navigate to page. 
        webPortal.Journey.advance(Microsoft.WebPortal.Feature.UpdateSubscriptions, subscriptionItem);
    }
    this.onRenewSubscriptionClicked = function () {
        var subscriptionItem = {
            SubscriptionId: this.SubscriptionId,
            PortalOfferId: this.PortalOfferId,
            FriendlyName: this.FriendlyName,
            LicensesTotal: this.LicensesTotal,
            SubscriptionProRatedPrice: this.SubscriptionProRatedPrice,
            isUpdateSubscription: false,
            isRenewSubscription: true
        }

        // navigate to page.
        webPortal.Journey.advance(Microsoft.WebPortal.Feature.UpdateSubscriptions, subscriptionItem);
    }

    this.viewModel = {
        ShowProgress: ko.observable(true),
        IsSet: ko.observable(false)
    }
}

// inherit BasePresenter
$WebPortal.Helpers.inherit(Microsoft.WebPortal.CustomerAccountPresenter, Microsoft.WebPortal.Core.TemplatePresenter);

Microsoft.WebPortal.CustomerAccountPresenter.prototype.onActivate = function () {
    /// <summary>
    /// Called when the presenter is activated.
    /// </summary>
}

Microsoft.WebPortal.CustomerAccountPresenter.prototype.onRender = function () {
    /// <summary>
    /// Called when the presenter is about to be rendered.
    /// </summary>

    var self = this;
    ko.applyBindings(self, $("#CustomerAccountContainer")[0]);

    var getCustomerAccount = function () {
        self.viewModel.IsSet(false);
        self.viewModel.ShowProgress(true);

        var customerInfoProgress = $.Deferred();
        self.webPortal.Session.fetchCustomerSubscriptionDetails(customerInfoProgress);

        customerInfoProgress.done(function (customerInformation) {
            self.viewModel.CustomerManagedSubscriptions = customerInformation.CustomerManagedSubscriptions;
            self.viewModel.PartnerManagedSubscriptions = customerInformation.PartnerManagedSubscriptions;
            self.viewModel.IsSet(true);
        }).fail(function (result, status, error) {
            var notification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Error,
                self.webPortal.Resources.Strings.Plugins.CustomerAccountPage.CouldNotRetrieveCustomerAccount);

            notification.buttons([
                Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.RETRY, self.webPortal.Resources.Strings.Retry, function () {
                    notification.dismiss();

                    // retry
                    getCustomerAccount();
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
    }

    getCustomerAccount();
}

//@ sourceURL=CustomerAccountPresenter.js
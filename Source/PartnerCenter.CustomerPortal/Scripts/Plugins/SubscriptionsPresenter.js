/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.SubscriptionsPresenter = function (webPortal, feature) {
    /// <summary>
    /// Manages the offers experience. 
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature for which this presenter is created.</param>
    this.base.constructor.call(this, webPortal, feature, "Subscriptions", "/Template/Subscriptions/");

    this.onToggleClicked = function (data, event) {
        // get this subscription row reference. 
        var currentSubscriptionRowElement = event.currentTarget.parentElement;

        if (currentSubscriptionRowElement) {
            $(currentSubscriptionRowElement).nextUntil('tr.SubscriptionRow').slideToggle(1);
            event.currentTarget.className = (event.currentTarget.className === "collapse_down") ? "collapse_up" : "collapse_down";
        }
    }

    this.viewModel = {
        ShowProgress: ko.observable(true),
        IsSet: ko.observable(false)
    }
}

// inherit BasePresenter
$WebPortal.Helpers.inherit(Microsoft.WebPortal.SubscriptionsPresenter, Microsoft.WebPortal.Core.TemplatePresenter);

Microsoft.WebPortal.SubscriptionsPresenter.prototype.onRender = function () {
    /// <summary>
    /// Called when the presenter is about to be rendered.
    /// </summary>

    var self = this;
    ko.applyBindings(self, $("#CustomerSubscriptionsContainer")[0]);

    var getSubscriptionsSummary = function () {
        var getSubscriptionSummaryServerCall = self.webPortal.ServerCallManager.create(
            self.feature, self.webPortal.Helpers.ajaxCall("api/Order/summary", Microsoft.WebPortal.HttpMethod.Get), "GetSubscriptionSummary");

        self.viewModel.IsSet(false);
        self.viewModel.ShowProgress(true);

        getSubscriptionSummaryServerCall.execute().done(function (subscriptionDetails) {
            self.viewModel.SubscriptionsSummary = ko.observable(subscriptionDetails);
            self.viewModel.IsSet(true);
        }).fail(function (result, status, error) {
            var notification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Error,
                self.webPortal.Resources.Strings.Plugins.SubscriptionsSummaryPage.CouldNotRetrieveSubscriptionsSummary);

            notification.buttons([
                Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.RETRY, self.webPortal.Resources.Strings.Retry, function () {
                    notification.dismiss();

                    // retry
                    getSubscriptionsSummary();
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

    getSubscriptionsSummary();
}

//@ sourceURL=SubscriptionsPresenter.js

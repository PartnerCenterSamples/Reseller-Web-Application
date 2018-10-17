Microsoft.WebPortal.AdminConsolePresenter = function (webPortal, feature) {
    /// <summary>
    /// Displays the customer portal admin dashboard.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature for which this presenter is created.</param>

    this.base.constructor.call(this, webPortal, feature, "Admin Console", "/Template/AdminConsole/");

    this.viewModel = {
        ShowProgress: ko.observable(true),
        UserName: userName,
        Offers: {
            Status: ko.observable(false)
        },
        Branding: {
            Status: ko.observable(false)
        },
        Payment: {
            Status: ko.observable(false)
        },
        IsSet: ko.observable(false)
    }

    this.viewModel.IsSetupComplete = ko.computed(function () {
        return this.viewModel.Offers.Status() && this.viewModel.Branding.Status() && this.viewModel.Payment.Status();
    }, this);

    this.viewModel.PromptMessage = ko.computed(function () {
        return this.viewModel.IsSetupComplete()
            ? this.webPortal.Resources.Strings.Plugins.AdminConsole.DashboardStatusOk
            : this.webPortal.Resources.Strings.Plugins.AdminConsole.DashboardStatusNotOk;
    }, this);

    this.getStatusIcon = function(status) {
        return status ? "/Content/Images/WebPortal/notification-success.png" : "/Content/Images/WebPortal/notification-error.png";
    }

    this.getStatusTooltip = function (status) {
        return status ? this.webPortal.Resources.Strings.Plugins.AdminConsole.DashboardItemConfigured : this.webPortal.Resources.Strings.Plugins.AdminConsole.DashboardItemNeedsConfiguration;
    }

    this.viewModel.Offers.Icon = ko.computed(function () {
        return this.getStatusIcon(this.viewModel.Offers.Status());
    }, this);

    this.viewModel.Offers.Tooltip = ko.computed(function () {
        return this.getStatusTooltip(this.viewModel.Offers.Status());
    }, this);

    this.viewModel.Branding.Icon = ko.computed(function () {
        return this.getStatusIcon(this.viewModel.Branding.Status());
    }, this);

    this.viewModel.Branding.Tooltip = ko.computed(function () {
        return this.getStatusTooltip(this.viewModel.Branding.Status());
    }, this);

    this.viewModel.Payment.Icon = ko.computed(function () {
        return this.getStatusIcon(this.viewModel.Payment.Status());
    }, this);

    this.viewModel.Payment.Tooltip = ko.computed(function () {
        return this.getStatusTooltip(this.viewModel.Payment.Status());
    }, this);
}

// inherit TemplatePresenter
$WebPortal.Helpers.inherit(Microsoft.WebPortal.AdminConsolePresenter, Microsoft.WebPortal.Core.TemplatePresenter);

Microsoft.WebPortal.AdminConsolePresenter.prototype.onRender = function () {
    /// <summary>
    /// Called when the presenter is rendered but not shown yet.
    /// </summary>

    ko.applyBindings(this, $("#AdminConsoleContainer")[0]);

    // get the dashboard information
    this.fetchDashboard();
}

Microsoft.WebPortal.AdminConsolePresenter.prototype.onConfigureBranding = function () {
    /// <summary>
    /// Called when the configure branding link is clicked.
    /// </summary>

    this.webPortal.Journey.advance(Microsoft.WebPortal.Feature.BrandingSetup);
}

Microsoft.WebPortal.AdminConsolePresenter.prototype.onConfigureOffers = function () {
    /// <summary>
    /// Called when the configure offers link is clicked.
    /// </summary>

    this.webPortal.Journey.advance(Microsoft.WebPortal.Feature.OfferList);
}

Microsoft.WebPortal.AdminConsolePresenter.prototype.onConfigurePayment = function () {
    /// <summary>
    /// Called when the configure payment link is clicked.
    /// </summary>

    this.webPortal.Journey.advance(Microsoft.WebPortal.Feature.PaymentSetup);
}

Microsoft.WebPortal.AdminConsolePresenter.prototype.fetchDashboard = function () {
    /// <summary>
    /// Retrieves the admin dashboard information from the server.
    /// </summary>
    
    // get the system configuration status
    var getSystemSettingsServerCall = this.webPortal.ServerCallManager.create(this.feature,
        this.webPortal.Helpers.ajaxCall("api/adminConsole", Microsoft.WebPortal.HttpMethod.Get), "GetAdminConsole");

    this.viewModel.IsSet(false);
    this.viewModel.ShowProgress(true);

    var self = this;  

    getSystemSettingsServerCall.execute().done(function (systemSettings) {
        // update the settings
        self.viewModel.Offers.Status(systemSettings.IsOffersConfigured);
        self.viewModel.Branding.Status(systemSettings.IsBrandingConfigured);
        self.viewModel.Payment.Status(systemSettings.IsPaymentConfigured);

        // show the dashboard
        self.viewModel.IsSet(true);
    }).fail(function (result, status, error) {
        var notification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Error, self.webPortal.Resources.Strings.Plugins.AdminConsole.DashboardLoadingError);

        notification.buttons([
            Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.RETRY, self.webPortal.Resources.Strings.Retry, function () {
                notification.dismiss();

                // retry
                self.fetchDashboard();
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

//@ sourceURL=AdminConsolePresenter.js
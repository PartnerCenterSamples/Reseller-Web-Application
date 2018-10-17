Microsoft.WebPortal.PaymentSetupPresenter = function (webPortal, feature, paymentConfiguration) {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature for which this presenter is created.</param>
    /// <param name="paymentConfiguration">An existing payment configuration model object to use.</param>

    this.base.constructor.call(this, webPortal, feature, "Payment Setup", "/Template/PaymentSetup/");

    this.viewModel = {
        IsSet: ko.observable(false),
        ClientId: ko.observable(""),
        ClientSecret: ko.observable(""),
        AccountType: ko.observable(null),
        SupportedAccountTypes: ko.observableArray([
            {
                Id: "sandbox",
                Title: this.webPortal.Resources.Strings.Plugins.PaymentConfiguration.SandboxAccountTypeCaption
            },
            {
                Id: "live",
                Title: this.webPortal.Resources.Strings.Plugins.PaymentConfiguration.LiveAccountTypeCaption
            }
        ])
    };

    this.viewModel.AccountType(this.viewModel.SupportedAccountTypes()[0]);

    this.existingPaymentConfiguration = paymentConfiguration;
};

// inherit TemplatePresenter
$WebPortal.Helpers.inherit(Microsoft.WebPortal.PaymentSetupPresenter, Microsoft.WebPortal.Core.TemplatePresenter);

Microsoft.WebPortal.PaymentSetupPresenter.prototype.onRender = function () {
    /// <summary>
    /// Called when the presenter is rendered but not shown yet.
    /// </summary>

    var self = this;

    ko.applyBindings(self, $("#PaymentForm")[0]);

    if (!this.existingPaymentConfiguration) {
        self.viewModel.IsSet(false);

        // we were not passed the existing payment configuration, get it from the server
        self.webPortal.ContentPanel.showProgress();

        var acquirePaymentConfiguration = function (errorNotification) {
            var resolver = $.Deferred();
            self.webPortal.Session.fetchPaymentConfiguration(resolver);

            resolver.done(function (paymentConfiguration) {
                self.existingPaymentConfiguration = paymentConfiguration;
                self._updateViewModel();
                self._setupActions();
                self.viewModel.IsSet(true);
            }).fail(function (result, status, error) {
                self.webPortal.Helpers.displayRetryCancelErrorNotification(errorNotification,
                    self.webPortal.Resources.Strings.Plugins.PaymentConfiguration.FetchPaymentConfigurationErrorMessage,
                    self.webPortal.Resources.Strings.Plugins.PaymentConfiguration.FetchPaymentConfigurationProgressMessage,
                    acquirePaymentConfiguration);
            }).always(function () {
                self.webPortal.ContentPanel.hideProgress();
            });
        };

        acquirePaymentConfiguration();
    } else {
        self._updateViewModel();
        self._setupActions();
        self.viewModel.IsSet(true);
    }
};

Microsoft.WebPortal.PaymentSetupPresenter.prototype.onSavePaymentConfiguration = function () {
    /// <summary>
    /// Saves the payment configuration to the server.
    /// </summary>

    var savePaymentServerCall = this.webPortal.ServerCallManager.create(this.feature,
        this.webPortal.Helpers.ajaxCall("api/AdminConsole/Payment", Microsoft.WebPortal.HttpMethod.Put, {
            ClientId: this.viewModel.ClientId(),
            ClientSecret: this.viewModel.ClientSecret(),
            AccountType: this.viewModel.AccountType().Id
        }), "Save Payment configuration");

    var paymentSaveNotification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Progress,
        this.webPortal.Resources.Strings.Plugins.PaymentConfiguration.UpdatePaymentProgressMessage);

    this.webPortal.Services.Notifications.add(paymentSaveNotification);

    var self = this;

    var savePayment = function () {
        // disable our action buttons
        self.savePaymentAction.enabled(false);
        self.resetPaymentAction.enabled(false);

        savePaymentServerCall.execute().done(function (updatedPaymentInformation) {
            // turn the notification into a success
            paymentSaveNotification.type(Microsoft.WebPortal.Services.Notification.NotificationType.Success);
            paymentSaveNotification.message(self.webPortal.Resources.Strings.Plugins.PaymentConfiguration.UpdatePaymentSuccessMessage);
            paymentSaveNotification.buttons([
                Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.OK, "save-ok", function () {
                    paymentSaveNotification.dismiss();
                })
            ]);

            // update the view model to reflect new changes
            self.existingPaymentConfiguration = updatedPaymentInformation;
            self._updateViewModel();
        }).fail(function (result, status, error) {
            var errorPayload = JSON.parse(result.responseText);

            if (errorPayload) {
                if (errorPayload.ErrorCode === Microsoft.WebPortal.ErrorCode.PaymentGatewayIdentityFailureDuringConfiguration) {
                    // treat this error as non retryable.                    
                    paymentSaveNotification.type(Microsoft.WebPortal.Services.Notification.NotificationType.Error);
                    paymentSaveNotification.buttons([
                        Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.OK, self.webPortal.Resources.Strings.OK, function () {
                            paymentSaveNotification.dismiss();
                        })
                    ]);
                    paymentSaveNotification.message(errorPayload.Details.ErrorMessage);
                }
                else {
                    // notify the user of the error and give them the ability to retry
                    self.webPortal.Helpers.displayRetryCancelErrorNotification(paymentSaveNotification,
                        self.webPortal.Resources.Strings.Plugins.PaymentConfiguration.UpdatePaymentErrorMessage,
                        self.webPortal.Resources.Strings.Plugins.PaymentConfiguration.UpdatePaymentProgressMessage, savePayment, function () {
                            self.viewModel.ClientId.notifySubscribers();
                        });
                }
            }

        });
    };

    savePayment();
};

Microsoft.WebPortal.PaymentSetupPresenter.prototype._updateViewModel = function () {
    /// <summary>
    /// Updates the view model with the existing payment configuration.
    /// </summary>

    if (!this.existingPaymentConfiguration.ClientId) {
        this.existingPaymentConfiguration.ClientId = "";
    }

    if (!this.existingPaymentConfiguration.ClientSecret) {
        this.existingPaymentConfiguration.ClientSecret = "";
    }

    if (!this.existingPaymentConfiguration.AccountType) {
        this.existingPaymentConfiguration.AccountType = "sandbox";
    }

    this.viewModel.ClientId(this.existingPaymentConfiguration.ClientId);
    this.viewModel.ClientSecret(this.existingPaymentConfiguration.ClientSecret);

    for (var i in this.viewModel.SupportedAccountTypes()) {
        if (this.viewModel.SupportedAccountTypes()[i].Id === this.existingPaymentConfiguration.AccountType) {
            this.viewModel.AccountType(this.viewModel.SupportedAccountTypes()[i]);
        }
    }
};


Microsoft.WebPortal.PaymentSetupPresenter.prototype._setupActions = function () {
    /// <summary>
    /// Sets up actions that can be performed on the portal payment configuration.
    /// </summary>

    var self = this;

    // add a save action
    this.savePaymentAction = new Microsoft.WebPortal.Services.Action("save-payment", this.webPortal.Resources.Strings.Save, function (menuItem) {
        if ($("#PaymentForm").valid()) {
            self.onSavePaymentConfiguration();
        }
    }, "/Content/Images/Plugins/action-save.png", this.webPortal.Resources.Strings.Save, null, false);

    this.webPortal.Services.Actions.add(this.savePaymentAction);

    // add a reset form action
    this.resetPaymentAction = new Microsoft.WebPortal.Services.Action("reset-payment", this.webPortal.Resources.Strings.Undo, function (menuItem) {
        self._updateViewModel();
    }, "/Content/Images/Plugins/action-undo.png", this.webPortal.Resources.Strings.Plugins.PaymentConfiguration.UndoUnsavedChanges, null, false);

    this.webPortal.Services.Actions.add(this.resetPaymentAction);

    this.webPortal.Services.Actions.actionsManager.operationSerializer.queue(this, function (operationResolver) {
        // this is queued to avoid a race condition in animating the actions which causes the actions to look disabled although they are enabled
        // ensure adding the action finishes animating the action and showing it as disabled before we compute its new disabled status
        this.saveActionStatusUpdater = ko.computed(function () {
            // enable and disable the action buttons depending on whether there was a value change or not
            var isFormUpdated = this.viewModel.ClientId() !== this.existingPaymentConfiguration.ClientId |
                this.viewModel.ClientSecret() !== this.existingPaymentConfiguration.ClientSecret |
                (this.viewModel.AccountType() && this.viewModel.AccountType().Id !== this.existingPaymentConfiguration.AccountType);

            this.savePaymentAction.enabled(isFormUpdated);
            this.resetPaymentAction.enabled(isFormUpdated);
        }, this);

        // tell the serializer that we are done
        operationResolver.resolve();
    });
};

//@ sourceURL=PaymentSetupPresenter.js
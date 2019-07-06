Microsoft.WebPortal.CustomerManagementSetupPresenter = function (webPortal, feature) {
    /// <summary>
    /// Manages the customer pre approval experience. 
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature for which this presenter is created.</param>
    this.base.constructor.call(this, webPortal, feature, "CustomerManagementSetup", "/Template/CustomerManagementSetup/");

    this.viewModel = {
        IsSet: ko.observable(false),
        IsEveryonePreApproved: ko.observable(false),
        searchTerm: ko.observable(""),
        preApprovedCustomersList: ko.observableArray(),
        preApprovedCustomerIds: ko.observableArray(),
        searchButtonClicked: function () {
            this.viewModel.preApprovedCustomersList(this.preChangePreApprovedCustomersDetails.Items.filter(customerfilter.bind(null, this.viewModel.searchTerm().toLowerCase())));
        }
    };

    function customerfilter(searchTerm, value, index, array) {

        if (searchTerm) {
            return array[index].CompanyName.toLowerCase().indexOf(searchTerm) !== -1 || array[index].Domain.toLowerCase().indexOf(searchTerm) !== -1;
        }
        else
            return array[index];
    }
};

// inherit TemplatePresenter
$WebPortal.Helpers.inherit(Microsoft.WebPortal.CustomerManagementSetupPresenter, Microsoft.WebPortal.Core.TemplatePresenter);

Microsoft.WebPortal.CustomerManagementSetupPresenter.prototype.onRender = function () {
    /// <summary>
    /// Called when the presenter is rendered but not shown yet.
    /// </summary>

    var self = this;
    ko.applyBindings(self, $("#CustomerPreApprovalForm")[0]);

    if (!this.preChangePreApprovedCustomersDetails) {
        self.webPortal.ContentPanel.showProgress();
        function customerfilter(searchTerm, value, index, array) {

            if (searchTerm) {
                return array[index].CompanyName.toLowerCase().indexOf(searchTerm().toLowerCase()) !== -1 || array[index].Domain.toLowerCase().indexOf(searchTerm().toLowerCase()) !== -1;
            }
            else
                return array[index];
        }

        var acquirePreApprovedCustomerDetails = function () {
            self.viewModel.IsSet(false);
            var getPreApprovedCustomerDetailsServerCall = self.webPortal.ServerCallManager.create(
                self.feature, self.webPortal.Helpers.ajaxCall("api/AdminConsole/PreApprovedCustomers", Microsoft.WebPortal.HttpMethod.Get), "GetPreApprovedCustomerDetails");

            getPreApprovedCustomerDetailsServerCall.execute().done(function (PreApprovedCustomersViewModel) {
                self.viewModel.IsEveryonePreApproved(PreApprovedCustomersViewModel.IsEveryCustomerPreApproved);
                self.viewModel.preApprovedCustomersList(PreApprovedCustomersViewModel.Items.filter(customerfilter.bind(null, "")));

                self._setupCustomerIds(PreApprovedCustomersViewModel);
                self.preChangePreApprovedCustomersDetails = PreApprovedCustomersViewModel;
                self.preChangePreApprovedCustomerIds = PreApprovedCustomersViewModel.CustomerIds;
                self._setupActions();
                self.viewModel.IsSet(true);

                self.viewModel.preApprovedCustomerIds.subscribe(function (newValue) {
                    self._setActionButtons();
                }, self);

                self.viewModel.IsEveryonePreApproved.subscribe(function (newValue) {
                    self._setActionButtons();
                }, self);

            }).fail(function (result, status, error) {
                var notification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Error,
                    self.webPortal.Resources.Strings.Plugins.CustomerManagementConfiguration.FetchPreApprovedCustomersErrorMessage);
                notification.buttons([
                    Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.RETRY, self.webPortal.Resources.Strings.Retry, function () {
                        notification.dismiss();
                        acquirePreApprovedCustomerDetails(); // retry
                    }),
                    Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.CANCEL, self.webPortal.Resources.Strings.Cancel, function () {
                        notification.dismiss();
                    })
                ]);
                self.webPortal.Services.Notifications.add(notification);
            }).always(function () {
                // stop showing progress
                self.webPortal.ContentPanel.hideProgress();
            });
        };

        acquirePreApprovedCustomerDetails();
    }
    else {
        self._setupActions();
        self.viewModel.IsSet(true);
    }
};

Microsoft.WebPortal.CustomerManagementSetupPresenter.prototype.onSaveConfiguration = function () {
    /// <summary>
    /// Saves the pre-approved customer configuration to the server.
    /// </summary>
    var self = this;
    var savePreApprovedCustomersCall = this.webPortal.ServerCallManager.create(this.feature,
        this.webPortal.Helpers.ajaxCall("api/AdminConsole/PreApprovedCustomers", Microsoft.WebPortal.HttpMethod.Put, {
            IsEveryCustomerPreApproved: self.viewModel.IsEveryonePreApproved(),
            CustomerIds: self.viewModel.preApprovedCustomerIds()
        }), "Saving pre-approved customers configuration");

    var saveNotification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Progress,
        this.webPortal.Resources.Strings.Plugins.CustomerManagementConfiguration.UpdatePreApprovedCustomersProgressMessage);
    this.webPortal.Services.Notifications.add(saveNotification);

    var saveConfiguration = function () {

        // disable our action buttons
        self.savePreApprovedCustomersAction.enabled(false);
        self.resetPreApprovedCustomersAction.enabled(false);
        savePreApprovedCustomersCall.execute().done(function (PreApprovedCustomersViewModel) {
            // turn the notification into a success
            saveNotification.type(Microsoft.WebPortal.Services.Notification.NotificationType.Success);
            saveNotification.message(self.webPortal.Resources.Strings.Plugins.CustomerManagementConfiguration.UpdatePreApprovedCustomersSuccessMessage);
            saveNotification.buttons([
                Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.OK, "save-ok", function () {
                    saveNotification.dismiss();
                })
            ]);

            self.viewModel.IsEveryonePreApproved(PreApprovedCustomersViewModel.IsEveryCustomerPreApproved);
            self.viewModel.preApprovedCustomersList(PreApprovedCustomersViewModel.Items);

            self._setupCustomerIds(PreApprovedCustomersViewModel);
            self.preChangePreApprovedCustomersDetails = PreApprovedCustomersViewModel;
            self.preChangePreApprovedCustomerIds = PreApprovedCustomersViewModel.CustomerIds;
            self.viewModel.searchTerm("");
            self.viewModel.IsSet(true);

            // disable our action buttons
            self.savePreApprovedCustomersAction.enabled(false);
            self.resetPreApprovedCustomersAction.enabled(false);
        }).fail(function (result, status, error) {
            var errorPayload = JSON.parse(result.responseText);

            if (errorPayload) {
                // notify the user of the error and give them the ability to retry
                self.webPortal.Helpers.displayRetryCancelErrorNotification(saveNotification,
                    self.webPortal.Resources.Strings.Plugins.CustomerManagementConfiguration.UpdatePreApprovedCustomersErrorMessage,
                    self.webPortal.Resources.Strings.Plugins.CustomerManagementConfiguration.UpdatePreApprovedCustomersProgressMessage, saveConfiguration, function () {
                        // self.viewModel.ClientId.notifySubscribers();
                    });
            }
        });
    };

    saveConfiguration();
};

Microsoft.WebPortal.CustomerManagementSetupPresenter.prototype._setupCustomerIds = function (preApprovedCustomerItems) {
    // for perf reasons we will operate with an alternative array instead of the observable array. 
    var newArray = [];
    for (var i in preApprovedCustomerItems.Items) {
        if (preApprovedCustomerItems.Items[i].IsPreApproved) {
            newArray.push(preApprovedCustomerItems.Items[i].TenantId);
        }
    }
    this.viewModel.preApprovedCustomerIds(newArray);
};

Microsoft.WebPortal.CustomerManagementSetupPresenter.prototype._setupActions = function () {
    /// <summary>
    /// Sets up actions that can be performed on the portal page configuration.
    /// </summary>

    var self = this;
    // add a save action
    this.savePreApprovedCustomersAction = new Microsoft.WebPortal.Services.Action("save-preapprovedcustomers", this.webPortal.Resources.Strings.Save, function (menuItem) {
        self.onSaveConfiguration();
    }, "/Content/Images/Plugins/action-save.png", this.webPortal.Resources.Strings.Save, null, false);
    this.webPortal.Services.Actions.add(this.savePreApprovedCustomersAction);

    // add a reset form action
    this.resetPreApprovedCustomersAction = new Microsoft.WebPortal.Services.Action("reset-preapprovedcustomers", this.webPortal.Resources.Strings.Undo, function (menuItem) {
        self.viewModel.IsEveryonePreApproved(self.preChangePreApprovedCustomersDetails.IsEveryCustomerPreApproved);
        self.viewModel.preApprovedCustomersList(self.preChangePreApprovedCustomersDetails.Items);
        self.viewModel.searchTerm("");
        self._setupCustomerIds(self.preChangePreApprovedCustomersDetails);
        self.preChangePreApprovedCustomerIds = self.preChangePreApprovedCustomersDetails.CustomerIds;
    }, "/Content/Images/Plugins/action-undo.png", this.webPortal.Resources.Strings.Undo, null, false);
    this.webPortal.Services.Actions.add(this.resetPreApprovedCustomersAction);
};

Microsoft.WebPortal.CustomerManagementSetupPresenter.prototype._setActionButtons = function () {
    var isFormUpdated = false;
    var isCustomersListUpdated = false;

    var isEveryOneFlagUpdated = this.preChangePreApprovedCustomersDetails.IsEveryCustomerPreApproved !== this.viewModel.IsEveryonePreApproved();

    var preChangeLength = 0;
    if (this.preChangePreApprovedCustomerIds) {
        preChangeLength = this.preChangePreApprovedCustomerIds.length;
    }

    isCustomersListUpdated = preChangeLength !== this.viewModel.preApprovedCustomerIds().length;

    if (!isCustomersListUpdated) {
        // sort the arrays. 
        var array1 = ko.utils.arrayMap(this.preChangePreApprovedCustomerIds, function (i) { return i; }).sort();
        var array2 = ko.utils.arrayMap(this.viewModel.preApprovedCustomerIds(), function (i) { return i; }).sort();

        var x = ko.utils.compareArrays(array1, array2);
        isCustomersListUpdated = x.length !== preChangeLength;
    }

    // if either of them is updated. 
    isFormUpdated = isEveryOneFlagUpdated | isCustomersListUpdated;

    this.savePreApprovedCustomersAction.enabled(isFormUpdated);
    this.resetPreApprovedCustomersAction.enabled(isFormUpdated);
};

//@ sourceURL=CustomerManagementSetupPresenter.js
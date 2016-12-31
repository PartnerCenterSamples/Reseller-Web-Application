/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.BrandingSetupPresenter = function (webPortal, feature, brandingConfiguration) {
    /// <summary>
    /// Displays UI for a partner to configure their portal's branding. Partners can configure their organization name, logo, contact details and thusforth.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature for which this presenter is created.</param>
    /// <param name="brandingConfiguration">The existing branding configuration, if present.</param>

    this.base.constructor.call(this, webPortal, feature, "Branding Setup", "/Template/BrandingSetup/");

    this.existingBrandingConfiguration = brandingConfiguration;

    this.viewModel = {
        IsSet: ko.observable(false),
        OrganizationName: ko.observable(""),
        OrganizationLogo: ko.observable(""),
        ContactUs: {
            Email: ko.observable(""),
            Phone: ko.observable("")
        },
        ContactSales: {
            Email: ko.observable(""),
            Phone: ko.observable("")
        },
        HeaderImage: ko.observable(""),
        PrivacyAgreement: ko.observable(""),
        InstrumentationKey: ko.observable("")
    }
}

// inherit TemplatePresenter
$WebPortal.Helpers.inherit(Microsoft.WebPortal.BrandingSetupPresenter, Microsoft.WebPortal.Core.TemplatePresenter);

Microsoft.WebPortal.BrandingSetupPresenter.prototype.onRender = function () {
    /// <summary>
    /// Called when the presenter is rendered but not shown yet.
    /// </summary>

    var self = this;

    ko.applyBindings(self, $("#GrandBrandingContainer")[0]);

    if (!this.existingBrandingConfiguration) {
        self.viewModel.IsSet(false);

        // we were not passed the existing branding configuration, get it from the server
        self.webPortal.ContentPanel.showProgress();

        var acquireBrandingConfiguration = function (errorNotification) {
            var resolver = $.Deferred();
            self.webPortal.Session.fetchBrandingConfiguration(resolver);

            resolver.done(function (brandingConfiguration) {
                self.existingBrandingConfiguration = brandingConfiguration;
                self._updateViewModel();
                self._setupActions();
                self.viewModel.IsSet(true);
            }).fail(function (result, status, error) {
                self.webPortal.Helpers.displayRetryCancelErrorNotification(errorNotification,
                    self.webPortal.Resources.Strings.Plugins.PortalBranding.BrandingRetrievalErrorMessage,
                    self.webPortal.Resources.Strings.Plugins.PortalBranding.BrandingRetrievalProgressMessage,
                    acquireBrandingConfiguration);
            }).always(function () {
                self.webPortal.ContentPanel.hideProgress();
            });
        }

        acquireBrandingConfiguration();
    } else {
        self._updateViewModel();
        self._setupActions();
        self.viewModel.IsSet(true);
    }
}

Microsoft.WebPortal.BrandingSetupPresenter.prototype.onUploadOrganizationLogoClicked = function () {
    /// <summary>
    /// Called when the upload organization logo image is clicked.
    /// </summary>

    // open up the file open dialog
    $("#UploadOrganizationLogo").click();
}

Microsoft.WebPortal.BrandingSetupPresenter.prototype.onOrganizationLogoUpdated = function (self, event) {
    /// <summary>
    /// Fired when an Organization logo file is selected.
    /// </summary>
    /// <param name="self">A reference to our presenter.</param>
    /// <param name="event">The change event.</param>

    // update the organization logo text box with the file name if a file was selected
    if (event.target.files.length >= 0) {
        self.viewModel.OrganizationLogo(event.target.files[0].name);
    }
}

Microsoft.WebPortal.BrandingSetupPresenter.prototype.onUploadHeaderClicked = function () {
    /// <summary>
    /// Called when the upload header image is clicked.
    /// </summary>

    // open up the open file dialog
    $("#UploadHeaderImage").click();
}

Microsoft.WebPortal.BrandingSetupPresenter.prototype.onHeaderImageUpdated = function (self, event) {
    /// <summary>
    /// Fired when a Header image file is selected.
    /// </summary>
    /// <param name="self">A reference to our presenter.</param>
    /// <param name="event">The change event.</param>

    if (event.target.files.length >= 0) {
        self.viewModel.HeaderImage(event.target.files[0].name);
    }
}

Microsoft.WebPortal.BrandingSetupPresenter.prototype.onSaveBranding = function () {
    /// <summary>
    /// Saves the portal branding changes.
    /// </summary>

    var formData = new FormData();

    if ($("#UploadOrganizationLogo")[0].files.length > 0) {
        if (this.viewModel.OrganizationLogo() === $("#UploadOrganizationLogo")[0].files[0].name) {
            // the selected image matches the org logo textbox which means the user has not changed the textbox afterwards, we should upload the logo image
            formData.append("OrganizationLogoFile", $("#UploadOrganizationLogo")[0].files[0]);
        }
    }

    if ($("#UploadHeaderImage")[0].files.length > 0) {
        if (this.viewModel.HeaderImage() === $("#UploadHeaderImage")[0].files[0].name) {
            // the selected image matches the header image textbox which means the user has not changed the textbox afterwards, we should upload the header image
            formData.append("HeaderImageFile", $("#UploadHeaderImage")[0].files[0]);
        }
    }

    formData.append("OrganizationName", this.viewModel.OrganizationName());
    formData.append("OrganizationLogo", this.viewModel.OrganizationLogo());

    formData.append("ContactUsEmail", this.viewModel.ContactUs.Email());
    formData.append("ContactUsPhone", this.viewModel.ContactUs.Phone());

    formData.append("ContactSalesEmail", this.viewModel.ContactSales.Email());
    formData.append("ContactSalesPhone", this.viewModel.ContactSales.Phone());

    formData.append("HeaderImage", this.viewModel.HeaderImage());
    formData.append("PrivacyAgreement", this.viewModel.PrivacyAgreement());
    formData.append("InstrumentationKey", this.viewModel.InstrumentationKey());

    var saveBrandingServerCall = this.webPortal.ServerCallManager.create(this.feature,
        function () {
            return $.ajax({
                type: "POST",
                url: "api/AdminConsole/Branding",
                data: formData,
                dataType: "json",
                contentType: false,
                processData: false
            });
        },
        "Save Branding");

    var brandingSaveNotification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Progress,
        this.webPortal.Resources.Strings.Plugins.PortalBranding.BrandingUpdateProgressMessage);

    this.webPortal.Services.Notifications.add(brandingSaveNotification);

    var self = this;

    var saveBranding = function () {
        // disable our action buttons
        self.saveBrandingAction.enabled(false);
        self.resetBrandingAction.enabled(false);

        saveBrandingServerCall.execute().done(function (updatedBrandingInformation) {
            // turn the notification into a success
            brandingSaveNotification.type(Microsoft.WebPortal.Services.Notification.NotificationType.Success);
            brandingSaveNotification.message(self.webPortal.Resources.Strings.Plugins.PortalBranding.BrandingUpdateSuccessMessage);
            brandingSaveNotification.buttons([
                Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.OK, "save-ok", function () {
                    brandingSaveNotification.dismiss();
                })
            ]);

            // update the view model to reflect new changes
            self.existingBrandingConfiguration = updatedBrandingInformation;
            self._updateViewModel();

            if (!self.restartPortalAction) {
                // enable the user to restart the portal to see their changes
                self.restartPortalAction = new Microsoft.WebPortal.Services.Action("reload-portal", self.webPortal.Resources.Strings.Plugins.PortalBranding.ReloadPortalButtonCaption, function (menuItem) {
                    window.location.reload(true);
                }, "/Content/Images/Plugins/action-refresh.png", self.webPortal.Resources.Strings.Plugins.PortalBranding.ReloadPortalButtonTooltip, null, true);

                self.webPortal.Services.Actions.add(self.restartPortalAction);
            }
        }).fail(function (result, status, error) {
            // notify the user of the error and give them the ability to retry

            if (result.status == 400) {
                brandingSaveNotification.type(Microsoft.WebPortal.Services.Notification.NotificationType.Error);
                var errorPayload = JSON.parse(result.responseText);

                switch (errorPayload.ErrorCode) {
                    case Microsoft.WebPortal.ErrorCode.InvalidFileType:
                        brandingSaveNotification.message(self.webPortal.Resources.Strings.Plugins.PortalBranding.InvalidFileType);
                        break;
                    case Microsoft.WebPortal.ErrorCode.MaximumRequestSizeExceeded:
                        brandingSaveNotification.message(self.webPortal.Resources.Strings.Plugins.PortalBranding.ImagesTooLarge);
                        break;
                    case Microsoft.WebPortal.ErrorCode.InvalidInput:
                        if (errorPayload.Details.Field == "HeaderImage") {
                            brandingSaveNotification.message(self.webPortal.Resources.Strings.Plugins.PortalBranding.InvalidHeaderImageUri);
                        } else if (errorPayload.Details.Field == "OrganizationLogo") {
                            brandingSaveNotification.message(self.webPortal.Resources.Strings.Plugins.PortalBranding.InvalidOrganizationLogoUri);
                        } else if (errorPayload.Details.Field == "PrivacyAgreement") {
                            brandingSaveNotification.message(self.webPortal.Resources.Strings.Plugins.PortalBranding.InvalidPrivacyAgreementUri);
                        } else if (errorPayload.Details.Field == "ContactUs.Phone") {
                            brandingSaveNotification.message(self.webPortal.Resources.Strings.Plugins.PortalBranding.InvalidContactUsPhone);
                        } else if (errorPayload.Details.Field == "ContactSales.Phone") {
                            brandingSaveNotification.message(self.webPortal.Resources.Strings.Plugins.PortalBranding.InvalidContactSalesPhone);
                        } else {
                            brandingSaveNotification.message(self.webPortal.Resources.Strings.Plugins.InvalidInput + errorPayload.Details.Field);
                        }
                        break;
                    default:
                        brandingSaveNotification.message(self.webPortal.Resources.Strings.Plugins.BadInputGenericMessage);
                        break;
                }

                brandingSaveNotification.buttons([
                    Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.OK, self.webPortal.Resources.Strings.OK, function () {
                        brandingSaveNotification.dismiss();
                    })
                ]);

                // re-calculate the action enabled status
                self.viewModel.OrganizationName.notifySubscribers();
            } else {
                self.webPortal.Helpers.displayRetryCancelErrorNotification(brandingSaveNotification,
                    self.webPortal.Resources.Strings.Plugins.PortalBranding.BrandingUpdateErrorMessage,
                    self.webPortal.Resources.Strings.Plugins.PortalBranding.BrandingUpdateProgressMessage, saveBranding, function () {
                        self.viewModel.OrganizationName.notifySubscribers();
                    });
            }
        });
    }

    saveBranding();
}

Microsoft.WebPortal.BrandingSetupPresenter.prototype._updateViewModel = function () {
    /// <summary>
    /// Updates the view model with the existing branding configuration.
    /// </summary>

    if (!this.existingBrandingConfiguration.OrganizationName) {
        this.existingBrandingConfiguration.OrganizationName = "";
    }

    if (!this.existingBrandingConfiguration.OrganizationLogo) {
        this.existingBrandingConfiguration.OrganizationLogo = "";
    }

    if (!this.existingBrandingConfiguration.ContactUs) {
        this.existingBrandingConfiguration.ContactUs = {}
    }

    if (!this.existingBrandingConfiguration.ContactUs.Email) {
        this.existingBrandingConfiguration.ContactUs.Email = "";
    }

    if (!this.existingBrandingConfiguration.ContactUs.Phone) {
        this.existingBrandingConfiguration.ContactUs.Phone = "";
    }

    if (!this.existingBrandingConfiguration.ContactSales) {
        this.existingBrandingConfiguration.ContactSales = {}
    }

    if (!this.existingBrandingConfiguration.ContactSales.Email) {
        this.existingBrandingConfiguration.ContactSales.Email = "";
    }

    if (!this.existingBrandingConfiguration.ContactSales.Phone) {
        this.existingBrandingConfiguration.ContactSales.Phone = "";
    }

    if (!this.existingBrandingConfiguration.HeaderImage) {
        this.existingBrandingConfiguration.HeaderImage = "";
    }

    if (!this.existingBrandingConfiguration.PrivacyAgreement) {
        this.existingBrandingConfiguration.PrivacyAgreement = "";
    }

    if (!this.existingBrandingConfiguration.InstrumentationKey) {
        this.existingBrandingConfiguration.InstrumentationKey = "";
    }

    this.viewModel.OrganizationName(this.existingBrandingConfiguration.OrganizationName);
    this.viewModel.OrganizationLogo(this.existingBrandingConfiguration.OrganizationLogo);
    this.viewModel.ContactUs.Email(this.existingBrandingConfiguration.ContactUs.Email);
    this.viewModel.ContactUs.Phone(this.existingBrandingConfiguration.ContactUs.Phone);
    this.viewModel.ContactSales.Email(this.existingBrandingConfiguration.ContactSales.Email);
    this.viewModel.ContactSales.Phone(this.existingBrandingConfiguration.ContactSales.Phone);
    this.viewModel.HeaderImage(this.existingBrandingConfiguration.HeaderImage);
    this.viewModel.PrivacyAgreement(this.existingBrandingConfiguration.PrivacyAgreement);
    this.viewModel.InstrumentationKey(this.existingBrandingConfiguration.InstrumentationKey);
}


Microsoft.WebPortal.BrandingSetupPresenter.prototype._setupActions = function () {
    /// <summary>
    /// Sets up actions that can be performed on the portal branding.
    /// </summary>

    var self = this;

    // add a save action
    this.saveBrandingAction = new Microsoft.WebPortal.Services.Action("save-branding", this.webPortal.Resources.Strings.Save, function (menuItem) {
        if ($("#PortalBrandingForm").valid()) {
            self.onSaveBranding();
        }
    }, "/Content/Images/Plugins/action-save.png", this.webPortal.Resources.Strings.Save, null, false);

    this.webPortal.Services.Actions.add(this.saveBrandingAction);

    // add a reset form action
    this.resetBrandingAction = new Microsoft.WebPortal.Services.Action("reset-branding", this.webPortal.Resources.Strings.Undo, function (menuItem) {
        self._updateViewModel();
    }, "/Content/Images/Plugins/action-undo.png", this.webPortal.Resources.Strings.Plugins.PortalBranding.UndoBrandingChangesTooltip, null, false);

    this.webPortal.Services.Actions.add(this.resetBrandingAction);

    this.saveActionStatusUpdater = ko.computed(function () {
        // enable and disable the action buttons depending on whether there was a value change or not
        var isFormUpdated = this.viewModel.OrganizationName() != this.existingBrandingConfiguration.OrganizationName |
            this.viewModel.OrganizationLogo() != this.existingBrandingConfiguration.OrganizationLogo |
            this.viewModel.HeaderImage() != this.existingBrandingConfiguration.HeaderImage |
            this.viewModel.PrivacyAgreement() != this.existingBrandingConfiguration.PrivacyAgreement | 
            this.viewModel.InstrumentationKey() != this.existingBrandingConfiguration.InstrumentationKey;

        isFormUpdated = isFormUpdated | this.viewModel.ContactUs.Email() != this.existingBrandingConfiguration.ContactUs.Email |
            this.viewModel.ContactUs.Phone() != this.existingBrandingConfiguration.ContactUs.Phone;

        isFormUpdated = isFormUpdated | this.viewModel.ContactSales.Email() != this.existingBrandingConfiguration.ContactSales.Email |
            this.viewModel.ContactSales.Phone() != this.existingBrandingConfiguration.ContactSales.Phone;

        this.saveBrandingAction.enabled(isFormUpdated);
        this.resetBrandingAction.enabled(isFormUpdated);
    }, this);
}

//@ sourceURL=BrandingSetupPresenter.js
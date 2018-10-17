Microsoft.WebPortal.AddOrUpdateOfferPresenter = function (webPortal, feature, existingOffer) {
    /// <summary>
    /// Manages adding a new partner offer or updating an existing partner offer.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature for which this presenter is created.</param>
    /// <param name="existingOffer">An existing partner offer to update. If not passed, the presenter will create a new offer.</param>

    this.base.constructor.call(this, webPortal, feature, "AddOrUpdatePartnerOffer", "/Template/AddOrUpdateOffer/");
    this.offerToUpdate = existingOffer;
    this.isNewOffer = typeof this.offerToUpdate === 'undefined' || this.offerToUpdate === null;
    Globalize.culture(this.webPortal.Resources.Strings.CurrentLocale);

    this.viewModel = {
        ShowProgress: ko.observable(true),
        IsSet: ko.observable(false),
        Id: this.isNewOffer ? "" : existingOffer.Id,
        MicrosoftOffer: ko.observable(this.isNewOffer ? "" : existingOffer.MicrosoftOffer),
        Title: ko.observable(this.isNewOffer ? "" : existingOffer.Title),
        SubTitle: ko.observable(this.isNewOffer ? "" : existingOffer.SubTitle),
        Price: ko.observable(this.isNewOffer ? "" : Globalize.format(existingOffer.Price, "n")),
        Features: ko.observableArray([]),
        Summary: ko.observableArray([]),
        Logo: ko.observable(this.isNewOffer ? "" : existingOffer.LogoUri),
        Thumbnail: ko.observable(this.isNewOffer ? "" : existingOffer.ThumbnailUri)
    }

    if (!this.isNewOffer)
    {
        this.viewModel.Features(ko.utils.arrayMap(existingOffer.Features, function (feature) {
            return ko.observable(feature);
        }));

        this.viewModel.Summary(ko.utils.arrayMap(existingOffer.Summary, function (summary) {
            return ko.observable(summary);
        }));
    }

    this.viewModel.IsOfferAutoRenewableCaption = ko.computed(function () {
        if (this.viewModel.MicrosoftOffer()) {
            return this.viewModel.MicrosoftOffer().Offer.IsAutoRenewable ?
                this.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.OfferAutomaticallyRenewable:
                this.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.OfferManuallyRenewable;
        } else {
            return "";
        }
    }, this);

    this.viewModel.IsAvailableForPurchaseCaption = ko.computed(function () {
        if (this.viewModel.MicrosoftOffer()) {
            return this.viewModel.MicrosoftOffer().Offer.IsAvailableForPurchase ?
                this.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.OfferAvailableForPurchase:
                this.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.OfferUnavailableForPurchase;
        } else {
            return "";
        }
    }, this);

    this.viewModel.AllowedQuantityCaption = ko.computed(function () {
        if (this.viewModel.MicrosoftOffer()) {
            return this.viewModel.MicrosoftOffer().Offer.MinimumQuantity +
                this.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.To +
                this.viewModel.MicrosoftOffer().Offer.MaximumQuantity +
                this.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.Seats;
        } else {
            return "";
        }
    }, this);
}

// inherit TemplatePresenter
$WebPortal.Helpers.inherit(Microsoft.WebPortal.AddOrUpdateOfferPresenter, Microsoft.WebPortal.Core.TemplatePresenter);

Microsoft.WebPortal.AddOrUpdateOfferPresenter.prototype.onRender = function () {
    /// <summary>
    /// Called when the presenter is rendered but not shown yet.
    /// </summary>

    ko.applyBindings(this, $("#AddOrUpdateOfferContainer")[0]);

    this.GetMicrosoftOffers();

    var self = this;

    if (this.isNewOffer) {
        this.pickMicrosoftOfferAction = new Microsoft.WebPortal.Services.Action("pick-offer", this.webPortal.Resources.Strings.Plugins.AddOrUpdateOffer.PickOfferCaption, function (menuItem) {
            self.onSelectBaseOffer();
        }, "/Content/Images/Plugins/action-pick.png", this.webPortal.Resources.Strings.Plugins.AddOrUpdateOffer.PickOfferTooltip, null);

        this.webPortal.Services.Actions.add(this.pickMicrosoftOfferAction);
    }

    this.saveOfferAction = new Microsoft.WebPortal.Services.Action("save-offer", this.webPortal.Resources.Strings.Save, function (menuItem) {
        self.onSaveOffer();
    }, "/Content/Images/Plugins/action-save.png", this.webPortal.Resources.Strings.Plugins.AddOrUpdateOffer.SaveOfferCaption, null, !self.isNewOffer);

    this.webPortal.Services.Actions.add(this.saveOfferAction);
}

Microsoft.WebPortal.AddOrUpdateOfferPresenter.prototype.GetMicrosoftOffers = function () {
    /// <summary>
    /// Retrieves Microsoft offers from the server.
    /// </summary>

    if (this.microsoftOffers) {
        // we already have the offers
        return;
    }

    this.viewModel.ShowProgress(true);

    var offersFetchProgress = $.Deferred();
    this.webPortal.Session.fetchMicrosoftOffers(offersFetchProgress);

    var self = this;

    offersFetchProgress.done(function (microsoftOffers) {
        self.microsoftOffers = microsoftOffers.filter(function (item) {
            return item.Offer.Id.indexOf('MS-AZR-') === -1;
        });

        self.baseOffersViewModels = ko.utils.arrayMap(self.microsoftOffers, function (baseOffer) {
            return {
                OriginalOffer: baseOffer,
                Name: baseOffer.Offer.Name,
                Description: baseOffer.Offer.Description,
                Category: baseOffer.Offer.Category.Name,
                ThumbnailUri: baseOffer.ThumbnailUri            
            }
        });

        self.viewModel.IsSet(true);
    }).fail(function () {
        var notification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Error, self.webPortal.Resources.Strings.Plugins.AddOrUpdateOffer.MicrosoftOfferRetrievalErrorMessage);

        notification.buttons([
            Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.RETRY, self.webPortal.Resources.Strings.Retry, function () {
                notification.dismiss();

                // retry
                self.GetMicrosoftOffers();
            }),
            Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.CANCEL, self.webPortal.Resources.Strings.Cancel, function () {
                notification.dismiss();
            })
        ]);

        self.webPortal.Services.Notifications.add(notification);
    }).always(function () {
        self.viewModel.ShowProgress(false);
    });
}

Microsoft.WebPortal.AddOrUpdateOfferPresenter.prototype.onAddNewOfferFeature = function (index, self) {
    /// <summary>
    /// Adds a new offer feature.
    /// </summary>
    /// <param name="index">The index after which to add the new feature.</param>
    /// <param name="self">A reference to the presetner class.</param>

    self.viewModel.Features.splice(index + 1, 0, ko.observable(""));
}

Microsoft.WebPortal.AddOrUpdateOfferPresenter.prototype.onRemoveOfferFeature = function (index, self) {
    /// <summary>
    /// Removes an offer feature.
    /// </summary>
    /// <param name="index">The index at which to remove the feature.</param>
    /// <param name="self">A reference to the presetner class.</param>
    self.viewModel.Features.splice(index, 1);
}

Microsoft.WebPortal.AddOrUpdateOfferPresenter.prototype.onAddNewOfferSummary = function (index, self) {
    /// <summary>
    /// Adds a new offer summary point.
    /// </summary>
    /// <param name="index">The index after which to add the new summary point.</param>
    /// <param name="self">A reference to the presetner class.</param>

    self.viewModel.Summary.splice(index + 1, 0, ko.observable(""));
}

Microsoft.WebPortal.AddOrUpdateOfferPresenter.prototype.onRemoveOfferSummary = function (index, self) {
    /// <summary>
    /// Removes a summary point.
    /// </summary>
    /// <param name="index">The index at which to remove the summary point.</param>
    /// <param name="self">A reference to the presetner class.</param>

    self.viewModel.Summary.splice(index, 1);
}

Microsoft.WebPortal.AddOrUpdateOfferPresenter.prototype.onSaveOffer = function () {
    /// <summary>
    /// Invokes when the user clicks the save offer button.
    /// </summary>

    if (!$("#AddOrUpdateOfferForm").valid()) {
        return;
    }

    var offerPayload = {
        Id: this.viewModel.Id,
        MicrosoftOfferId: this.viewModel.MicrosoftOffer().Offer.Id,
        Title: this.viewModel.Title(),
        Subtitle: this.viewModel.SubTitle(),
        Price: 0.0
    }

    // Only save the culture neutral value for price in the backend. 
    Globalize.culture(this.webPortal.Resources.Strings.CurrentLocale);
    offerPayload.Price = Globalize.parseFloat(this.viewModel.Price());

    offerPayload.Features = ko.utils.arrayMap(this.viewModel.Features(), function (feature) {
        return feature();
    })

    offerPayload.Summary = ko.utils.arrayMap(this.viewModel.Summary(), function (summary) {
        return summary();
    })

    var saveOfferServerCall = null;
    var saveNotificationMessage = "";

    if (this.isNewOffer) {
        saveOfferServerCall = this.webPortal.ServerCallManager.create(this.feature,
            this.webPortal.Helpers.ajaxCall("api/AdminConsole/Offers", Microsoft.WebPortal.HttpMethod.Post, offerPayload), "AddNewOffer");
        saveNotificationMessage = this.webPortal.Resources.Strings.Plugins.AddOrUpdateOffer.SavingOfferMessage;
    }
    else {
        saveOfferServerCall = this.webPortal.ServerCallManager.create(this.feature,
            this.webPortal.Helpers.ajaxCall("api/AdminConsole/Offers", Microsoft.WebPortal.HttpMethod.Put, offerPayload), "AddNewOffer");
        saveNotificationMessage = this.webPortal.Resources.Strings.Plugins.AddOrUpdateOffer.UpdatingOfferMessage;
    }

    var offerSaveNotification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Progress,
        saveNotificationMessage);

    this.webPortal.Services.Notifications.add(offerSaveNotification);

    var self = this;

    var saveOfferOperation = function () {
        self.saveOfferAction.enabled(false);

        saveOfferServerCall.execute().done(function (updatedOfferInformation) {
            self.viewModel.Price(updatedOfferInformation.Price); // display reconciled price from server. 
            offerSaveNotification.type(Microsoft.WebPortal.Services.Notification.NotificationType.Success);
            offerSaveNotification.message(self.webPortal.Resources.Strings.Plugins.AddOrUpdateOffer.OfferSaveSuccessMessage);
            offerSaveNotification.buttons([
                Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.OK, "save-ok", function () {
                    offerSaveNotification.dismiss();
                })
            ]);

            if (self.isNewOffer) {
                if (!self.webPortal.Journey.retract()) {
                    self.webPortal.Journey.start(Microsoft.WebPortal.Feature.OfferList);
                }
            }
        }).fail(function (result, status, error) {
            self.webPortal.Helpers.displayRetryCancelErrorNotification(offerSaveNotification, self.webPortal.Resources.Strings.Plugins.AddOrUpdateOffer.OfferSaveErrorMessage, saveNotificationMessage, saveOfferOperation, function () {
            });
        }).always(function () {
            self.saveOfferAction.enabled(true);
        });
    };

    saveOfferOperation();
}

Microsoft.WebPortal.AddOrUpdateOfferPresenter.prototype.onSelectBaseOffer = function () {
    /// <summary>
    /// Called when the user wishes to select a Microsoft base offer.
    /// </summary>

    var self = this;

    var selectOfferButton = Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.OK, 1, function () {
        if (self.OfferSelectionWizardViewModel.offerList.getSelectedRows().length <= 0) {
            self.OfferSelectionWizardViewModel.errorMessage(self.webPortal.Resources.Strings.Plugins.AddOrUpdateOffer.SelectOfferErrorMessage);
            return;
        }

        self.viewModel.MicrosoftOffer(self.OfferSelectionWizardViewModel.offerList.getSelectedRows()[0].OriginalOffer);
        self.viewModel.Title(self.viewModel.MicrosoftOffer().Offer.Name);
        self.webPortal.Services.Dialog.hide();
        self.saveOfferAction.enabled(true);
    });

    var cancelButton = Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.CANCEL, 1, function () {
        self.webPortal.Services.Dialog.hide();
    });

    self.onSortChanged = function (field, direction) {
        var sortedOffers = this.baseOffersViewModels.sort(function (firstElement, secondElement) {
            if (firstElement[field] < secondElement[field]) {
                return direction === Microsoft.WebPortal.Views.List.SortDirection.Ascending ? -1 : 1;
            } else if (firstElement[field] > secondElement[field]) {
                return direction === Microsoft.WebPortal.Views.List.SortDirection.Ascending ? 1 : -1;
            } else {
                return 0;
            }
        });

        this.OfferSelectionWizardViewModel.offerList.set(sortedOffers);
    }

    this.OfferSelectionWizardViewModel = {
        offerList: new Microsoft.WebPortal.Views.List(self.webPortal, "#offerList", self),
        Offers: ko.observableArray(this.baseOffersViewModels),
        searchTerm: ko.observable(""),
        baseOffers: this.microsoftOffers,
        errorMessage: ko.observable(""),
        searchButtonClicked: function () {
            this.offerList.set(this.filteredItems());
            this.offerList.setComplete(true);
            this.offerList.renderer();
        }

 
    };
    this.OfferSelectionWizardViewModel.filteredItems = ko.computed(function () {
        var filter = this.searchTerm().toLowerCase();
        if (!filter) {
           return this.Offers();          
        } else {
            return ko.utils.arrayFilter(this.Offers(), function (item) {
                return item.Name.toLowerCase().indexOf(filter) !== -1;
            });
        }
    }, this.OfferSelectionWizardViewModel);
  
    // TODO: In later iterations, we will support sorting and filtering
    this.OfferSelectionWizardViewModel.offerList.setColumns([
        new Microsoft.WebPortal.Views.List.Column("Name", "min-width: 300px; width: 300px; white-space: normal;", true, false,
            self.webPortal.Resources.Strings.Plugins.AddOrUpdateOffer.Offer),
        new Microsoft.WebPortal.Views.List.Column("Category", "min-width: 100px; width: 100px;", true, false, self.webPortal.Resources.Strings.Plugins.AddOrUpdateOffer.Category),
        new Microsoft.WebPortal.Views.List.Column("Description", "min-width: 500px; white-space: normal;", false, false, self.webPortal.Resources.Strings.Plugins.AddOrUpdateOffer.Description)
    ]);

    this.OfferSelectionWizardViewModel.offerList.setEmptyListUI(self.webPortal.Resources.Strings.Plugins.AddOrUpdateOffer.EmptyMicrosoftOfferListMessage);
    this.OfferSelectionWizardViewModel.offerList.enableStatusBar(false);
    this.OfferSelectionWizardViewModel.offerList.setSelectionMode(Microsoft.WebPortal.Views.List.SelectionMode.Single);
    this.OfferSelectionWizardViewModel.offerList.setSorting("Name", Microsoft.WebPortal.Views.List.SortDirection.Ascending , true);
    this.OfferSelectionWizardViewModel.offerList.set(this.OfferSelectionWizardViewModel.filteredItems());
    this.OfferSelectionWizardViewModel.offerList.setComplete(true);

    this.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.DialogShown, this.onSelectBaseOfferWizardShown, this);

    this.webPortal.Services.Dialog.show("offerPicker-template", this.OfferSelectionWizardViewModel, [selectOfferButton, cancelButton]);
    this.webPortal.Services.Dialog.showProgress();
}

Microsoft.WebPortal.AddOrUpdateOfferPresenter.prototype.onSelectBaseOfferWizardShown = function (eventId, isShown) {
    /// <summary>
    /// Called when the dialog box is shown or hidden.
    /// </summary>
    /// <param name="eventId">The event ID.</param>
    /// <param name="isShown">Indicates whether the dialog is shown or hidden.</param>

    if (isShown) {
        // show the list and hide the progress bar once the dialog is shown
        this.OfferSelectionWizardViewModel.offerList.show();
        this.webPortal.Services.Dialog.hideProgress();
    }

    // stop listening to dialog box events
    this.webPortal.EventSystem.unsubscribe(Microsoft.WebPortal.Event.DialogShown, this.onSelectBaseOfferWizardShown, this);
}


//@ sourceURL=AddOrUpdateOfferPresenter.js
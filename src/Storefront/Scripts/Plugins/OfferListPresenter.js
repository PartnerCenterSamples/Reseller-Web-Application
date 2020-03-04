Microsoft.WebPortal.OfferListPresenter = function (webPortal, feature) {
    /// <summary>
    /// Manages the offer configuration UX.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature for which this presenter is created.</param>

    this.base.constructor.call(this, webPortal, feature, "OfferList", "/Template/OfferList/");

    this.viewModel = {
    };
};

// inherit TemplatePresenter
$WebPortal.Helpers.inherit(Microsoft.WebPortal.OfferListPresenter, Microsoft.WebPortal.Core.TemplatePresenter);

Microsoft.WebPortal.OfferListPresenter.prototype.onRender = function () {
    /// <summary>
    /// Called when the presenter is rendered but not shown yet.
    /// </summary>

    ko.applyBindings(this, $("#OfferListContainer")[0]);

    var self = this;

    self.onNotificationsCleared = function () {
        // the user cleared all the notifications, null our reference and recompute the enabled status of the delete button
        self.offersDeletionPrompt = null;
        self.deleteOffersAction.enabled(self.viewModel.offerList.selectedRows().length > 0);
    };

    // listen to the clear all notifications event
    self.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.NotificationsCleared, self.onNotificationsCleared, self);

    this.addOfferAction = new Microsoft.WebPortal.Services.Action("add-offer", this.webPortal.Resources.Strings.Add, function (menuItem) {
        // advance to the add/update offer feature
        self.webPortal.Journey.advance(Microsoft.WebPortal.Feature.AddOrUpdateOffers);
    }, "/Content/Images/Plugins/action-new.png", this.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.AddOfferCaption, null, true);

    this.deleteOffersAction = new Microsoft.WebPortal.Services.Action("delete-offers", this.webPortal.Resources.Strings.Delete, function (menuItem) {
        if (!self.offersDeletionPrompt) {
            self.deleteOffersAction.enabled(false);
            self.offersDeletionPrompt = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Warning,
                self.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.DeleteOffersPromptMessage, [
                    Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.YES, self.webPortal.Resources.Strings.Yes, function () {
                        self.offersDeletionPrompt.dismiss();
                        self._deleteSelectedOffers();
                    }),
                    Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.NO, self.webPortal.Resources.Strings.No, function () {
                        self.offersDeletionPrompt.dismiss();
                        self.offersDeletionPrompt = null;
                        self.deleteOffersAction.enabled(self.viewModel.offerList.selectedRows().length > 0);
                    })
                ]);

            self.webPortal.Services.Notifications.add(self.offersDeletionPrompt);
        }

    }, "/Content/Images/Plugins/action-delete.png", this.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.DeleteOffersCaption, null, false);

    this.webPortal.Services.Actions.add(this.addOfferAction);
    this.webPortal.Services.Actions.add(this.deleteOffersAction);

    this.viewModel.offerList = new Microsoft.WebPortal.Views.List(this.webPortal, "#offerList", this);

    this.viewModel.offerList.setColumns([
        new Microsoft.WebPortal.Views.List.Column(this.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.MicrosoftOfferListColumnCaption,
            "white-space: normal;", false, true, null, null, null, "microsoftOfferListItem-template"),
        new Microsoft.WebPortal.Views.List.Column(this.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.CustomizationOfferListColumnCaption,
            "white-space: normal;", false, false, null, null, null, "partnerOfferListItem-template")
    ]);

    this.viewModel.offerList.setEmptyListUI(this.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.EmptyOfferListMessage);
    this.viewModel.offerList.enableStatusBar(false);
    this.viewModel.offerList.setSelectionMode(Microsoft.WebPortal.Views.List.SelectionMode.Multiple);

    this.viewModel.offerList.setComplete(true);
};

Microsoft.WebPortal.OfferListPresenter.prototype.onShow = function () {
    /// <summary>
    /// Called when the UX is shown.
    /// </summary>
    this._retrievePartnerOffers(null);
};

Microsoft.WebPortal.OfferListPresenter.prototype.onDeactivate = function () {
    /// <summary>
    /// Called when the presenter is no longer active.
    /// </summary>

    // stop listening to the event
    this.webPortal.EventSystem.unsubscribe(Microsoft.WebPortal.Event.NotificationsCleared, this.onNotificationsCleared, this);
};

Microsoft.WebPortal.OfferListPresenter.prototype.onSelectionChanged = function (selectedRows) {
    /// <summary>
    /// Called when the list selection changes.
    /// </summary>
    /// <param name="selectedRows">The selected rows.</param>

    this.deleteOffersAction.enabled(selectedRows.length > 0 && !this.offersDeletionPrompt);

    if (selectedRows.length <= 0 && this.offersDeletionPrompt) {
        this.offersDeletionPrompt.dismiss();
        this.offersDeletionPrompt = null;
    }
};

Microsoft.WebPortal.OfferListPresenter.prototype.onCellClicked = function (column, row) {
    /// <summary>
    /// Called when the user clicks on a list cell.
    /// </summary>
    /// <param name="column">The column that was clicked.</param>
    /// <param name="row">The row where the click happened.</param>

    var offerToUpdate = {
        Id: row.PartnerOffer.Id,
        MicrosoftOffer: row.MicrosoftOffer,
        Title: row.PartnerOffer.Title,
        SubTitle: row.PartnerOffer.Subtitle,
        Price: row.PartnerOffer.Price,
        Features: row.PartnerOffer.Features,
        Summary: row.PartnerOffer.Summary,
        Logo: row.PartnerOffer.LogoUri,
        Thumbnail: row.PartnerOffer.ThumbnailUri,
        DisplayIndex: row.PartnerOffer.DisplayIndex
    };

    // go to the update offer feature and pass it the clicked offer
    this.webPortal.Journey.advance(Microsoft.WebPortal.Feature.AddOrUpdateOffers, offerToUpdate);
};

Microsoft.WebPortal.OfferListPresenter.prototype._deleteSelectedOffers = function () {
    /// <summary>
    /// Deletes the selected offers in the list.
    /// </summary>

    Globalize.culture(this.webPortal.Resources.Strings.CurrentLocale);
    var selectedRows = this.viewModel.offerList.selectedRows();

    if (!selectedRows || selectedRows.length <= 0) {
        return;
    }

    var offersToDelete = [];

    for (var i in selectedRows) {
        offersToDelete.push(selectedRows[i].PartnerOffer);
    }

    var deletePartnerOffersServerCall = this.webPortal.ServerCallManager.create(this.feature,
        this.webPortal.Helpers.ajaxCall("api/AdminConsole/Offers/Delete", Microsoft.WebPortal.HttpMethod.Post, offersToDelete, Microsoft.WebPortal.ContentType.Json), "DeletePartnerOffers");

    var self = this;
    var offersDeletionNotification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Progress,
        self.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.DeletingOfferMessage);

    this.webPortal.Services.Notifications.add(offersDeletionNotification);

    var deletePartnerOffers = function () {
        self.deleteOffersAction.enabled(false);

        deletePartnerOffersServerCall.execute().done(function (partnerOffers) {
            self.viewModel.Offers = [];

            // aggregate the partner and microsoft offers
            for (var i in partnerOffers) {
                var offerToPush = {
                    PartnerOffer: partnerOffers[i],
                    FormattedPrice: Globalize.format(partnerOffers[i].Price, "c"),
                    MicrosoftOffer: function () {
                        for (var j in self.microsoftOffers) {
                            if (partnerOffers[i].MicrosoftOfferId === self.microsoftOffers[j].Offer.Id) {
                                return self.microsoftOffers[j];
                            }
                        }
                        return null;
                    }()
                };

                // TODO :: Handle Microsoft offer being pulled back due to EOL.
                // Temporary fix - Do not display this partner offer for further configuration. Need a better way to handle this. 
                if (offerToPush.MicrosoftOffer !== null) {
                    offerToPush.IsOfferAutoRenewableCaption = offerToPush.MicrosoftOffer.Offer.IsAutoRenewable ?
                        self.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.OfferAutomaticallyRenewable :
                        self.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.OfferManuallyRenewable;

                    offerToPush.IsAvailableForPurchaseCaption = offerToPush.MicrosoftOffer.Offer.IsAvailableForPurchase ?
                        self.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.OfferAvailableForPurchase :
                        self.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.OfferUnavailableForPurchase;

                    offerToPush.AllowedQuantityCaption = offerToPush.MicrosoftOffer.Offer.MinimumQuantity +
                        self.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.To +
                        offerToPush.MicrosoftOffer.Offer.MaximumQuantity +
                        self.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.Seats;

                    self.viewModel.Offers.push(offerToPush);
                }
            }

            self.viewModel.offerList.set(self.viewModel.Offers);
            offersDeletionNotification.type(Microsoft.WebPortal.Services.Notification.NotificationType.Success);
            offersDeletionNotification.message(self.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.OfferDeletionConfirmation);
            offersDeletionNotification.buttons([
                Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.OK, self.webPortal.Resources.Strings.OK, function () {
                    offersDeletionNotification.dismiss();
                })
            ]);

            self.offersDeletionPrompt = null;
        }).fail(function (result, status, error) {
            self.webPortal.Helpers.displayRetryCancelErrorNotification(offersDeletionNotification, self.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.OfferDeletionFailure,
                self.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.DeletingOfferMessage, deletePartnerOffers, function () {
                    self.offersDeletionPrompt = null;
                    self.deleteOffersAction.enabled(self.viewModel.offerList.selectedRows().length > 0);
                });
        });
    };

    deletePartnerOffers();
};

Microsoft.WebPortal.OfferListPresenter.prototype._retrievePartnerOffers = function () {
    /// <summary>
    /// Fetches partner offers from the server.
    /// </summary>

    var self = this;
    Globalize.culture(this.webPortal.Resources.Strings.CurrentLocale);
    var getPartnerOffers = function (offerSaveNotification) {
        self.webPortal.ContentPanel.showProgress();

        var microsoftOffersProgress = $.Deferred();

        self.webPortal.Session.fetchMicrosoftOffers(microsoftOffersProgress);

        var getPartnerOffersServerCall = self.webPortal.ServerCallManager.create(self.feature,
            self.webPortal.Helpers.ajaxCall("api/AdminConsole/Offers", Microsoft.WebPortal.HttpMethod.Get), "GetPartnerOffers");

        var partnerOffersProgress = getPartnerOffersServerCall.execute();

        $.when(microsoftOffersProgress, partnerOffersProgress).then(function (microsoftOffers, partnerOffers) {
            // both calls were successful
            self.webPortal.ContentPanel.hideProgress();

            partnerOffers = partnerOffers[0];

            self.microsoftOffers = microsoftOffers;

            self.viewModel.Offers = [];

            // aggregate the partner and microsoft offers
            for (var i in partnerOffers) {

                var offerToPush = {
                    PartnerOffer: partnerOffers[i],
                    FormattedPrice: Globalize.format(partnerOffers[i].Price, "c"),
                    DisplayIndex: partnerOffers[i].DisplayIndex,
                    MicrosoftOffer: function () {
                        for (var j in microsoftOffers) {
                            if (partnerOffers[i].MicrosoftOfferId === microsoftOffers[j].Offer.Id) {
                                return microsoftOffers[j];
                            }
                        }
                        return null;
                    }()
                };

                // TODO :: Handle Microsoft offer being pulled back due to EOL.
                // Temporary fix - Do not display this partner offer for further configuration. Need a better way to handle this. 
                if (offerToPush.MicrosoftOffer !== null) {
                    offerToPush.IsOfferAutoRenewableCaption = offerToPush.MicrosoftOffer.Offer.IsAutoRenewable ?
                        self.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.OfferAutomaticallyRenewable :
                        self.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.OfferManuallyRenewable;

                    offerToPush.IsAvailableForPurchaseCaption = offerToPush.MicrosoftOffer.Offer.IsAvailableForPurchase ?
                        self.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.OfferAvailableForPurchase :
                        self.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.OfferUnavailableForPurchase;

                    offerToPush.AllowedQuantityCaption = offerToPush.MicrosoftOffer.Offer.MinimumQuantity +
                        self.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.To +
                        offerToPush.MicrosoftOffer.Offer.MaximumQuantity +
                        self.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.Seats;

                    self.viewModel.Offers.push(offerToPush);
                }
            }

            self.viewModel.offerList.set(self.viewModel.Offers);
            self.viewModel.offerList.show();
        }, function () {
            self.webPortal.ContentPanel.hideProgress();
            self.webPortal.Helpers.displayRetryCancelErrorNotification(offerSaveNotification,
                self.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.OffersFetchFailure,
                self.webPortal.Resources.Strings.Plugins.AdminOfferConfiguration.OffersFetchProgress, getPartnerOffers);
        });
    };

    getPartnerOffers();
};

//@ sourceURL=OfferListPresenter.js
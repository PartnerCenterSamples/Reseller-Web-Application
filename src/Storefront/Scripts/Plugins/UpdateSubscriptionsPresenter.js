Microsoft.WebPortal.UpdateSubscriptionsPresenter = function (webPortal, feature, subscriptionItem) {
    /// <summary>
    /// Manages the offers experience. 
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature for which this presenter is created.</param>
    /// <param name="subscriptionItem">The customer subscription item which is being edited.</param>

    this.base.constructor.call(this, webPortal, feature, "Update Subscriptions", "/Template/UpdateSubscriptions/");
    var self = this;
    self.isPosting = false;

    // these viewModel fields are used to control how the form elements behave. 
    // viewModel.isUpdateSubscription is true when form is for updating an existing subscription. 
    // viewModel.isRenewSubscription  is true when form is for renewing an existing subscription.

    this.viewModel = subscriptionItem;

    // set up quantity field in the form. 
    self.viewModel.Quantity = ko.observable(1);
    if (self.viewModel.isRenewSubscription) {
        self.viewModel.Quantity(self.viewModel.LicensesTotal);
    }

    // setting up page title based on whether page is loaded to renew or add seats to a subscription. 
    this.viewModel.PageTitle = "";
    if (this.viewModel.isUpdateSubscription) {
        this.viewModel.PageTitle = self.webPortal.Resources.Strings.Plugins.UpdateSubscriptionPage.AddMoreSeatsTitleText;
    } else if (this.viewModel.isRenewSubscription) {
        this.viewModel.PageTitle = self.webPortal.Resources.Strings.Plugins.UpdateSubscriptionPage.RenewSubscriptionTitleText;
    }

    this.viewModel.OperationType = 1;
    if (this.viewModel.isUpdateSubscription) {
        this.viewModel.OperationType = Microsoft.WebPortal.CommerceOperationType.AdditionalSeatsPurchase; // AddSeats.
    } else if (this.viewModel.isRenewSubscription) {
        this.viewModel.OperationType = Microsoft.WebPortal.CommerceOperationType.Renewal; // RenewSubscription.
    }

    this.viewModel.ShowProgress = ko.observable(true);
    this.viewModel.IsSet = ko.observable(false);

    // form event handlers follow. 
    this.onCancelClicked = function () {
        webPortal.Journey.retract();
    };

    this.onFormPostClicked = function () {
        // create order payload & post to order api. 
        // on success, order api will return a redirect uri for payment gateway 
        //  -- redirect UX to payment gateway URI. 

        var thisNotification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Progress, self.webPortal.Resources.Strings.Plugins.UpdateSubscriptionPage.PreparingOrderAndRedirectingMessage);
        self.webPortal.Services.Notifications.add(thisNotification);

        new Microsoft.WebPortal.Utilities.RetryableServerCall(this.webPortal.Helpers.ajaxCall("api/Order/Prepare", Microsoft.WebPortal.HttpMethod.Post, {
            Subscriptions: this.getSubscriptions(),
            OperationType: self.viewModel.OperationType
        }, Microsoft.WebPortal.ContentType.Json, 120000), "api/Order/Prepare", []).execute()
            .done(function (result) {
                // if result is a Uri for PreApproved transaction then build context and advance the journey in the portal to ProcessOrder feature. 
                var pairs = result.slice(1).split('&');
                var resultPairs = {};
                pairs.forEach(function (pair) {
                    pair = pair.split('=');
                    resultPairs[pair[0]] = decodeURIComponent(pair[1] || '');
                });

                var queryStringParams = JSON.parse(JSON.stringify(resultPairs));
                var orderContext = {
                    paymentId: queryStringParams["paymentId"],
                    PayerID: queryStringParams["PayerID"],
                    customerId: queryStringParams["customerId"],
                    orderId: queryStringParams["oid"],
                    oid: queryStringParams["oid"],
                    payment: queryStringParams["payment"],
                    txStatus: queryStringParams["payment"]
                };

                // hand it off to the subscriptions page.        
                thisNotification.dismiss();

                if (queryStringParams["paymentId"] !== null && queryStringParams["paymentId"].toLowerCase() === "preapproved") {
                    // hand it off to the order processing presenter
                    self.webPortal.Journey.advance(Microsoft.WebPortal.Feature.ProcessOrder, orderContext);
                } else {
                    // we need to now redirect to paypal based on the response from the API.             
                    window.location = result;
                }
            })
            .fail(function (result, status, error) {
                thisNotification.type(Microsoft.WebPortal.Services.Notification.NotificationType.Error);
                thisNotification.buttons([
                    Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.OK, self.webPortal.Resources.Strings.OK, function () {
                        thisNotification.dismiss();
                    })
                ]);

                var errorPayload = JSON.parse(result.responseText);

                if (errorPayload) {
                    switch (errorPayload.ErrorCode) {
                        case Microsoft.WebPortal.ErrorCode.InvalidInput:
                            thisNotification.message(self.webPortal.Resources.Strings.Plugins.UpdateSubscriptionPage.InvalidInputErrorPrefix + errorPayload.Details.ErrorMessage);
                            break;
                        case Microsoft.WebPortal.ErrorCode.DownstreamServiceError:
                            thisNotification.message(self.webPortal.Resources.Strings.Plugins.UpdateSubscriptionPage.DownstreamErrorPrefix + errorPayload.Details.ErrorMessage);
                            break;
                        default:
                            thisNotification.message(self.webPortal.Resources.Strings.Plugins.UpdateSubscriptionPage.OrderUpdateFailureMessag);
                            break;
                    }
                } else {
                    thisNotification.message(self.webPortal.Resources.Strings.Plugins.UpdateSubscriptionPage.OrderUpdateFailureMessage);
                }
            })
            .always(function () {
                self.isPosting = false;
            });
    };

    this.getSubscriptions = function () {
        var orders = [];

        orders.push({
            SubscriptionId: self.viewModel.SubscriptionId,  // required for the add seats & renew calls.                         
            Quantity: self.viewModel.Quantity()             // this.addSubscriptionsView.subscriptionsList.rows()[i].quantity()
        });

        return orders;
    };
};

// inherit BasePresenter
$WebPortal.Helpers.inherit(Microsoft.WebPortal.UpdateSubscriptionsPresenter, Microsoft.WebPortal.Core.TemplatePresenter);

Microsoft.WebPortal.UpdateSubscriptionsPresenter.prototype.onActivate = function () {
    /// <summary>
    /// Called when the presenter is activated.
    /// </summary>
};

Microsoft.WebPortal.UpdateSubscriptionsPresenter.prototype.onRender = function () {
    /// <summary>
    /// Called when the presenter is about to be rendered.
    /// </summary>

    ko.applyBindings(this, $("#Form")[0]);

    var self = this;

    var getPortalOfferDetails = function () {
        self.viewModel.IsSet(false);
        var portaloffersFetchProgress = $.Deferred();
        self.webPortal.Session.fetchPortalOffers(portaloffersFetchProgress);
        portaloffersFetchProgress.done(function (portalOffers) {
            // find & set up the portal offer from the cached offers in the Portal. 
            var matchedOffer = ko.utils.arrayFirst(self.webPortal.Session.IdMappedPortalOffers, function (item) {
                return item.Id.toLowerCase() === self.viewModel.PortalOfferId.toLowerCase();
            });
            self.viewModel.portalOffer = matchedOffer.OriginalOffer;

            // pricePerSeat - Manages the price per seat. Will either be normal offer price or pro rated price provided by server. 
            self.viewModel.pricePerSeat = 0;
            Globalize.culture(self.webPortal.Resources.Strings.CurrentLocale);

            // globalize the price variables. 
            if (self.viewModel.isUpdateSubscription) {
                self.viewModel.pricePerSeat = Globalize.format(self.viewModel.SubscriptionProRatedPrice, "c");
            }
            if (self.viewModel.isRenewSubscription) {
                self.viewModel.pricePerSeat = Globalize.format(self.viewModel.portalOffer.Price, "c");
            }

            // Globalize the pricePerSeat using currency format.             
            // set up the total charge form field computed value. 
            self.viewModel.TotalCharge = ko.computed(function () {
                // globalize the total using currency format. 
                Globalize.culture(self.webPortal.Resources.Strings.CurrentLocale);

                var total = 0;
                total = self.viewModel.Quantity() * Globalize.parseFloat(self.viewModel.pricePerSeat);

                return Globalize.format(total, "c");
            });
            self.viewModel.IsSet(true);
        }).fail(function (result, status, error) {
            var notification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Error,
                self.webPortal.Resources.Strings.Plugins.UpdateSubscriptionPage.CouldNotRetrieveOffer);

            notification.buttons([
                Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.RETRY, self.webPortal.Resources.Strings.Retry, function () {
                    notification.dismiss();
                    getPortalOfferDetails();
                }),
                Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.CANCEL, self.webPortal.Resources.Strings.Cancel, function () {
                    notification.dismiss();
                })
            ]);

            self.webPortal.Services.Notifications.add(notification);
        }).always(function () {
            self.viewModel.ShowProgress(false);
        });
    };

    getPortalOfferDetails();
};

Microsoft.WebPortal.UpdateSubscriptionsPresenter.prototype.onShow = function () {
    /// <summary>
    /// Called when content is shown.
    /// </summary>    
};

//@ sourceURL=UpdateSubscriptionsPresenter.js
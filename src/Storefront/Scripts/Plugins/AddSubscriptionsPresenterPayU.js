Microsoft.WebPortal.AddSubscriptionsPresenter = function (webPortal, feature, context) {
    /// <summary>
    /// Manages the offers experience. 
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature for which this presenter is created.</param>
    this.base.constructor.call(this, webPortal, feature, "Add Subscriptions", "/Template/AddSubscriptions/");

    this.addSubscriptionsView = new Microsoft.WebPortal.Views.AddSubscriptionsView(webPortal, "#AddSubscriptionsViewContainer", context);

    this.onCancelClicked = function () {
        webPortal.Journey.retract();
    }

    var self = this;
    var isPosting = false;
    self.OperationType = Microsoft.WebPortal.CommerceOperationType.NewPurchase; //PurchaseSubscriptions.

    this.getOrders = function () {
        var orders = [];

        for (var i in this.addSubscriptionsView.subscriptionsList.rows()) {
            orders.push({
                OfferId: this.addSubscriptionsView.subscriptionsList.rows()[i].offer.Id,
                SubscriptionId: this.addSubscriptionsView.subscriptionsList.rows()[i].offer.Id,
                Quantity: this.addSubscriptionsView.subscriptionsList.rows()[i].quantity()
            });
        }
        return orders;
    }

    this.onSubmitClicked = function () {
        if (isPosting) {
            return;
        }

        if ($("#Form").valid()) {
            if (self.addSubscriptionsView.subscriptionsList.rows().length <= 0) {
                self.webPortal.Services.Dialog.show("emptyOffersErrorMessage-template", {}, [
                    Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.OK, "ok-Button", function () {
                        self.webPortal.Services.Dialog.hide();
                    })
                ]);

                return;
            }

            isPosting = true;

            var notification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Progress, self.webPortal.Resources.Strings.Plugins.AddSubscriptionPage.PreparingOrderAndRedirectingMessage);
            self.webPortal.Services.Notifications.add(notification);

            new Microsoft.WebPortal.Utilities.RetryableServerCall(this.webPortal.Helpers.ajaxCall("api/Order/Prepare", Microsoft.WebPortal.HttpMethod.Post, {
                Subscriptions: self.getOrders(),
                OperationType: self.OperationType
                // CustomerId: registeredCustomer.MicrosoftId // Will be retrieved from loggin in principle.              
            }, Microsoft.WebPortal.ContentType.Json, 120000),
                "AddSubscriptions", []).execute()
                .done(function (result) {
                    if (result.indexOf('PreApproved') > -1) {
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
                        notification.dismiss();

                        if (queryStringParams["paymentId"] !== null && queryStringParams["paymentId"].toLowerCase() === "preapproved") {
                            // hand it off to the order processing presenter
                            self.webPortal.Journey.advance(Microsoft.WebPortal.Feature.ProcessOrder, orderContext);
                        } else {
                            // we need to now redirect to paypal based on the response from the API.             
                            window.location = result;
                        }
                    } else {
                        $('body').html(result);
                    }
                })
                .fail(function (result, status, error) {
                    notification.type(Microsoft.WebPortal.Services.Notification.NotificationType.Error);
                    notification.buttons([
                        Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.OK, self.webPortal.Resources.Strings.OK, function () {
                            notification.dismiss();
                        })
                    ]);

                    var errorPayload = JSON.parse(result.responseText);

                    if (errorPayload) {
                        switch (errorPayload.ErrorCode) {
                            case Microsoft.WebPortal.ErrorCode.InvalidInput:
                                notification.message(self.webPortal.Resources.Strings.Plugins.AddSubscriptionPage.InvalidInputErrorPrefix + errorPayload.Details.ErrorMessage);
                                break;
                            case Microsoft.WebPortal.ErrorCode.DownstreamServiceError:
                                notification.message(self.webPortal.Resources.Strings.Plugins.AddSubscriptionPage.DownstreamErrorPrefix + errorPayload.Details.ErrorMessage);
                                break;
                            default:
                                notification.message(self.webPortal.Resources.Strings.Plugins.AddSubscriptionPage.OrderAddFailureMessage);
                                break;
                        }
                    } else {
                        notification.message(self.webPortal.Resources.Strings.Plugins.AddSubscriptionPage.OrderAddFailureMessage);
                    }
                })
                .always(function () {
                    isPosting = false;
                });
        } else {
            // the form is invalid
        }
    }
}

// inherit BasePresenter
$WebPortal.Helpers.inherit(Microsoft.WebPortal.AddSubscriptionsPresenter, Microsoft.WebPortal.Core.TemplatePresenter);

Microsoft.WebPortal.AddSubscriptionsPresenter.prototype.onActivate = function () {
    /// <summary>
    /// Called when the presenter is activated.
    /// </summary>
}

Microsoft.WebPortal.AddSubscriptionsPresenter.prototype.onRender = function () {
    /// <summary>
    /// Called when the presenter is about to be rendered.
    /// </summary>

    ko.applyBindings(this, $("#Form")[0]);

    this.addSubscriptionsView.render();
}

Microsoft.WebPortal.AddSubscriptionsPresenter.prototype.onShow = function () {
    /// <summary>
    /// Called when content is shown.
    /// </summary>

    this.addSubscriptionsView.show();

    // show the offers dialog if there is no row set from parent page. 
    if (this.addSubscriptionsView.subscriptionsList.rows().length <= 0) {
        this.addSubscriptionsView.onAddOfferClicked();
    }
}

Microsoft.WebPortal.AddSubscriptionsPresenter.prototype.onDeactivate = function () {
    /// <summary>
    /// Called when the presenter is no longer active.
    /// </summary>

    // hide the offers dialog in case it was shown
    this.webPortal.Services.Dialog.hide();
}

//@ sourceURL=AddSubscriptionsPresenter.js
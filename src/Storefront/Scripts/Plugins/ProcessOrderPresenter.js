Microsoft.WebPortal.ProcessOrderPresenter = function (webPortal, feature, processOrderViewModel) {
    /// <summary>
    /// Shows the registration confirmation page.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature for which this presenter is created.</param>
    /// <param name="processOrderViewModel">The order processing view model.</param>
    this.base.constructor.call(this, webPortal, feature, "Order Processor", "/Template/ProcessOrder/");
    var self = this;

    function QueryStringToJSON() {
        var pairs = window.location.hash.slice(1).split('&');

        var result = {};
        pairs.forEach(function (pair) {
            pair = pair.split('=');
            result[pair[0]] = decodeURIComponent(pair[1] || '');
        });

        return JSON.parse(JSON.stringify(result));
    }

    var queryStringParams;
    if (processOrderViewModel !== null && processOrderViewModel.paymentId.toLowerCase() === "preapproved") {
        queryStringParams = processOrderViewModel;
    }
    else {
        queryStringParams = QueryStringToJSON();
    }

    self.viewModel = {
        paymentId: queryStringParams["paymentId"],
        PayerID: queryStringParams["PayerID"],
        customerId: queryStringParams["customerId"],
        orderId: queryStringParams["oid"],
        txStatus: queryStringParams["payment"],
        PageTitle: ko.observable(""),
        Subscriptions: ko.observable(""),
        TotalPrice: ko.observable(""),
        showDoneButton: ko.observable(false),
        showSubscriptions: ko.observable(false),
        nextJourney: Microsoft.WebPortal.Feature.CustomerAccount,
        CustomerRegistrationInfo: ko.observable(""),
        Address: ko.observable(""),
        ContactInformation: ko.observable("")
    };

    self.apiUrl = "";
    var existingCustomerOrderUrl = "api/Order/Process" + "?paymentId=" + self.viewModel.paymentId + "&payerId=" + self.viewModel.PayerID + "&orderId=" + self.viewModel.orderId;
    var newCustomerOrderUrl = "api/Order/NewCustomerProcessOrder" + "?customerId=" + self.viewModel.customerId + "&paymentId=" + self.viewModel.paymentId + "&payerId=" + self.viewModel.PayerID;

    if (self.viewModel.customerId !== null) {
        self.apiUrl = newCustomerOrderUrl;
        self.viewModel.nextJourney = Microsoft.WebPortal.Feature.Home;
    } else {
        self.apiUrl = existingCustomerOrderUrl;
        self.viewModel.nextJourney = Microsoft.WebPortal.Feature.CustomerAccount;
    }

    this.onDoneClicked = function () {
        self.webPortal.Journey.start(self.viewModel.nextJourney);
    };
};

// inherit BasePresenter
$WebPortal.Helpers.inherit(Microsoft.WebPortal.ProcessOrderPresenter, Microsoft.WebPortal.Core.TemplatePresenter);

Microsoft.WebPortal.ProcessOrderPresenter.prototype.onRender = function () {
    /// <summary>
    /// Called when the presenter is about to be rendered.
    /// </summary>

    var self = this;
    ko.applyBindings(this, $("#ProcessOrderContainer")[0]);

    var processOrder = function () {
        // If paypal sent failure then display message and go back to subscriptions page. 
        if (self.viewModel.txStatus.toLowerCase() === "failure") {
            self.viewModel.showDoneButton(true);
            if (self.viewModel.customerId !== null) {
                self.viewModel.PageTitle(self.webPortal.Resources.Strings.Plugins.ProcessOrderPage.NewCustomerProcessOrderFailureReceivingPaymentMessage);
            } else {
                self.viewModel.PageTitle(self.webPortal.Resources.Strings.Plugins.ProcessOrderPage.PaymentReceiptFailureNotification);
            }
        }
        else { // success from payment gateway.            
            var pageTitleText;
            if (self.viewModel.paymentId.toLowerCase() === "preapproved") {
                pageTitleText = self.webPortal.Resources.Strings.Plugins.ProcessOrderPage.ProcessingPreApprovedTxNotification;
            }

            self.viewModel.PageTitle(pageTitleText);

            var thisNotification;
            if (self.viewModel.customerId !== null) {
                thisNotification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Progress, self.webPortal.Resources.Strings.Plugins.ProcessOrderPage.ProcessingOrderMessage);
            }
            else {
                thisNotification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Progress, self.webPortal.Resources.Strings.Plugins.ProcessOrderPage.ExistingCustomerProcessingOrderMessage);
            }

            self.webPortal.Services.Notifications.add(thisNotification);

            new Microsoft.WebPortal.Utilities.RetryableServerCall(self.webPortal.Helpers.ajaxCall(self.apiUrl, Microsoft.WebPortal.HttpMethod.Get, {
            }, Microsoft.WebPortal.ContentType.Json, 120000), self.apiUrl, []).execute()
                .done(function (result) {
                    // hand it off to the subscriptions page.        
                    thisNotification.dismiss();

                    if (self.viewModel.customerId !== null) {
                        self.viewModel.showDoneButton(true);
                        self.viewModel.showSubscriptions(true);
                        self.viewModel.Subscriptions(result.Subscriptions);
                        self.viewModel.TotalPrice(result.SummaryTotal);
                        self.viewModel.CustomerRegistrationInfo(result.CustomerViewModel);

                        var addressLine = result.CustomerViewModel.AddressLine1;
                        if (result.CustomerViewModel.AddressLine2) {
                            addressLine += " " + result.CustomerViewModel.AddressLine2;
                        }

                        var AddressInfo = [
                            addressLine,
                            result.CustomerViewModel.City + ", " + result.CustomerViewModel.State + " " + result.CustomerViewModel.ZipCode,
                            result.CustomerViewModel.Country
                        ];

                        var ContactInfo = [
                            result.CustomerViewModel.FirstName + " " + result.CustomerViewModel.LastName,
                            result.CustomerViewModel.Email,
                            result.CustomerViewModel.Phone
                        ];

                        self.viewModel.Address(AddressInfo);
                        self.viewModel.ContactInformation(ContactInfo);

                    } else {
                        // all processed so push to subscriptions page. 
                        self.webPortal.Journey.start(self.viewModel.nextJourney);
                    }
                })
                .fail(function (result, status, error) {
                    thisNotification.type(Microsoft.WebPortal.Services.Notification.NotificationType.Error);
                    thisNotification.buttons([
                        Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.OK, self.webPortal.Resources.Strings.OK, function () {
                            thisNotification.dismiss();
                        })
                    ]);

                    self.viewModel.showDoneButton(true);
                    self.viewModel.PageTitle(self.webPortal.Resources.Strings.Plugins.ProcessOrderPage.OrderProcessingFailureNotification);

                    var errorPayload = JSON.parse(result.responseText);
                    if (errorPayload) {
                        switch (errorPayload.ErrorCode) {
                            case Microsoft.WebPortal.ErrorCode.AlreadyExists:
                                thisNotification.message(self.webPortal.Resources.Strings.Plugins.ProcessOrderPage.CannotAddExistingSubscriptionError);
                                break;
                            case Microsoft.WebPortal.ErrorCode.InvalidInput:
                                thisNotification.message(self.webPortal.Resources.Strings.Plugins.ProcessOrderPage.InvalidInputErrorPrefix + errorPayload.Details.ErrorMessage);
                                break;
                            case Microsoft.WebPortal.ErrorCode.DownstreamServiceError:
                                thisNotification.message(self.webPortal.Resources.Strings.Plugins.ProcessOrderPage.DownstreamErrorPrefix + errorPayload.Details.ErrorMessage);
                                break;
                            case Microsoft.WebPortal.ErrorCode.PaymentGatewayPaymentError:
                                thisNotification.message(self.webPortal.Resources.Strings.Plugins.ProcessOrderPage.PaymentGatewayErrorPrefix + errorPayload.Details.ErrorMessage);
                                break;
                            case Microsoft.WebPortal.ErrorCode.PaymentGatewayIdentityFailureDuringPayment:
                            case Microsoft.WebPortal.ErrorCode.PaymentGatewayFailure:
                                thisNotification.message(errorPayload.Details.ErrorMessage);
                                break;
                            default:
                                thisNotification.message(self.webPortal.Resources.Strings.Plugins.ProcessOrderPage.OrderFailureMessage);
                                break;
                        }
                    } else {
                        thisNotification.message(self.webPortal.Resources.Strings.Plugins.ProcessOrderPage.OrderFailureMessage);
                    }
                })
                .always(function () {
                    self.isPosting = false;
                });
        }
    };

    processOrder();
};

//@ sourceURL=ProcessOrderPresenter.js
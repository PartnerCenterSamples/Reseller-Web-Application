Microsoft.WebPortal.RegistrationConfirmationPresenter = function (webPortal, feature, registrationConfirmationViewModel) {
    /// <summary>
    /// Shows the registration confirmation page.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature for which this presenter is created.</param>
    /// <param name="registrationConfirmationViewModel">The registration confirmation view model.</param>
    this.base.constructor.call(this, webPortal, feature, "Home", "/Template/RegistrationConfirmation/");

    var self = this;
    self.viewModel = registrationConfirmationViewModel;

    // object to pass to order API.
    self.viewModel.orderToPlace = {
        Subscriptions: registrationConfirmationViewModel.SubscriptionsToOrder,
        OperationType: Microsoft.WebPortal.CommerceOperationType.NewPurchase, //PurchaseSubscriptions.
        CustomerId: registrationConfirmationViewModel.MicrosoftId // populate the Customer Id.             
    };

    var addressLine = this.viewModel.AddressLine1;
    if (this.viewModel.AddressLine2) {
        addressLine += " " + this.viewModel.AddressLine2;
    }

    this.viewModel.Address = [
        addressLine,
        this.viewModel.City + ", " + this.viewModel.State + " " + this.viewModel.ZipCode,
        this.viewModel.Country
    ];

    this.viewModel.ContactInformation = [
        this.viewModel.FirstName + " " + this.viewModel.LastName,
        this.viewModel.Email,
        this.viewModel.Phone
    ];

    this.onDoneClicked = function () {
        // go back to the home page
        // webPortal.Journey.start(Microsoft.WebPortal.Feature.Home);        

        // Prepare the order
        self.raiseOrder();
    };

    this.raiseOrder = function (customerNotification, registeredCustomer) {
        /// <summary>
        /// Called when the customer has been created and hence order can be placed. 
        /// </summary>

        // order notification.        
        var orderNotification = new Microsoft.WebPortal.Services.Notification(Microsoft.WebPortal.Services.Notification.NotificationType.Progress,
            self.webPortal.Resources.Strings.Plugins.CustomerRegistrationPage.PreparingOrderAndRedirectingMessage);
        self.webPortal.Services.Notifications.add(orderNotification);

        new Microsoft.WebPortal.Utilities.RetryableServerCall(this.webPortal.Helpers.ajaxCall("api/Order/NewCustomerPrepareOrder", Microsoft.WebPortal.HttpMethod.Post, self.viewModel.orderToPlace, Microsoft.WebPortal.ContentType.Json, 120000), "RegisterCustomerOrder", []).execute()
            // Success of Create CustomerOrder API Call. 
            .done(function (result) {
                orderNotification.dismiss();
                // we need to now redirect to paypal based on the response from the API.             
                window.location = result;
            })
            // Failure in Create CustomerOrder API call. 
            .fail(function (result, status, error) {
                // on failure check if customerid is returned (or check using errCode). if returned then do something to set the ClientCustomerId
                orderNotification.type(Microsoft.WebPortal.Services.Notification.NotificationType.Error);
                orderNotification.buttons([
                    // no need for retry button. user should be able to hit submit.
                    Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.OK, self.webPortal.Resources.Strings.OK, function () {
                        orderNotification.dismiss();
                    })
                ]);

                var errorPayload = JSON.parse(result.responseText);

                if (errorPayload) {
                    switch (errorPayload.ErrorCode) {
                        case Microsoft.WebPortal.ErrorCode.InvalidInput:
                            orderNotification.message(self.webPortal.Resources.Strings.Plugins.CustomerRegistrationPage.InvalidInputErrorPrefix + errorPayload.Details.ErrorMessage);
                            break;
                        case Microsoft.WebPortal.ErrorCode.DownstreamServiceError:
                            orderNotification.message(self.webPortal.Resources.Strings.Plugins.CustomerRegistrationPage.DownstreamErrorPrefix + errorPayload.Details.ErrorMessage);
                            break;
                        case Microsoft.WebPortal.ErrorCode.PaymentGatewayPaymentError:
                        case Microsoft.WebPortal.ErrorCode.PaymentGatewayIdentityFailureDuringPayment:
                        case Microsoft.WebPortal.ErrorCode.PaymentGatewayFailure:
                            orderNotification.message(errorPayload.Details.ErrorMessage);
                            break;
                        default:
                            orderNotification.message(self.webPortal.Resources.Strings.Plugins.CustomerRegistrationPage.OrderRegistrationFailureMessage);
                            break;
                    }
                } else {
                    orderNotification.message(self.webPortal.Resources.Strings.Plugins.CustomerRegistrationPage.OrderRegistrationFailureMessage);
                }

            })
            .always(function () {
                self.isPosting = false;
            });
    };
};

// inherit BasePresenter
$WebPortal.Helpers.inherit(Microsoft.WebPortal.RegistrationConfirmationPresenter, Microsoft.WebPortal.Core.TemplatePresenter);

Microsoft.WebPortal.RegistrationConfirmationPresenter.prototype.onRender = function () {
    /// <summary>
    /// Called when the presenter is about to be rendered.
    /// </summary>

    ko.applyBindings(this, $("#RegistrationConfirmationContainer")[0]);
};

//@ sourceURL=RegistrationConfirmationPresenter.js
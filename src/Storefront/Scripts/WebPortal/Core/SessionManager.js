/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Core.SessionManager = function (webPortal) {
    /// <summary>
    /// Stores session information.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>

    this.webPortal = webPortal;

    // Shell, please let us know when you have finished initializing
    this.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.PortalInitializing, this.initialize, this);

    this.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.FeatureDeactivated, this.onFeatureDeactivated, this);

    // a hashtable that caches the HTML template for each feature
    this.featureTemplates = {};    
};

Microsoft.WebPortal.Core.SessionManager.prototype.initialize = function (eventId, context, broadcaster) {
    /// <summary>
    /// Called when the portal is initializing.
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="context"></param>
    /// <param name="broadcaster"></param>

    // prefetch Microsoft offers
    this.fetchMicrosoftOffers($.Deferred());

    // prefetch Portal offers
    this.fetchPortalOffers($.Deferred());

    // assign feature presenters
    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.Home, Microsoft.WebPortal.HomePagePresenter);
    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.CustomerRegistration, Microsoft.WebPortal.CustomerRegistrationPresenter);
    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.RegistrationConfirmation, Microsoft.WebPortal.RegistrationConfirmationPresenter);

    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.Subscriptions, Microsoft.WebPortal.SubscriptionsPresenter);
    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.AddSubscriptions, Microsoft.WebPortal.AddSubscriptionsPresenter);
    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.UpdateSubscriptions, Microsoft.WebPortal.UpdateSubscriptionsPresenter);
    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.ProcessOrder, Microsoft.WebPortal.ProcessOrderPresenter);

    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.CustomerAccount, Microsoft.WebPortal.CustomerAccountPresenter);
    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.UpdateContactInformation, Microsoft.WebPortal.UpdateContactInformationPresenter);
    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.UpdateCompanyInformation, Microsoft.WebPortal.UpdateCompanyInformationPresenter);

    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.AdminConsole, Microsoft.WebPortal.AdminConsolePresenter);
    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.AddOrUpdateOffers, Microsoft.WebPortal.AddOrUpdateOfferPresenter);
    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.OfferList, Microsoft.WebPortal.OfferListPresenter);
    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.BrandingSetup, Microsoft.WebPortal.BrandingSetupPresenter);
    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.PaymentSetup, Microsoft.WebPortal.PaymentSetupPresenter);
    this.webPortal.registerFeaturePresenter(Microsoft.WebPortal.Feature.CustomerManagementSetup, Microsoft.WebPortal.CustomerManagementSetupPresenter);
}

Microsoft.WebPortal.Core.SessionManager.prototype.onFeatureDeactivated = function () {
    /// <summary>
    /// Called whenever a feature is deactivated.
    /// </summary>

    // clear out the actions bar
    this.webPortal.Services.Actions.clear();
}

Microsoft.WebPortal.Core.SessionManager.prototype.fetchMicrosoftOffers = function (resolver) {
    /// <summary>
    /// Retrieves and stores Microsoft offers in memory.
    /// </summary>
    /// <param name="resolver">A JQuery deferred object which will be notified with the offers once they
    /// are available or get a rejection if there was a failure retrieving them.</param>

    if (this.MicrosoftOffers) {
        resolver.resolve(this.MicrosoftOffers);
        return;
    }

    var getMicrosoftOffersServerCall =
        new Microsoft.WebPortal.Utilities.RetryableServerCall(this.webPortal.Helpers.ajaxCall("api/AdminConsole/MicrosoftOffers", Microsoft.WebPortal.HttpMethod.Get))

    var self = this;

    getMicrosoftOffersServerCall.execute().done(function (microsoftOffers) {
        self.MicrosoftOffers = microsoftOffers;        
        self.webPortal.Diagnostics.information("Acquired Microsoft offers");
        resolver.resolve(self.MicrosoftOffers);
    }).fail(function (result, status, error) {
        self.MicrosoftOffers = null;
        self.webPortal.Diagnostics.error("Failed to acquired Microsoft offers: " + error);
        resolver.reject();
    });   
}

Microsoft.WebPortal.Core.SessionManager.prototype.fetchPortalOffers = function (resolver) {
    /// <summary>
    /// Retrieves and stores Portal offers in memory.
    /// </summary>
    /// <param name="resolver">A JQuery deferred object which will be notified with the offers once they
    /// are available or get a rejection if there was a failure retrieving them.</param>

    var getPortalOffersServerCall =
        new Microsoft.WebPortal.Utilities.RetryableServerCall(this.webPortal.Helpers.ajaxCall("api/partnerOffers", Microsoft.WebPortal.HttpMethod.Get))

    var self = this;

    getPortalOffersServerCall.execute()
    .done(function (portalOffers) {
        self.PortalOffers = portalOffers.Offers;

        //setup an Id based mapped observable array for Portal offer items.
        self.IdMappedPortalOffers = ko.utils.arrayMap(self.PortalOffers, function (offerItem) {
            return {
                OriginalOffer: offerItem,
                Id: offerItem.Id
            }
        });

        self.webPortal.Diagnostics.information("Acquired Portal offers");
        resolver.resolve(self.PortalOffers);
    })
    .fail(function (result, status, error) {        
        self.IdMappedPortalOffers = null;
        self.webPortal.Diagnostics.error("Failed to acquired Portal offers: " + error);
        resolver.reject();
    });
}

Microsoft.WebPortal.Core.SessionManager.prototype.fetchCustomerSubscriptionDetails = function (resolver) {
    /// <summary>
    /// Retrieves the customer account details for use across the app.
    /// </summary>
    /// <param name="resolver">A JQuery deferred object which will be notified with the customer details once they
    /// are available or get a rejection if there was a failure retrieving them.</param>

    var getCustomerServerCall =
        new Microsoft.WebPortal.Utilities.RetryableServerCall(this.webPortal.Helpers.ajaxCall("api/CustomerAccounts/Subscriptions", Microsoft.WebPortal.HttpMethod.Get))

    var self = this;

    getCustomerServerCall.execute()
    .done(function (customerInfo) {        
        self.webPortal.Diagnostics.information("Acquired Customer Information.");
        resolver.resolve(customerInfo);
    })
    .fail(function (result, status, error) {        
        self.webPortal.Diagnostics.error("Failed to acquire Customer Information: " + error);
        resolver.reject();
    });
}

Microsoft.WebPortal.Core.SessionManager.prototype.fetchBrandingConfiguration = function (resolver) {
    /// <summary>
    /// Retrieves and the partner's branding configuration.
    /// </summary>
    /// <param name="resolver">A JQuery deferred object which will be notified with the branding configuration once it is
    /// available, or get a rejection if there was a failure retrieving it.</param>

    var getBrandingConfigurationServerCall =
        new Microsoft.WebPortal.Utilities.RetryableServerCall(this.webPortal.Helpers.ajaxCall("api/AdminConsole/Branding", Microsoft.WebPortal.HttpMethod.Get))

    var self = this;

    getBrandingConfigurationServerCall.execute().done(function (brandingConfiguration) {
        self.webPortal.Diagnostics.information("Acquired branding configuration");
        resolver.resolve(brandingConfiguration);
    }).fail(function (result, status, error) {
        self.webPortal.Diagnostics.error("Failed to acquired branding configuration: " + error);
        resolver.reject();
    });
}

Microsoft.WebPortal.Core.SessionManager.prototype.fetchPaymentConfiguration = function (resolver) {
    /// <summary>
    /// Retrieves and the partner's payment configuration.
    /// </summary>
    /// <param name="resolver">A JQuery deferred object which will be notified with the payment configuration once it is
    /// available, or get a rejection if there was a failure retrieving it.</param>

    var getPaymentConfigurationServerCall =
        new Microsoft.WebPortal.Utilities.RetryableServerCall(this.webPortal.Helpers.ajaxCall("api/AdminConsole/Payment", Microsoft.WebPortal.HttpMethod.Get))

    var self = this;

    getPaymentConfigurationServerCall.execute().done(function (paymentConfiguration) {
        self.webPortal.Diagnostics.information("Acquired payment configuration");
        resolver.resolve(paymentConfiguration);
    }).fail(function (result, status, error) {
        self.webPortal.Diagnostics.error("Failed to acquired payment configuration: " + error);
        resolver.reject();
    });
}

//@ sourceURL=SessionManager.js

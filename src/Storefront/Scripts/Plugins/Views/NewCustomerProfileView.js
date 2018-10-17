Microsoft.WebPortal.Views.NewCustomerProfileView = function (webPortal, elementSelector, isShown, animation) {
    /// <summary>
    /// A view that render input fields for a new customer company and contact information.
    /// </summary>
    /// <param name="webPortal">The web portal instance</param>
    /// <param name="elementSelector">The JQuery selector for the HTML element this view will own.</param>
    /// <param name="isShown">The initial show state. Optional. Default is false.</param>
    /// <param name="animation">Optional animation to use for showing and hiding the view.</param>

    this.base.constructor.call(this, webPortal, elementSelector, isShown, null, animation);
    this.template = "newCustomerProfile-template";

    this.viewModel = {
        PartnerCountry: this.webPortal.Resources.Strings.PartnerCountry,
        Country: ko.observable(this.webPortal.Resources.Strings.PartnerCountry),
        CompanyName: ko.observable(""),
        AddressLine1: ko.observable(""),
        AddressLine2: ko.observable(""),
        City: ko.observable(""),
        State: ko.observable(""),
        ZipCode: ko.observable(""),
        Email: ko.observable(""),
        Password: ko.observable(""),
        PasswordConfirmation: ko.observable(""),
        FirstName: ko.observable(""),
        LastName: ko.observable(""),
        Phone: ko.observable(""),
        DomainPrefix: ko.observable(""),
        CustomerMicrosoftID: ko.observable("") // manage the client id field value. 
    };

    this.viewModel.Countries = ko.computed(function () {
        switch (this.viewModel.PartnerCountry) {
            case "AU":
                return [{ Id: "AU", Name: this.webPortal.Resources.Strings.Countries.AU }];
            case "CA":
                return [{ Id: "CA", Name: this.webPortal.Resources.Strings.Countries.CA }];
            case "JP":
                return [{ Id: "JP", Name: this.webPortal.Resources.Strings.Countries.JP }];
            case "NZ":
                return [{ Id: "NZ", Name: this.webPortal.Resources.Strings.Countries.NZ }];
            case "US":
                return [{ Id: "US", Name: this.webPortal.Resources.Strings.Countries.US }];
            case "IN":
                return [{ Id: "IN", Name: this.webPortal.Resources.Strings.Countries.IN }];
            case "AT":
            case "BE":
            case "BG":
            case "EE":
            case "HR":
            case "CY":
            case "CZ":
            case "DK":
            case "FI":
            case "FR":
            case "DE":
            case "GR":
            case "HU":
            case "IS":
            case "IE":
            case "IT":
            case "LV":
            case "LI":
            case "LT":
            case "LU":
            case "MT":
            case "MC":
            case "NL":
            case "NO":
            case "PO":
            case "PT":
            case "RO":
            case "SK":
            case "SL":
            case "ES":
            case "SE":
            case "CH":
            case "GB":
                return [
                    { Id: "AT", Name: this.webPortal.Resources.Strings.Countries.AT },
                    { Id: "BE", Name: this.webPortal.Resources.Strings.Countries.BE },
                    { Id: "BG", Name: this.webPortal.Resources.Strings.Countries.BG },
                    { Id: "EE", Name: this.webPortal.Resources.Strings.Countries.EE },
                    { Id: "HR", Name: this.webPortal.Resources.Strings.Countries.HR },
                    { Id: "CY", Name: this.webPortal.Resources.Strings.Countries.CY },
                    { Id: "CZ", Name: this.webPortal.Resources.Strings.Countries.CZ },
                    { Id: "DK", Name: this.webPortal.Resources.Strings.Countries.DK },
                    { Id: "FI", Name: this.webPortal.Resources.Strings.Countries.FI },
                    { Id: "FR", Name: this.webPortal.Resources.Strings.Countries.FR },
                    { Id: "DE", Name: this.webPortal.Resources.Strings.Countries.DE },
                    { Id: "GR", Name: this.webPortal.Resources.Strings.Countries.GR },
                    { Id: "HU", Name: this.webPortal.Resources.Strings.Countries.HU },
                    { Id: "IS", Name: this.webPortal.Resources.Strings.Countries.IS },
                    { Id: "IE", Name: this.webPortal.Resources.Strings.Countries.IE },
                    { Id: "IT", Name: this.webPortal.Resources.Strings.Countries.IT },
                    { Id: "LV", Name: this.webPortal.Resources.Strings.Countries.LV },
                    { Id: "LI", Name: this.webPortal.Resources.Strings.Countries.LI },
                    { Id: "LT", Name: this.webPortal.Resources.Strings.Countries.LT },
                    { Id: "LU", Name: this.webPortal.Resources.Strings.Countries.LU },
                    { Id: "MT", Name: this.webPortal.Resources.Strings.Countries.MT },
                    { Id: "MC", Name: this.webPortal.Resources.Strings.Countries.MC },
                    { Id: "NL", Name: this.webPortal.Resources.Strings.Countries.NL },
                    { Id: "NO", Name: this.webPortal.Resources.Strings.Countries.NO },
                    { Id: "PO", Name: this.webPortal.Resources.Strings.Countries.PO },
                    { Id: "PT", Name: this.webPortal.Resources.Strings.Countries.PT },
                    { Id: "RO", Name: this.webPortal.Resources.Strings.Countries.RO },
                    { Id: "SK", Name: this.webPortal.Resources.Strings.Countries.SK },
                    { Id: "SL", Name: this.webPortal.Resources.Strings.Countries.SL },
                    { Id: "ES", Name: this.webPortal.Resources.Strings.Countries.ES },
                    { Id: "SE", Name: this.webPortal.Resources.Strings.Countries.SE },
                    { Id: "CH", Name: this.webPortal.Resources.Strings.Countries.CH },
                    { Id: "GB", Name: this.webPortal.Resources.Strings.Countries.GB }
                ];

            default:
                return [];
        }
    }, this);

    this.viewModel.UserName = ko.computed(function () {
        return "admin@" + this.viewModel.DomainPrefix() + ".onmicrosoft.com";
    }, this);
};

// extend the base view
$WebPortal.Helpers.inherit(Microsoft.WebPortal.Views.NewCustomerProfileView, Microsoft.WebPortal.Core.View);

Microsoft.WebPortal.Views.NewCustomerProfileView.prototype.onRender = function () {
    /// <summary>
    /// Called when the view is rendered.
    /// </summary>

    $(this.elementSelector).attr("data-bind", "template: { name: '" + this.template + "'}");
    ko.applyBindings(this, $(this.elementSelector)[0]);
};

Microsoft.WebPortal.Views.NewCustomerProfileView.prototype.onShowing = function (isShowing) {
};

Microsoft.WebPortal.Views.NewCustomerProfileView.prototype.onShown = function (isShowing) {
};

Microsoft.WebPortal.Views.NewCustomerProfileView.prototype.onDestroy = function () {
    /// <summary>
    /// Called when the journey trail is about to be destroyed.
    /// </summary>

    if ($(this.elementSelector)[0]) {
        // if the element is there, clear its bindings and clean up its content
        ko.cleanNode($(this.elementSelector)[0]);
        $(this.elementSelector).empty();
    }
};

//@ sourceURL=NewCustomerProfileView.js
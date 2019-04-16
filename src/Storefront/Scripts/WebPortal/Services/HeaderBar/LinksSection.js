Microsoft.WebPortal.Services.LinksSection = function (webPortal) {
    /// <summary>
    /// Renders the links the header bar.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>

    this.base.constructor.call(this, webPortal, "LinksSection", "linksHeaderBarSection-template");
    this.style("padding:0; width: 1px;");

    this.isLoggedIn = ko.observable(false);

    this.onLoginClicked = function () {
        webPortal.Services.Login.login();
    };

    var self = this;

    this.onContactUsClicked = function () {
        if (window.contactUs && window.contactSales) {
            // contact us and sales have been configured, show them in a dialog, otherwise do nothing until they are configured
            var okButton = Microsoft.WebPortal.Services.Button.create(Microsoft.WebPortal.Services.Button.StandardButtons.OK, 1, function () {
                self.webPortal.Services.Dialog.hide();
            });

            self.webPortal.Services.Dialog.show("contactUs-template", {
                contactUs: {
                    emailLink: "mailto:" + contactUs.email,
                    email: contactUs.email,
                    phone: contactUs.phone,
                    phoneLink: "tel:" + contactUs.phone
                },
                contactSales: {
                    emailLink: "mailto:" + contactSales.email,
                    email: contactSales.email,
                    phone: contactSales.phone,
                    phoneLink: "tel:" + contactSales.phone
                }
            }, [okButton]);
        }
    };

    webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.UserLoggedIn, function (eventId, isLoggedIn, broadcaster) {
        this.isLoggedIn(isLoggedIn);

        if (isLoggedIn) {
            webPortal.Services.HeaderBar.addSection(new Microsoft.WebPortal.Services.UserSection(webPortal));

            // TODO :: Loc review. 
            webPortal.Services.UserMenu.add(new Microsoft.WebPortal.Services.Action("SignOut", webPortal.Resources.Strings.SignOut, function () {
                // Implement sign out
                webPortal.Services.Login.logout();
            }, null, webPortal.Resources.Strings.SignOutToolTipCaption));
        } else {
            webPortal.Services.HeaderBar.removeSection("UserInfoSection");
        }
    }, this);
};

$WebPortal.Helpers.inherit(Microsoft.WebPortal.Services.LinksSection, Microsoft.WebPortal.Services.HeaderBarSection);

//@ sourceURL=LinksSection.js
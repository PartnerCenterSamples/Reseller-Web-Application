/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Services.UserSection = function (webPortal, loggedInUser) {
    /// <summary>
    /// Renders the user information section in the header bar. Displays the user name and avatar icon and manages the user menu.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>

    this.base.constructor.call(this, webPortal, "UserInfoSection", "userSection-template");

    this.loggedInUser = loggedInUser;

    // create and register the actions service
    this.userMenuSevice = ko.observable(null);
    //this.webPortal.registerPortalService(this.userMenuSevice);
}

$WebPortal.Helpers.inherit(Microsoft.WebPortal.Services.UserSection, Microsoft.WebPortal.Services.HeaderBarSection);

Microsoft.WebPortal.Services.UserSection.prototype.initialize = function () {
    /// <summary>
    /// This function is called to initialize the header bar section.
    /// </summary>

    var service = new Microsoft.WebPortal.Services.UserMenuService(this.webPortal, "#UserInfoSection");
    this.userMenuSevice(service);
    this.webPortal.registerPortalService(service);
}

Microsoft.WebPortal.Services.UserSection.prototype.destroy = function () {
    /// <summary>
    /// This function is called to destroy the header bar section.
    /// </summary>

    this.webPortal.deregisterPortalService(this.userMenuSevice());
}


//@ sourceURL=UserSection.js
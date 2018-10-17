/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Services.Login = function (webPortal) {
    /// <summary>
    /// The Login service. Maintains the login state and broadcasts user login events.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>

    this.base.constructor.call(this, webPortal, "Login");

    this.isLoggedIn = ko.observable(false);
    this.isInErrorState = ko.observable(false);
    this.errorMessage = ko.observable();
    this.userName = ko.observable("");
    this.password = ko.observable("");
    this.rememberMe = ko.observable(false);
}

$WebPortal.Helpers.inherit(Microsoft.WebPortal.Services.Login, Microsoft.WebPortal.Core.PortalService);

Microsoft.WebPortal.Services.Login.prototype._runService = function () {
    /// <summary>
    /// Runs the Login service.
    /// </summary>
  
    if (isAuthenticated) {
        this.isLoggedIn(true);
        this.userName(userName);
        this.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.UserLoggedIn, true);
    }
}

Microsoft.WebPortal.Services.Login.prototype._stopService = function () {
    /// <summary>
    /// Stops the Login service.
    /// </summary>   
}

Microsoft.WebPortal.Services.Login.prototype.login = function () {
    /// <summary>
    /// Displays the login dialog to the user.
    /// </summary>

    window.location = "Account/Login";
}

Microsoft.WebPortal.Services.Login.prototype.logout = function () {
    /// <summary>
    /// Logs the user out from the application.
    /// </summary>

    window.location = "Account/SignOut";
}


//@ sourceURL=Login.js
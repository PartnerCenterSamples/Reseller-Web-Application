/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Services.UserMenuService = function (webPortal, elementSelector) {
    /// <summary>
    /// Implements the user menu service which manages the user information (avatar and user name) and displays a menu of user related actions.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="elementSelector">The JQuery selector for the HTML element that will host the actions.</param>

    this.base.constructor.call(this, webPortal, "UserMenu", elementSelector);
    this.actionsManager = new Microsoft.WebPortal.Services.ActionsManager(this.webPortal, elementSelector, 1);
    this.userMenu = new Microsoft.WebPortal.Services.Action("UserInfo", this.webPortal.Services.Login.userName());
    this.destroyed = true;

    // user menu will be togglable
    Microsoft.WebPortal.Utilities.Toggler.injectToggling(this, this.show, this.hide, false);

    this.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.PortalInitialized, function () {
        this.destroyed = false;
        this.actionsManager.add(this.userMenu);
    }, this);
}

$WebPortal.Helpers.inherit(Microsoft.WebPortal.Services.UserMenuService, Microsoft.WebPortal.Core.PortalService);

Microsoft.WebPortal.Services.UserMenuService.prototype._runService = function () {
    /// <summary>
    /// Runs the user menu service.
    /// </summary>

    if (this.destroyed) {
        this.actionsManager.add(this.userMenu);
    }

    this.actionsManager.render();
}

Microsoft.WebPortal.Services.UserMenuService.prototype._stopService = function () {
    /// <summary>
    /// Stops the user menu service.
    /// </summary>

    this.actionsManager.destroy();
    this.destroyed = true;
}

Microsoft.WebPortal.Services.UserMenuService.prototype.add = function (action) {
    /// <summary>
    /// Adds an action to the bar. See the Microsoft.WebPortal.Services.Action class to learn how to create and configure actions.
    /// </summary>
    /// <param name="action">The action to add. Must have unique ID. Otherwise, the existing action will be kept./param>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The " + this.name + " service is not running.");
        return;
    }

    this.userMenu.children.push(action);
}

Microsoft.WebPortal.Services.UserMenuService.prototype.remove = function (action) {
    /// <summary>
    ///  Removes an action from the user menu.
    /// </summary>
    /// <param name="action">The action to remove.</param>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The " + this.name + " service is not running.");
        return;
    }

    for (var i in this.userMenu.children()) {
        if (this.userMenu.children()[i] == action) {
            this.userMenu.children.remove(action);
            break;
        }
    }
}

Microsoft.WebPortal.Services.UserMenuService.prototype.removeById = function (actionId) {
    /// <summary>
    /// Removes an action from the user menu using its ID.
    /// </summary>
    /// <param name="actionId">The ID of the action to remove.</param>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The " + this.name + " service is not running.");
        return;
    }

    for (var i in this.userMenu.children()) {
        if (this.userMenu.children()[i].id() == actionId) {
            this.userMenu.children.remove(this.userMenu.children()[i]);
            break;
        }
    }
}

Microsoft.WebPortal.Services.UserMenuService.prototype.clear = function () {
    /// <summary>
    /// Removes all registered actions from the user menu.
    /// </summary>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The " + this.name + " service is not running.");
        return;
    }

    this.userMenu.children([]);
}

Microsoft.WebPortal.Services.UserMenuService.prototype.show = function (showProgress) {
    /// <summary>
    /// Shows the user menu.
    /// </summary>
    /// <param name="showProgress">A JQuery deferred object used to signal show completion.</param>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The user menu service is not running.");
        showProgress.reject();
    } else {
        this.userMenu.onClick(this.userMenu);
        showProgress.resolve();
    }
}

Microsoft.WebPortal.Services.UserMenuService.prototype.hide = function (hideProgress) {
    /// <summary>
    /// Hides the user menu.
    /// </summary>
    /// <param name="hideProgress">A JQuery deferred object used to signal hide completion.</param>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The user menu service is not running.");
        hideProgress.reject();
    } else {
        this.userMenu.onClick(this.userMenu);
        hideProgress.resolve();
    }
}

//@ sourceURL=UserMenuService.js
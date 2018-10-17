/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Services.ActionsService = function (webPortal, serviceName, elementSelector) {
    /// <summary>
    /// Implements an actions service which supports adding and removing simple and compound actions.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="serviceName">The service name.</param>
    /// <param name="elementSelector">The JQuery selector for the HTML element that will host the actions.</param>

    this.base.constructor.call(this, webPortal, serviceName);
    this.actionsManager = new Microsoft.WebPortal.Services.ActionsManager(this.webPortal, elementSelector);
}

$WebPortal.Helpers.inherit(Microsoft.WebPortal.Services.ActionsService, Microsoft.WebPortal.Core.PortalService);

Microsoft.WebPortal.Services.ActionsService.prototype._runService = function () {
    /// <summary>
    /// Runs the actions bar service.
    /// </summary>

    this.actionsManager.render();
}

Microsoft.WebPortal.Services.ActionsService.prototype._stopService = function () {
    /// <summary>
    /// Stops the action bar service.
    /// </summary>

    this.actionsManager.destroy();
}

Microsoft.WebPortal.Services.ActionsService.prototype.add = function (action) {
    /// <summary>
    /// Adds an action to the bar. See the Microsoft.WebPortal.Services.Action class to learn how to create and configure actions.
    /// </summary>
    /// <param name="action">The action to add. Must have unique ID. Otherwise, the existing action will be kept./param>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The " + this.name + " service is not running.");
        return;
    }

    this.actionsManager.add(action);
}

Microsoft.WebPortal.Services.ActionsService.prototype.addRange = function (actions) {
    /// <summary>
    /// Adds a list of actions to the action bar.
    /// </summary>
    /// <param name="actions">An array of Microsoft.WebPortal.Services.Action objects.</param>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The " + this.name + " service is not running.");
        return;
    }

    this.actionsManager.addRange(actions);
}

Microsoft.WebPortal.Services.ActionsService.prototype.remove = function (action) {
    /// <summary>
    ///  Removes an action from the action bar.
    /// </summary>
    /// <param name="action">The action to remove.</param>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The " + this.name + " service is not running.");
        return;
    }

    this.actionsManager.remove(action);
}

Microsoft.WebPortal.Services.ActionsService.prototype.removeById = function (actionId) {
    /// <summary>
    /// Removes an action from the action bar using its ID.
    /// </summary>
    /// <param name="actionId">The ID of the action to remove.</param>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The " + this.name + " service is not running.");
        return;
    }

    this.actionsManager.removeById(actionId);
}

Microsoft.WebPortal.Services.ActionsService.prototype.clear = function () {
    /// <summary>
    /// Removes all registered actions from the action bar.
    /// </summary>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The " + this.name + " service is not running.");
        return;
    }

    this.actionsManager.clear();
}

//@ sourceURL=ActionsService.js
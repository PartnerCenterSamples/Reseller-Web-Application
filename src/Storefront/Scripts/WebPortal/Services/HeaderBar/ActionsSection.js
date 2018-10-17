Microsoft.WebPortal.Services.ActionsSection = function (webPortal) {
    /// <summary>
    /// Renders the actions the user can invoke on the current feature.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>

    this.base.constructor.call(this, webPortal, webPortal.Settings.Ids.ActionsSection, "actionsHeaderBarSection-template");

    // create and register the actions service
    this.actionBarSevice = new Microsoft.WebPortal.Services.ActionsService(this.webPortal, "Actions", "#" + webPortal.Settings.Ids.ActionsSection);
    this.webPortal.registerPortalService(this.actionBarSevice);
};

$WebPortal.Helpers.inherit(Microsoft.WebPortal.Services.ActionsSection, Microsoft.WebPortal.Services.HeaderBarSection);

Microsoft.WebPortal.Services.ActionsSection.prototype.initialize = function () {
    /// <summary>
    /// This function is called to initialize the header bar section.
    /// </summary>

    if (!this.webPortal.Services.Actions) {
        this.webPortal.registerPortalService(this.actionBarSevice);
    }
};

Microsoft.WebPortal.Services.ActionsSection.prototype.destroy = function () {
    /// <summary>
    /// This function is called to destroy the header bar section.
    /// </summary>

    this.webPortal.deregisterPortalService(this.actionBarSevice);
};

//@ sourceURL=ActionsSection.js
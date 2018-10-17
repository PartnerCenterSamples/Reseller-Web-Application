/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Services.NotificationsSection = function (webPortal) {
    /// <summary>
    /// Renders the notifications icon and number if there are pending notifications.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>

    this.base.constructor.call(this, webPortal, webPortal.Settings.Ids.NotificationsSection, "notificationsHeaderBarSection-template");

    this.onNotificationsSectionClicked = function () {
        // toggle between showing and hiding the notification panel
        if (this.webPortal.Services.Notifications && this.webPortal.Services.Notifications.isRunning) {
            this.webPortal.Services.Notifications.toggle();
        }
    }

    this.notificationsCount = ko.computed(function () {
        if (this.webPortal.Services.Notifications && this.webPortal.Services.Notifications.isRunning) {
            return this.webPortal.Services.Notifications.notifications().length;
        } else {
            return 0;
        }
    }, this);

    // display the notifications flag only if there are active notifications
    this.notificationsSectionVisible = ko.computed(function () {
        if (this.webPortal.Services.Notifications && this.webPortal.Services.Notifications.isRunning) {
            return this.webPortal.Services.Notifications.notifications().length > 0 ? "table" : "none";
        } else {
            return false;
        }
    }, this);

    this.onHover = function (elementId, model) {
        $(elementId).css("background-color", webPortal.activeTile().AlternateColor);
    }

    this.onUnhover = function (elementId, model) {
        $(elementId).css("background-color", "");
    }
}

$WebPortal.Helpers.inherit(Microsoft.WebPortal.Services.NotificationsSection, Microsoft.WebPortal.Services.HeaderBarSection);

//@ sourceURL=NotificationsSection.js
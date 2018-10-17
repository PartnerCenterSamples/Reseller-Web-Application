/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Services.NotificationsManager = function (webPortal, animation, notificationsPanelTemplate) {
    /// <summary>
    /// Manages notifications and their rendering. Provides methods to show or hide the notification panel and to add or remove
    /// notifications to/from it.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="animation">The animation used to show and hide the panel. Use a derivative of Microsoft.WebPortal.Utilities.BaseAnimation. Optional.</param>
    /// <param name="notificationsPanelTemplate">The HTML template used to render the notifications panel. Optional. Default will be used if not provided.</param>

    this.base.constructor.call(this, webPortal, "Notifications");

    this.notificationPanelElementSelector = "#NotificationsPanel";
    this.template = notificationsPanelTemplate || "notificationPanel-template";
    this.animation = animation || new Microsoft.WebPortal.Utilities.Animation(Microsoft.WebPortal.Effects.SlideDown, 350);
    this.notifications = ko.observableArray([]);

    // if we have more than one notification, display a "clear all" button
    this.displayClearAllNotifications = ko.computed(function () {
        return this.notifications().length > 1;
    }, this);

    // returns a summary message based on the number of active notifications
    this.notificationsSummary = ko.computed(function () {
        return this.notifications().length + this.webPortal.Resources.Strings.NotificationsSummary;
    }, this);

    // notifications panel will be togglable
    Microsoft.WebPortal.Utilities.Toggler.injectToggling(this, this.show, this.hide, false);

    // we will serialize notification manipulation functions since they use animations, we want to make sure the whole async operation is complete before
    // the next requests are executed
    this.serializer = new Microsoft.WebPortal.Utilities.AsyncOperationSerializer();
}

$WebPortal.Helpers.inherit(Microsoft.WebPortal.Services.NotificationsManager, Microsoft.WebPortal.Core.PortalService);

Microsoft.WebPortal.Services.NotificationsManager.prototype._runService = function () {
    /// <summary>
    /// Runs the notifications service.
    /// </summary>

    // bind to the notification panel
    ko.applyBindings(this, $(this.notificationPanelElementSelector)[0]);

    this.onHideMenus = function (eventId, context, broadcaster) {
        // hide the notifications panel in response to the hide menus event
        if (broadcaster != this && this.isShown()) {
            this.hide();
        }
    }

    // the notification panel will hide upon a hide menus event
    this.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.HideMenus, this.onHideMenus, this);
}

Microsoft.WebPortal.Services.NotificationsManager.prototype._stopService = function () {
    /// <summary>
    /// Stops the notifications service.
    /// </summary>

    // clear notifications
    this.clear();

    this.serializer.queue(this, function (taskProgress) {
        // clean up HTML after clearing is done
        ko.cleanNode($(this.notificationPanelElementSelector)[0]);
        $(this.notificationPanelElementSelector).empty();

        this.webPortal.EventSystem.unsubscribe(Microsoft.WebPortal.Event.HideMenus, this.onHideMenus, this);

        taskProgress.resolve();
    });
}

Microsoft.WebPortal.Services.NotificationsManager.prototype.show = function (showProgress) {
    /// <summary>
    /// Shows the notifications panel.
    /// </summary>
    /// <param name="showProgress">A JQuery deferred object used to signal show completion.</param>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The notifications service is not running.");
        showProgress.reject();
    } else if (!this.isShown()) {
        // hide any shown menus
        this.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.HideMenus, null, this);

        this.animation.show(this.notificationPanelElementSelector).always(function () {
            showProgress.resolve();
        });
    } else {
        showProgress.resolve();
    }
}

Microsoft.WebPortal.Services.NotificationsManager.prototype.hide = function (hideProgress) {
    /// <summary>
    /// Hides the notifications panel.
    /// </summary>
    /// <param name="hideProgress">A JQuery deferred object used to signal hide completion.</param>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The notifications service is not running.");
        hideProgress.reject();
    } else if (this.isShown()) {
        this.animation.hide(this.notificationPanelElementSelector).always(function () {
            hideProgress.resolve();
        });
    } else {
        hideProgress.resolve();
    }
}

Microsoft.WebPortal.Services.NotificationsManager.prototype.add = function (notification) {
    /// <summary>
    /// Adds a notification to the notifications panel. This will cause the notifications panel to show.
    /// </summary>
    /// <param name="notification">An instance of Microsoft.WebPortal.Services.Notification.</param>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The notifications service is not running.");
        return;
    }

    this.webPortal.Helpers.throwIfNotSet(notification, "notification", "Microsoft.WebPortal.Services.NotificationsManager.add");

    this.serializer.queue(this, function (taskProgress) {
        for (var i in this.notifications()) {
            if (this.notifications()[i] === notification) {
                this.webPortal.Diagnostics.warningLocal("The same notification already exists.");
                taskProgress.reject();
                return;
            }
        }

        notification.id("Notification_" + this.webPortal.Helpers.random());
        this.notifications.push(notification);

        if (this.isShown()) {
            // animate the new notification
            this.animation.show("#" + notification.id()).always(function () {
                taskProgress.resolve();
            });
        } else {
            // set the notification display to shown
            $("#" + notification.id()).css("display", "block");

            // show the whole panel
            this.show().always(function () {
                taskProgress.resolve();
            });
        }
    });
}

Microsoft.WebPortal.Services.NotificationsManager.prototype.remove = function (notification) {
    /// <summary>
    /// Removes a notification from the panel.
    /// </summary>
    /// <param name="notification">An instance of Microsoft.WebPortal.Services.Notification.</param>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The notifications service is not running.");
        return;
    }

    this.webPortal.Helpers.throwIfNotSet(notification, "notification", "Microsoft.WebPortal.Services.NotificationsManager.remove");

    this.serializer.queue(this, function (taskProgress) {
        var self = this;

        if (this.isShown()) {
            if ($("#" + notification.id())[0] === undefined) {
                // the user passed in a notification that does not exist in our array, terminate this task
                taskProgress.resolve();
                return;
            }

            // animate the notification
            this.animation.hide("#" + notification.id()).always(function () {
                self.notifications.remove(notification);

                if (self.notifications().length <= 0) {
                    // hide the panel if there are no more notifications
                    self.hide().always(function () {
                        taskProgress.resolve();
                    });
                } else {
                    taskProgress.resolve();
                }
            });
        } else {
            // no need to animate, just remove the notification
            this.notifications.remove(notification);
            taskProgress.resolve();
        }
    });
    
}

Microsoft.WebPortal.Services.NotificationsManager.prototype.clear = function () {
    /// <summary>
    /// Removes all the notifications and hides the panel.
    /// </summary>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The notifications service is not running.");
        return;
    }

    this.serializer.queue(this, function (taskProgress) {
        var self = this;

        this.hide().always(function () {
            self.notifications.removeAll();
            self.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.NotificationsCleared);
            taskProgress.resolve();
        });
    });
}

Microsoft.WebPortal.Services.NotificationsManager.prototype.onClearAllNotificationsClicked = function () {
    /// <summary>
    /// Called when the user clicks on the clear all notifications button.
    /// </summary>

    this.clear();
}


//@ sourceURL=Notifications.js
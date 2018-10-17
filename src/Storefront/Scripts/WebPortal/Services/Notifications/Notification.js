Microsoft.WebPortal.Services.Notification = function (type, message, buttons) {
    /// <summary>
    /// Defines a notification. Use this class to create and configure notifications. Pass a notification type from 
    /// the Microsoft.WebPortal.Services.Notification.NotificationType enumeration, a message and an array of Button objects to attach behavior to the notification.
    /// </summary>
    /// <param name="type">The notification type. Use the Microsoft.WebPortal.Services.Notification.NotificationType enumeration.</param>
    /// <param name="message">The notification message. Mandatory.</param>
    /// <param name="buttons">An optional array of Microsoft.WebPortal.Services.Button objects to attach to the notification.</param>

    $WebPortal.Helpers.throwIfNotSet(type, "type", "Microsoft.WebPortal.Services.Notification.Constructor");
    $WebPortal.Helpers.throwIfNotSet(message, "message", "Microsoft.WebPortal.Services.Notification.Constructor");

    this.id = ko.observable();
    this.type = ko.observable(type);
    this.message = ko.observable(message);
    this.buttons = ko.observableArray(buttons ? buttons : []);

    // listen to the buttons array's changes
    this.buttons.subscribe(function () {
        // set the buttons owner to us!
        for (var index in this.buttons()) {
            this.buttons()[index].owner = this;
        }
    }, this);

    // force an update on the buttons to set their owner to this notification
    this.buttons.valueHasMutated();

    // generate the icon based on the type
    this.icon = ko.computed(function () {
        var icon;

        switch (this.type()) {
            case Microsoft.WebPortal.Services.Notification.NotificationType.Warning:
                icon = $WebPortal.Resources.Images.WarningNotification;
                break;
            case Microsoft.WebPortal.Services.Notification.NotificationType.Error:
                icon = $WebPortal.Resources.Images.ErrorNotification;
                break;
            case Microsoft.WebPortal.Services.Notification.NotificationType.Progress:
                icon = $WebPortal.Resources.Images.ProgressNotification;
                break;
            case Microsoft.WebPortal.Services.Notification.NotificationType.Success:
                icon = $WebPortal.Resources.Images.SuccessNotification;
                break;
            default:
                icon = $WebPortal.Resources.Images.InfoNotification;
                break;
        }

        return icon;
    }, this);

    this.dismiss = function () {
        /// <summary>
        /// Removes the notification from the notification panel.
        /// </summary>

        $WebPortal.Services.Notifications.remove(this);
    };
};

/*
    The notification types supported by the portal.
*/
Microsoft.WebPortal.Services.Notification.NotificationType = {
    Info: 1,
    Warning: 2,
    Error: 3,
    Success: 4,
    Progress: 5
};

//@ sourceURL=Notification.js
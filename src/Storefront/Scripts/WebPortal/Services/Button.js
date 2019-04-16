Microsoft.WebPortal.Services.Button = function (id, displayName, icon, handler) {
    /// <summary>
    /// Represents a button that performs an action on a notification or a dialog wizard. Use this constructor to create
    /// generic buttons. To use the standard set of buttons (YES, NO, OK, CANCEL, RETRY, BACK, NEXT), call the static create method 
    /// instead and pass it your callback function. The call back function will receive the notification and the button as arguments.
    /// </summary>
    /// <param name="id">The button Id. Mandatory.</param>
    /// <param name="displayName">The button text. Either the icon or the text must be passed.</param>
    /// <param name="icon">The button icon. Either the icon or the text must be passed.</param>
    /// <param name="handler">Mandatory. A function to call when the button is clicked. signature is: callbackFunction(button, owner).
    /// The owner is automatically set to the notification or the wizard that owns the button.
    /// </param>

    $WebPortal.Helpers.throwIfNotSet(id, "id", "Microsoft.WebPortal.Services.Button.Constructor");
    $WebPortal.Helpers.throwIfNotSet(handler, "handler", "Microsoft.WebPortal.Services.Button.Constructor");

    if (!displayName && !icon) {
        $WebPortal.Diagnostics.errorLocal("Microsoft.WebPortal.Services.Button.Constructor: displayName and icon can't be both empty.");
        throw new Error("A notification button must must be passed a display name or an icon.");
    }

    this.id = ko.observable(id);
    this.displayName = ko.observable(displayName);
    this.icon = ko.observable(icon);
    this.handler = handler;

    // this will be set by the owner of the button (if any). For instance, a notification will set itself as the owner of its buttons
    this.owner = null;

    this.onClick = function () {
        /// <summary>
        /// Handles the button click.
        /// </summary>

        if (this.handler) {
            // call the associated handler and pass it the the button and the owner of the button
            this.handler(this, this.owner, arguments[2]);
        }
    };
};

Microsoft.WebPortal.Services.Button.create = function (standardButton, id, handler) {
    /// <summary>
    /// Creates a standard button.
    /// </summary>
    /// <param name="standardButton">The standard button.Use Microsoft.WebPortal.Services.Button.StandardButtons.</param>
    /// <param name="id">The button Id. Mandatory.</param>
    /// <param name="handler">A callback function. function signature is: callbackFunction(owner, button). Mandatory.</param>
    /// <returns type="Microsoft.WebPortal.Services.Button">A configured standard button.</returns>

    $WebPortal.Helpers.throwIfNotSet(standardButton, "standardButton", "Microsoft.WebPortal.Services.Button.create");
    return new Microsoft.WebPortal.Services.Button(id, standardButton.name, standardButton.icon, handler);
};

/*
    Standard buttons.
*/
Microsoft.WebPortal.Services.Button.StandardButtons = {
    YES: {
        name: $WebPortal.Resources.Strings.Yes,
        icon: $WebPortal.Resources.Images.Tick
    },

    NO: {
        name: $WebPortal.Resources.Strings.No,
        icon: $WebPortal.Resources.Images.Cross
    },

    OK: {
        name: $WebPortal.Resources.Strings.OK,
        icon: $WebPortal.Resources.Images.Tick
    },

    CANCEL: {
        name: $WebPortal.Resources.Strings.Cancel,
        icon: $WebPortal.Resources.Images.Cross
    },

    RETRY: {
        name: $WebPortal.Resources.Strings.Retry,
        icon: $WebPortal.Resources.Images.Refresh
    },

    NEXT: {
        name: $WebPortal.Resources.Strings.Next,
        icon: $WebPortal.Resources.Images.ForwardArrow
    },

    BACK: {
        name: $WebPortal.Resources.Strings.Back,
        icon: $WebPortal.Resources.Images.BackwardArrow
    }
};

//@ sourceURL=Button.js
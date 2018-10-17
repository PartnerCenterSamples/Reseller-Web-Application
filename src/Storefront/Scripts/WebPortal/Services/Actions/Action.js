/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Services.Action = function (id, displayName, handler, icon, tooltip, children, enabled) {
    /// <summary>
    /// Defines an action that can be displayed in the header bar.
    /// </summary>
    /// <param name="id">The action ID.</param>
    /// <param name="displayName">The action display name. You can pass a display name and/or an icon but you need at least to pass one.</param>
    /// <param name="handler">A function that gets called when the action is clicked. Signature: onActionClicked(menuItem).</param>
    /// <param name="icon">An optional icon.</param>
    /// <param name="tooltip">The action tooltip. If not provided the display name will be used.</param>
    /// <param name="children">Child actions. Optional.</param>
    /// <param name="enabled">The enabled state of the action. True by default.</param>

    $WebPortal.Helpers.throwIfNotSet(id, "id", "Microsoft.WebPortal.Core.Action.Constructor");

    if (!displayName && !icon) {
        $WebPortal.Diagnostics.errorLocal("Microsoft.WebPortal.Core.Action.Constructor: displayName and icon can't be both empty.");
        throw new Error("Please pass a non empty action displayName or icon");
    }

    this.id = ko.observable(id);
    this.displayName = ko.observable(displayName);
    this.handler = handler;
    this.icon = ko.observable(icon);
    this.tooltip = ko.observable(tooltip || displayName);
    this.enabled = ko.observable(enabled !== false);
    this.children = ko.observableArray(children || []);

    // set the action's parent to null by default
    this.parent = ko.observable(null);

    // listen to the childern array's changes
    this.children.subscribe(function () {
        // set the children's parent to us!
        for (var index in this.children()) {
            this.children()[index].parent(this);
        }
    }, this);

    // update the children's parent (if any)
    this.children.valueHasMutated();

    // the background color of the action
    this.backgroundColor = ko.observable();

    // the text color of the action
    this.textColor = ko.observable();

    // the sub menu horizontal position
    this.subMenuXPosition = ko.observable();

    // the sub menu min width
    this.subMenuMinWidth = ko.observable();

    // controls if we show the action sub menu or not
    this.isSubMenuShown = ko.observable(false);

    // computed properties based on the action state //

    this.elementId = ko.computed(function () {
        // the _elementId is used to ID the rendered HTML tag and is based on the ID
        return "action_" + this.id();
    }, this);

    this.cursor = ko.computed(function () {
        // let the mouse cursor be pointer is the action is enabled to indicate interaction
        return this.enabled() ? "pointer" : "default";
    }, this);
   
    this.arrowTransformation = ko.computed(function () {
        // controls the 3d transformation applied to the sub actions arrow, rotates the arrow when the sub menu is shown
        return this.isSubMenuShown() ? "rotate(180deg)" : "rotate(0deg)";
    }, this);

    this.opacity = ko.computed(function () {
        // the element opacity simulates enabling or disabling an action
        return this.enabled() ? 1.0 : 0.4;
    }, this);

    this.hasChildren = ko.computed(function () {
        // determines if the action is compound or not
        return this.children().length > 0;
    }, this);

    this.renderArrow = ko.computed(function () {
        // determines whether to render an arrow signifying a drop down menu for the action or not
        return this.hasChildren() && this.id() != $WebPortal.Settings.Ids.ExtraActionsMenuItem;
    }, this);

    this.isRoot = ko.computed(function () {
        // determines whether this is a root action or a child one
        return this.parent() == null;
    }, this);

    this.onClick = function (action, event) {
        /// <summary>
        /// Handles clicking an action.
        /// </summary>
        /// <param name="action">The action that was clicked.</param>
        /// <param name="event">The event object.</param>

        if (this.enabled()) {
            // grab the current sub menu shown state
            var isSubMenuShown = this.isSubMenuShown();

            // hide all menus anyway
            $WebPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.HideMenus, null, this);

            if (!this.hasChildren()) {
                // call the handler if this is a simple action
                if (this.handler) {
                    this.handler(this);
                }

                // consume the click event
                if (event) {
                    event.stopPropagation();
                }
            } else if (!isSubMenuShown) {
                // this is a compound action which had its menu hidden, align the menu to the action and its min width to the action's width
                this.subMenuXPosition($("#" + this.elementId()).position().left);
                this.subMenuMinWidth( ( $("#" + this.elementId()).width() + 2 ) + "px");

                // show the menu
                this.isSubMenuShown(true);

                // cancel event bubbling as we do not want to hide the menu we have just shown
                if (event) {
                    event.stopPropagation();
                }
            }
        } else {
            // the action is disabled, consume the event so that it does not bubble up to higher levels and possibly hide a displayed menu
            if (event) {
                event.stopPropagation();
            }
        }
    }

    this.onHover = function () {
        /// <summary>
        /// Called when the mouse goes over the action space.
        /// </summary>
        /// <param name="topLevel">Whether this action is a top level one or a child action.</param>

        if (this.enabled()) {
            if (this.isRoot()) {
                // if this is a top level action then set the background to the alternate tile color 
                this.backgroundColor($WebPortal.activeTile().AlternateColor);
            } else {
                // otherwise set the background to the default menu hover background color and the text color to the menu hover text color
                this.backgroundColor($WebPortal.Configuration.ActionBar.MenuHoverBackgroundColor);
                this.textColor($WebPortal.Configuration.ActionBar.MenuHoverTextColor);
            }
        }
    }

    this.onUnhover = function () {
        /// <summary>
        /// Called when the mouse goes out of the action space.
        /// </summary>
        /// <param name="topLevel">Whether this action is a top level one or a child action.</param>

        if (this.enabled()) {
            if (this.isRoot()) {
                // if this is a top level action then inherit the background from the parent (parent's background is the active tile color)
                this.backgroundColor("");
            } else {
                // otherwise set the background to the default menu background color and the text color to the menu text color
                this.backgroundColor($WebPortal.Configuration.ActionBar.MenuBackgroundColor);
                this.textColor($WebPortal.Configuration.ActionBar.MenuTextColor);
            }
        }
    }
}

//@ sourceURL=Action.js
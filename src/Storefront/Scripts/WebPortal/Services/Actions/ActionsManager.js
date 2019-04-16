Microsoft.WebPortal.Services.ActionsManager = function (webPortal, elementSelector, maxActionsInBar, actionsTemplate) {
    /// <summary>
    /// The actions manager maintains and renders actions which the user can invoke. It provides APIs to add and remove actions.
    /// Actions can be enabled or disabled. Actions also can be simple or compound.
    /// A compound action has children actions and displays a drop down menu when clicked.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="elementSelector">The JQuery selector for the HTML element hosting the actions.</param>
    /// <param name="maxActionsInBar">The maximum number of actions to add to the bar before going to overflow mode. Default is
    /// Microsoft.WebPortal.Configuration.ActionsBar.MaxActions
    /// <param name="actionsTemplate">The knock out template to use to render the actions. Optional. Used to override the default template.</param>
    /// </param>

    if (!webPortal) {
        throw new Error("Microsoft.WebPortal.Services.ActionsManager: Invalid webPortal instance.");
    }

    this.webPortal = webPortal;

    this.webPortal.Helpers.throwIfNotSet(elementSelector, "elementSelector", "Microsoft.WebPortal.Services.ActionsManager.Constructor.");
    this.elementSelector = elementSelector;

    // the maximum number of actions to add before adding new actions to the (...) action to conserve width space
    this._maxActionsInBar = maxActionsInBar || this.webPortal.Configuration.ActionBar.MaxActions;

    this.actionsTemplate = actionsTemplate || "actions-template";
    this.actions = ko.observableArray();

    // after a certain number, the actions will be grouped under a (...) action to conserve space
    this._extraActions = new Microsoft.WebPortal.Services.Action(this.webPortal.Settings.Ids.ExtraActionsMenuItem, null, null,
        this.webPortal.Resources.Images.Ellipsis, this.webPortal.Resources.Strings.MoreActionsTooltip);

    // the animations used when adding and removing actions
    this.addActionAnimation = this.webPortal.Configuration.ActionBar.getAddActionAnimation();
    this.removeActionAnimation = this.webPortal.Configuration.ActionBar.getRemoveActionAnimation();

    // the serializer is used to ensure that adding and removing actions execute sequentially. Since each operation is asynchronous,
    // we need to ensure the animation is finished before we start with the next operation
    this.operationSerializer = new Microsoft.WebPortal.Utilities.AsyncOperationSerializer();
};

Microsoft.WebPortal.Services.ActionsManager.prototype.render = function () {
    /// <summary>
    /// Renders the actions.
    /// </summary>

    // listen to the hide menus event
    this.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.HideMenus, this._onHideMenusEvent, this);
};

Microsoft.WebPortal.Services.ActionsManager.prototype.destroy = function () {
    /// <summary>
    /// Destroys the actions manager.
    /// </summary>

    // remove all actions
    this.clear();

    // stop listening to the hide menus event
    this.webPortal.EventSystem.unsubscribe(Microsoft.WebPortal.Event.HideMenus, this._onHideMenusEvent, this);
};

Microsoft.WebPortal.Services.ActionsManager.prototype.add = function (action) {
    /// <summary>
    /// Adds an action to the bar. See the Microsoft.WebPortal.Services.Action class to learn how to create and configure actions.
    /// </summary>
    /// <param name="action">The action to add. Must have unique ID. Otherwise, the existing action will be kept./param>

    if (action) {
        this.webPortal.Diagnostics.informationLocal("Microsoft.WebPortal.Services.ActionsManager.add: enqueuing request.");

        // enqueue the action add as a request to the serializer. This will guarantee actions are added and removed squentially i.e. after animations are complete
        this.operationSerializer.queue(this, this._add, action);
    } else {
        this.webPortal.Diagnostics.warningLocal("Microsoft.WebPortal.Services.ActionsManager.add: action is undefined.");
    }
};

Microsoft.WebPortal.Services.ActionsManager.prototype.addRange = function (actions) {
    /// <summary>
    /// Adds a list of actions to the action bar.
    /// </summary>
    /// <param name="actions">An array of Microsoft.WebPortal.Services.Action objects.</param>

    if (actions) {
        for (var i = 0; i < actions.length; ++i) {
            this.add(actions[i]);
        }
    } else {
        this.webPortal.Diagnostics.warningLocal("Microsoft.WebPortal.Services.ActionsManager.addRange: actions is undefined.");
    }
};

Microsoft.WebPortal.Services.ActionsManager.prototype.remove = function (action) {
    /// <summary>
    ///  Removes an action from the action bar.
    /// </summary>
    /// <param name="action">The action to remove.</param>

    if (action) {
        this.webPortal.Diagnostics.informationLocal("Microsoft.WebPortal.ActionsManager.remove: enqueuing request.");

        // enqueue the action remove as a request to the serializer. This will guarantee actions are added and removed squentially i.e. after animations are complete
        this.operationSerializer.queue(this, this._remove, action);
    } else {
        this.webPortal.Diagnostics.warningLocal("Microsoft.WebPortal.ActionsManager.remove: action is undefined.");
    }
};

Microsoft.WebPortal.Services.ActionsManager.prototype.removeById = function (actionId) {
    /// <summary>
    /// Removes an action from the action bar using its ID.
    /// </summary>
    /// <param name="actionId">The ID of the action to remove.</param>

    if (actionId) {
        this.webPortal.Diagnostics.informationLocal("Microsoft.WebPortal.ActionsManager.removeById: enqueuing request.");

        // enqueue the action remove as a request to the serializer. This will guarantee actions are added and removed squentially i.e. after animations are complete
        this.operationSerializer.queue(this, this._remove, null, actionId);
    } else {
        this.webPortal.Diagnostics.warningLocal("Microsoft.WebPortal.ActionsManager.removeById: actionId is undefined.");
    }
};

Microsoft.WebPortal.Services.ActionsManager.prototype.clear = function () {
    /// <summary>
    /// Removes all registered actions from the action bar.
    /// </summary>

    this.webPortal.Diagnostics.informationLocal("Microsoft.WebPortal.ActionsManager.clear: enqueuing request.");
    this.operationSerializer.queue(this, this._clear);
};

Microsoft.WebPortal.Services.ActionsManager.prototype._add = function (operationResolver, action) {
    /// <summary>
    /// Private method. Adds an action to the action bar.
    /// </summary>
    /// <param name="operationResolver">The object used to signal that the add is complete.</param>
    /// <param name="action">The action to add.</param>

    if (action) {
        if (this._searchActionsById(this.actions(), action.id())) {
            this.webPortal.Diagnostics.errorLocal("Microsoft.WebPortal.ActionsManager.add: An action must have a unique 'Id' property.");
            operationResolver.resolve();
            return;
        }

        if (this.actions().length > this._maxActionsInBar) {
            // we are in overflow mode, append the action to the (...) action
            action.parent(this._extraActions);
            this._extraActions.children.push(action);
            operationResolver.resolve();
        } else {
            if (this.actions().length === this._maxActionsInBar) {
                // we have reached our max actions, clear the (...) from any previous children
                this._extraActions.children.splice(0, this._extraActions.children().length);

                // add the new action to the (...) children
                action.parent(this._extraActions);
                this._extraActions.isSubMenuShown(false);
                this._extraActions.children.push(action);

                // add the (...) to the action bar
                action = this._extraActions;
            }

            // add the action to the bar and animate it
            this.actions.push(action);
            this.addActionAnimation.show("#" + action.elementId()).always(function () {
                // the animation has completed, resolve the request
                operationResolver.resolve();
            });
        }
    } else {
        operationResolver.resolve();
    }
};

Microsoft.WebPortal.Services.ActionsManager.prototype._remove = function (operationResolver, action, actionId) {
    /// <summary>
    /// Private method. Removes an action from the action bar.
    /// </summary>
    /// <param name="operationResolver">The object used to signal that the removal is complete.</param>
    /// <param name="action">The action to remove.</param>
    /// <param name="actionId">An optional parameter sent to delete an action using its ID.</param>
    if (action) {
        if (!this._validateAction(action)) {
            this.webPortal.Diagnostics.warningLocal("Microsoft.WebPortal.ActionsManager.remove: action is not stored in the actions manager.");
            operationResolver.resolve();
            return;
        }
    } else {
        // try finding the action by id
        action = this._searchActionsById(this.actions(), actionId);

        if (!action) {
            this.webPortal.Diagnostics.warningLocal("Microsoft.WebPortal.ActionsManager.remove: action is not stored in the actions manager.");
            operationResolver.resolve();
            return;
        }
    }

    if (action.parent()) {
        // this action is not top level, remove it from its parent
        action.parent().children.remove(action);

        if (action.parent().children().length === 0) {
            // parent no longer has children, remove it too
            this._remove(operationResolver, action.parent());
        } else {
            // we are done
            operationResolver.resolve();
        }
    } else {
        var self = this;

        // this is a top level action, execute the hide animation
        this.removeActionAnimation.hide("#" + action.elementId()).always(function () {
            if (self.actions().length >= self._maxActionsInBar && self._extraActions.children() && self._extraActions.children().length > 0) {
                // we have items under (...), let's move the first one to the end of the top bar

                // remove the first item inside the (...)
                var itemToAdd = self._extraActions.children.splice(0, 1)[0];

                // set it as a top level action and reset its background and text colors
                itemToAdd.parent(null);
                itemToAdd.onUnhover();

                // remove the action as usual
                self.actions.remove(action);

                // insert the sub item to the tail of the action list but before the (...) action
                self.actions.splice(self._maxActionsInBar - 1, 0, itemToAdd);

                // animate the action
                self.addActionAnimation.show("#" + itemToAdd.elementId()).always(function () {
                    if (self._extraActions.children().length === 0) {
                        // remove the (...) since it is now empty
                        self._remove(operationResolver, self._extraActions);
                    } else {
                        // we are done
                        operationResolver.resolve();
                    }
                });
            } else {
                // just remove the action
                self.actions.remove(action);
                operationResolver.resolve();
            }
        });
    }
};

Microsoft.WebPortal.Services.ActionsManager.prototype._clear = function (operationResolver) {
    /// <summary>
    /// Clears all actions.
    /// </summary>
    /// <param name="operationResolver">The object used to signal that the clearing is complete.</param>

    this._hideCompoundActionMenus();

    // clear the (...) children first as we don't want these to be shifted into the main bar when the existing items are deleted
    this._extraActions.children().splice(0, this._extraActions.children().length);

    if (this.actions().length > 0) {
        var self = this;

        this.removeActionAnimation.hide(self.elementSelector + " .Action").always(function () {
            if ($(self.elementSelector + " .Action").find(":animated").length === 0) {
                // all animations are now complete
                self.actions.removeAll();
                operationResolver.resolve();
            }
        });
    } else {
        operationResolver.resolve();
    }
};

Microsoft.WebPortal.Services.ActionsManager.prototype._validateAction = function (action) {
    /// <summary>
    /// Ensures the given action is stored in the actions array.
    /// </summary>
    /// <param name="action">The action to validate.</param>
    /// <returns type="Boolean">True is found, false otherwise.</returns>

    for (var i in this.actions()) {
        if (this.actions()[i] === action) {
            return true;
        }
    }

    return false;
};

Microsoft.WebPortal.Services.ActionsManager.prototype._searchActionsById = function (actionsArray, actionId) {
    /// <summary>
    /// Searches for an action by ID. Scans the given actions array as well as any children actions.
    /// </summary>
    /// <param name="actionsArray">The actions array to scan.</param>
    /// <param name="actionId">The action ID to look for.</param>
    /// <returns type="">The found action or null if nothing was found.</returns>

    var matchingAction = null;

    if (actionsArray && actionId) {
        for (var i = 0; i < actionsArray.length; ++i) {
            if (actionsArray[i].id() === actionId) {
                // found it, exit
                matchingAction = actionsArray[i];
                break;
            } else {
                // look in the action's children
                matchingAction = this._searchActionsById(actionsArray[i].children(), actionId);

                if (matchingAction) {
                    // we found it there, exit
                    break;
                }
            }
        }
    }

    return matchingAction;
};

Microsoft.WebPortal.Services.ActionsManager.prototype._hideCompoundActionMenus = function () {
    /// <summary>
    /// Hides all action menus.
    /// </summary>

    for (var i in this.actions()) {
        this.actions()[i].isSubMenuShown(false);
    }
};

Microsoft.WebPortal.Services.ActionsManager.prototype._onHideMenusEvent = function (eventId, context, broadcaster) {
    /// <summary>
    /// Called in response to a hide menus event. Hides any displayed compound action menus.
    /// </summary>
    /// <param name="eventId">The event id.</param>
    /// <param name="context">The event context.</param>
    /// <param name="broadcaster">broadcaster of the event.</param>

    this._hideCompoundActionMenus();
};

//@ sourceURL=ActionsManager.js
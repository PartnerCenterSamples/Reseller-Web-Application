Microsoft.WebPortal.Core.EventSystem = function (webPortal) {
    /// <summary>
    /// The event system implements an observer pattern where a component can raise an event and
    /// any other component interested in that event can listen to it.
    /// </summary>
    /// <param name="webPortal">An instance of the portal shell to use its services.</param>

    if (!webPortal) {
        throw new Error("Microsoft.WebPortal.Core.EventSystem.Constructor: Invalid web portal instance.");
    }

    this.webPortal = webPortal;

    // a hash table that maps event ids to a list of listeners
    this.eventListenerMap = {};
}

Microsoft.WebPortal.Core.EventSystem.prototype.subscribe = function (eventId, listenerMethod, listenerObject) {
    /// <summary>
    /// Subscribes the given listener function to be fired when an event occurs.
    /// </summary>
    /// <param name="eventId">The ID of the event to be subscribed to. Use Microsoft.WebPortal.Event enumeration.</param>
    /// <param name="listenerMethod"> A function with the following signature: listenerMethod(eventId, context, broadcaster) where the event Id is passed alongside
    /// an optional context object and an optional broadcaster.
    /// </param>
    /// <param name="listenerObject">The object on which to invoke the listener method. Optional.</param>

    this.webPortal.Helpers.throwIfNotSet(eventId, "eventId", "Microsoft.WebPortal.Core.EventSystem.subscribe");
    this.webPortal.Helpers.throwIfNotSet(listenerMethod, "listenerMethod", "Microsoft.WebPortal.Core.EventSystem.subscribe");
    listenerObject = listenerObject || null;

    if (!this.eventListenerMap[eventId]) {
        this.eventListenerMap[eventId] = new Array();
    } else {
        for (var i = 0; i < this.eventListenerMap[eventId].length; ++i) {
            if (this.eventListenerMap[eventId][i].listenerMethod === listenerMethod && this.eventListenerMap[eventId][i].listenerObject === listenerObject) {
                // the listener is already there, exit
                return;
            }
        }
    }

    // if we got here then this is a brand new listener, add it
    this.eventListenerMap[eventId].push({
        listenerMethod: listenerMethod,
        listenerObject: listenerObject
    });
}

/*
    
    @Param eventId 
    @Param listener the event listener function.
*/
Microsoft.WebPortal.Core.EventSystem.prototype.unsubscribe = function (eventId, listenerMethod, listenerObject) {
    /// <summary>
    /// Stops sending events of a given ID to the listener.
    /// </summary>
    /// <param name="eventId">The ID of the event to be unsubscribed from. Use Microsoft.WebPortal.Event enumeration.</param>
    /// <param name="listenerMethod">The listener method to unsubscribe.</param>
    /// <param name="listenerObject">The object which the listener method belongs to. Optional.</param>

    this.webPortal.Helpers.throwIfNotSet(eventId, "eventId", "Microsoft.WebPortal.Core.EventSystem.unsubscribe");
    this.webPortal.Helpers.throwIfNotSet(listenerMethod, "listenerMethod", "Microsoft.WebPortal.Core.EventSystem.unsubscribe");
    listenerObject = listenerObject || null;

    // check if there are listeners for the given event id
    if (this.eventListenerMap[eventId]) {
        // find the listener
        for (var i = 0; i < this.eventListenerMap[eventId].length; ++i) {
            if (this.eventListenerMap[eventId][i].listenerMethod === listenerMethod && this.eventListenerMap[eventId][i].listenerObject === listenerObject) {
                // remove it
                this.eventListenerMap[eventId].splice(i, 1);
                return;
            }
        }
    }
}

Microsoft.WebPortal.Core.EventSystem.prototype.broadcast = function (eventId, context, broadcaster) {
    /// <summary>
    /// Notifies all subscribers of an event.
    /// </summary>
    /// <param name="eventId">The event id. Use Microsoft.WebPortal.Event enumeration. Required.</param>
    /// <param name="context">An optional object to be sent to the subscribers.</param>
    /// <param name="broadcaster">The event broadcaster. Optional.</param>

    this.webPortal.Helpers.throwIfNotSet(eventId, "eventId", "Microsoft.WebPortal.Core.EventSystem.broadcast");

    // check if there are listeners for the given event id
    if (this.eventListenerMap[eventId]) {
        for (var i = 0; i < this.eventListenerMap[eventId].length; ++i) {
            // notify the listeners sequentially
            this.eventListenerMap[eventId][i].listenerMethod.call(this.eventListenerMap[eventId][i].listenerObject, eventId, context, broadcaster);
        }
    }
}
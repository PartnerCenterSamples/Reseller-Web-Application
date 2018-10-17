/// <reference path="~/Scripts/_references.js" />

/*
    define the namespace of the web portal.
*/
var Microsoft = {
    WebPortal: {
        /*
            The core namespace.
        */
        Core: {},

        /*
            The views namespace.
        */
        Views: {},

        /*
            The controls namespace.
        */
        Controls: {},

        /*
            The services namespace.
        */
        Services: {},

        /*
            The infrastructure namespace.
        */
        Infrastructure: {},

        /*
            The utilities namespace.
        */
        Utilities: {},

        /*
            The supported content types used for AJAX requests.
        */
        ContentType: {
            Json: "Json"
        },

        /*
            The supported HTTP methods.
        */
        HttpMethod: {
            Get: "GET",
            Post: "POST",
            Delete: "DELETE",
            Put: "PUT",
            Patch: "PATCH"
        },

        /*
            The supported animation effects.
        */
        Effects: {
            /*
                Fades the content in or out.
            */
            Fade: {
                Name: "fade"
            },

            /*
                Clips the content in or out.
            */
            Clip: {
                Name: "clip"
            },

            /*
                Sweeps content from the right of the screen when shown and to the left when hidden.
            */
            SweepLeft: {
                Name: "slide",
                ShowOptions: {
                    direction: "right"
                },
                HideOptions: {
                    direction: "left"
                }
            },

            /*
                Sweeps content from the left of the screen when shown and to the right when hidden.
            */
            SweepRight: {
                Name: "slide",
                ShowOptions: {
                    direction: "left"
                },
                HideOptions: {
                    direction: "right"
                }
            },

            /*
                A combination of sweep and fade.
            */
            SweepFadeLeft: {
                Name: "drop",
                ShowOptions: {
                    direction: "right"
                },
                HideOptions: {
                    direction: "left"
                }
            },

            /*
                A combination of reverse sweep and fade.
            */
            SweepFadeRight: {
                Name: "drop",
                ShowOptions: {
                    direction: "left"
                },
                HideOptions: {
                    direction: "right"
                }
            },

            SlideDown: {
                Name: "slide",
                ShowOptions: {
                    direction: "up"
                },
                HideOptions: {
                    direction: "up"
                }
            }
        },

        /*
            Event Id enumeration. All events sent in the broadcast-subsribe system should use this enumeration to ID themselves.
            Add your custom events here.
        */
        Event: {
            // framework events

            /*
                Fired when the web portal is initializing.
            */
            PortalInitializing: 1000,

            /*
                Fired when the portal finishes initializing.
            */
            PortalInitialized: 1001,

            /*
                Fired when a feature is activated.
            */
            FeatureActivated: 1002,

            /*
                Fired when a feature is deactivated.
            */
            FeatureDeactivated: 1003,

            /*
                Fired when a feature is destroyed.
            */
            FeatureDestroyed: 1004,

            /*
                Fired when the dialog is shown.
            */
            DialogShown: 1005,

            /*
                Fired when the control panel has completed showing or hiding itself.
            */
            ControlPanelShown: 1006,

            /*
                Fired when the content panel is resized.
            */
            ContentPanelResized: 1007,

            /*
                Fire this to hide all the menus in the portal.
            */
            HideMenus: 1008,

            /*
                Signaled when the primary navigation is about to be shown or hidden
            */
            PrimaryNavigationShowing: 1009,

            /*
                Signaled when a portal service is started.
            */
            ServiceStarted: 1010,

            /*
                Signaled when a portal service is stopped.
            */
            ServiceStopped: 1011,

            /*
                Signaled when a window resize event has occurred. This is the place to fit components into the new window dimensions.
            */
            OnWindowResizing: 1012,

            /*
                Signaled after all components have respected the new window size. This is where these componenets can resize themselves to fit the document
                in case the window had scroll bars.
            */
            OnWindowResized: 1013,

            /*
                Signaled when all the notifications have been cleared.
            */
            NotificationsCleared: 1014,

            // Feature events

            /*
                Fired when a view tab is selected.
            */
            ViewTabActivated: 4000,

            UserLoggedIn: 4001
        }
    }
};
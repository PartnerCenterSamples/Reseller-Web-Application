Microsoft.WebPortal.Infrastructure.KoExtensions = {
    apply: function (webPortal) {
        /// <summary>
        /// Adds Knock out binding extensions useful to the portal features.
        /// </summary>
        /// <param name="webPortal">The web portal instance.</param>

        if (!webPortal) {
            throw new Error("Microsoft.WebPortal.Infrastructure.KoExtensions.apply: Invalid web portal instance.");
        }

        /*
            A custom Knockout binding used to fade in and out the bound element. use the following syntax:
            data-bind= { fadeVisible: yourObservable, duration: 400, onAnimationDone: callbackFunction(isShown) }
            The duration and onAnimationDone bindings are optional.
        */
        ko.bindingHandlers.fadeVisible = {
            init: function (element, valueAccessor) {
                // initially set the element to be instantly visible/hidden depending on the value
                var value = valueAccessor();
                $(element).toggle(ko.utils.unwrapObservable(value));
            },
            update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
                // whenever the value subsequently changes, slowly fade the element in or out
                var value = valueAccessor();

                // read the duration from the binding string, or use the default one if none is provided
                var duration = allBindings().duration || webPortal.Configuration.DefaultAnimationDuration;

                // fade in or out the element
                ko.utils.unwrapObservable(value) ? $(element).fadeIn(duration, function () {
                    Microsoft.WebPortal.Infrastructure.KoExtensions._animationDoneCallback(allBindings, true);
                }) : $(element).fadeOut(duration, function () {
                    Microsoft.WebPortal.Infrastructure.KoExtensions._animationDoneCallback(allBindings, false);
                });
            }
        };

        /*
            A custom Knockout binding used to slide the bound element up and down like a menu item. use the following syntax:
            data-bind= { slideDown: yourObservable, duration: 400, onAnimationDone: callbackFunction(isShown) }
            The duration and onAnimationDone bindings are optional.
        */
        ko.bindingHandlers.slideDown = {
            init: function (element, valueAccessor) {
                // initially set the element to be instantly visible/hidden depending on the value
                var value = valueAccessor();
                $(element).toggle(ko.utils.unwrapObservable(value));
            },
            update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
                // whenever the value subsequently changes, slowly slide the element down or up
                var value = valueAccessor();

                // read the duration from the binding string, or use the default one if none is provided
                var duration = allBindings().duration || webPortal.Configuration.DefaultAnimationDuration;

                ko.utils.unwrapObservable(value) ? $(element).slideDown(duration, function () {
                    Microsoft.WebPortal.Infrastructure.KoExtensions._animationDoneCallback(allBindings, true);
                }) : $(element).slideUp(duration, function () {
                    Microsoft.WebPortal.Infrastructure.KoExtensions._animationDoneCallback(allBindings, false);
                });
            }
        };

        /*
            A custom Knockout binding used to slide the bound element up and down like a menu item. use the following syntax:
            data-bind= { slideDown: yourObservable, duration: 400, onAnimationDone: callbackFunction(isShown) }
            The duration and onAnimationDone bindings are optional.
        */
        ko.bindingHandlers.slideDown2 = {
            init: function (element, valueAccessor) {
                // initially set the element to be instantly visible/hidden depending on the value
                var value = valueAccessor();
                $(element).toggle(ko.utils.unwrapObservable(value));
            },
            update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
                // whenever the value subsequently changes, slowly slide the element down or up
                var value = valueAccessor();

                // read the duration from the binding string, or use the default one if none is provided
                var duration = allBindings().duration || webPortal.Configuration.DefaultAnimationDuration;

                ko.utils.unwrapObservable(value) ? $(element).show("slide", { direction: "up" }, duration, function () {
                    Microsoft.WebPortal.Infrastructure.KoExtensions._animationDoneCallback(allBindings, true);
                }) : $(element).hide("slide", { direction: "up" }, duration, function () {
                    Microsoft.WebPortal.Infrastructure.KoExtensions._animationDoneCallback(allBindings, false);
                });
            }
        };

        /*
            A custom Knockout binding used to throttle updates to an HTML input field to a given interval. Pass it a callback function in your
            binding. You can also set the throttling time in milliseconds by adding: "throttle: 600" to your binding. Default is 450 ms.
        */
        ko.bindingHandlers.throttledChange = {
            init: function (element, valueAccessor) {
            },
            update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
                // the observable has been updated...
                // read the value of the throttle binding
                var throttle = allBindings().throttle || webPortal.Configuration.DefaultThrottlingDuration;

                // grab the provided callback function
                var value = valueAccessor;
                var callBackFunction = ko.unwrap(value);

                if (callBackFunction) {
                    if (!element.throttlingFunction) {
                        // assign the throttling function to the HTML element
                        element.throttlingFunction = webPortal.Helpers.throttle(function () { callBackFunction(); }, throttle);
                    }

                    // invoke the throttled callback
                    element.throttlingFunction();
                }
            }
        };

        /*
            This is very useful in case of averlapped KO bindings. Sometimes we want to bind nested HTML elements to different view models.
            Adding a : <!-- ko stopBinding: true --> <div id="innerContent" /><!-- /ko --> will prevent the parent binding from taking control of this element
            and thus enables binding the child element to a different view model.
        */
        ko.bindingHandlers.stopBinding = {
            init: function () {
                return { controlsDescendantBindings: true };
            }
        };

        ko.virtualElements.allowedBindings.stopBinding = true;
    },

    _animationDoneCallback: function (allBindings, isShown) {
        /// <summary>
        /// Private method. Invokes the animation done call back if specified in the KO bindings.
        /// </summary>
        /// <param name="allBindings">The KO bindings object.</param>
        /// <param name="isShown">Is the element shown or hidden.</param>

        if (allBindings().onAnimationDone) {
            allBindings().onAnimationDone(isShown);
        }
    }
};
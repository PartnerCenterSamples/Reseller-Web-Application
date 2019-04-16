Microsoft.WebPortal.Utilities.Toggler = {
    injectToggling: function (targetObject, showFunction, hideFunction, isShown) {
        /// <summary>
        /// Adds toggling behavior to the provided object. The object will support show(), hide(), toggle() and isShown() methods. The inner
        /// shown state is managed automatically.
        /// </summary>
        /// <param name="targetObject">Required. The object to which to add the toggling behavior.</param>
        /// <param name="showFunction">Required. The function to call to show. The function will receive a JQuery deferred object which it has to resolve when showing is complete.</param>
        /// <param name="hideFunction">Required. The function to call to hide. The function will receive a JQuery deferred object which it has to resolve when hiding is complete.</param>
        /// <param name="isShown">The initial show state of the object. Boolean. Default is false.</param>

        $WebPortal.Helpers.throwIfNotSet(targetObject, "targetObject", "Microsoft.WebPortal.Utilities.Helpers.implementToggling");
        $WebPortal.Helpers.throwIfNotSet(showFunction, "showFunction", "Microsoft.WebPortal.Utilities.Helpers.implementToggling");
        $WebPortal.Helpers.throwIfNotSet(hideFunction, "hideFunction", "Microsoft.WebPortal.Utilities.Helpers.implementToggling");

        if (targetObject.show || targetObject.hide || targetObject.toggle || targetObject.isShown) {
            $WebPortal.Diagnostics.warning("Microsoft.WebPortal.Utilities.Toggler.injectToggling: the provided object has a show, hide, toggle or isShown properties, overwritting.");
        }

        // add the shown state property to the object
        targetObject.isShown = ko.observable(isShown === true);

        // show and hide requests will be serialized since they are asynchronous
        var serializer = new Microsoft.WebPortal.Utilities.AsyncOperationSerializer();

        var showTask = function (taskProgress, showProgress, show, operation, parameters) {
            /// <summary>
            /// The task that will be called by the serializer when it is turn. It will either show or hide the object.
            /// </summary>
            /// <param name="taskProgress">A Jquery deferred object we must resolve to finish the task.</param>
            /// <param name="showProgress">A Jquery deferred object that tracks showing progress.</param>
            /// <param name="show">A boolean to specify whether to show or hide. Default is true.</param>
            /// <param name="operation">The function to call in order to show or hide.</param>
            /// <param name="parameters">The parameters to send to the function.</param>

            show = show !== false;

            showProgress.done(function () {
                targetObject.isShown(show);
            }).always(function () {
                // show or hide is complete, resolve the task
                taskProgress.resolve();
            });

            if (targetObject.isShown() !== show) {
                var taskArguments = [showProgress];
                if (parameters) {
                    for (var i in parameters) {
                        taskArguments.push(parameters[i]);
                    }
                }

                operation.apply(targetObject, taskArguments);
            } else {
                showProgress.resolve();
            }
        };

        // implement a show method on the object
        targetObject.show = function () {
            /// <summary>
            /// Shows the object.
            /// </summary>
            /// <returns type="$.Deferred">A JQuery deferred object whose listener functions will be called upon show completion.</returns>

            var showProgress = $.Deferred();
            serializer.queue(null, showTask, showProgress, true, showFunction, arguments);
            return showProgress;
        };

        // implement a hide method on the object
        targetObject.hide = function () {
            /// <summary>
            /// Hides the object
            /// </summary>
            /// <returns type="$.Deferred">A JQuery deferred object whose listener functions will be called upon hide completion.</returns>

            var hideProgress = $.Deferred();
            serializer.queue(null, showTask, hideProgress, false, hideFunction, arguments);
            return hideProgress;
        };

        targetObject.toggle = function () {
            /// <summary>
            /// Toggles the object between shown and hidden.
            /// </summary>
            /// <returns type="$.Deferred">A JQuery deferred object whose listener functions will be called upon toggle completion.</returns>

            var toggleProgress = $.Deferred();
            var parameters = arguments;

            serializer.queue(null, function (taskProgress) {
                /// <summary>
                /// the task that will be called by the serializer when it is turn.
                /// </summary>
                /// <param name="taskProgress">A Jquery deferred object we must resolve.</param>

                if (targetObject.isShown()) {
                    showTask(taskProgress, toggleProgress, false, hideFunction, parameters);
                } else {
                    showTask(taskProgress, toggleProgress, true, showFunction, parameters);
                }
            });

            return toggleProgress;
        };
    }
};

//@ sourceURL=Toggler.js
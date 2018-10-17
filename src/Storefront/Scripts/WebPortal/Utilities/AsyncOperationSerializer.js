/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Utilities.AsyncOperationSerializer = function () {
    /// <summary>
    /// This class ensures that asynchronous operations are run sequentially. This is useful when you want to perform an animation let's say when adding an
    /// item to a menu but only after the previous animations are done. This is also useful to guarantee object integrity if the user clicks too fast on something
    /// that executes an async operation.
    /// </summary>

    this.operationQueue = [];
    this.isProcessing = false;
}

Microsoft.WebPortal.Utilities.AsyncOperationSerializer.prototype.queue = function (targetObject, targetMethod) {
    /// <summary>
    /// Adds an operation to the serializer's queue. The serializer will execute the requests sequentially and will wait for each request to resolve before
    /// executing the next in line. When it is time to execute the operation, the serializer will perform the following call: targetObject.targetMethod(deferred, arguments).
    /// You can pass optional arguments right after the first two. These will be served as arguments to the target method.
    /// </summary>
    /// <param name="targetObject">The object which owns the target method. If not set, the method will be called on its own: targetMethod(deferred, arguments)</param>
    /// <param name="targetMethod">The method to execute. Required. It will be passed the deferred object as the first argument followed by the given arguments. The method MUST
    /// resolve or reject the deferred object once it is complete to allow the serializer to process the next operations in line.
    /// </param>

    $WebPortal.Helpers.throwIfNotSet(targetMethod, "targetMethod", "Microsoft.WebPortal.Core.AsyncOperationSerializer.queue");

    this.operationQueue.push(arguments);

    if (!this.isProcessing) {
        // there are no operations executing at the moments, start processing...
        this.isProcessing = true;
        this._process();
    }
}

Microsoft.WebPortal.Utilities.AsyncOperationSerializer.prototype._process = function () {
    /// <summary>
    /// Private method. Processes the requests sequentially until the request queue is empty.
    /// </summary>

    if (this.operationQueue.length > 0) {
        var operationResolver = $.Deferred();
        var currentOperation = this.operationQueue[0];

        // acquire the target object, method and any passed arguments
        var targetObject = currentOperation[0];
        var targetMethod = currentOperation[1];

        // add the deferred as the first argument to be passed to the target method
        var methodArguments = [ operationResolver ];

        // add the provided arguments to argument list
        for (var i = 2; i < currentOperation.length; ++i) {
            methodArguments.push(currentOperation[i]);
        }
        
        $WebPortal.Diagnostics.informationLocal("AsyncOperationSerializer: Processing request: " + targetObject + ":" + targetMethod.toString().slice(0, 100) + "...");

        // invoke the method!
        targetMethod.apply(targetObject, methodArguments);

        var self = this;

        operationResolver.always(function () {
            // operation is complete, remove it from the queue
            self.operationQueue.splice(0, 1);
            $WebPortal.Diagnostics.informationLocal("AsyncOperationSerializer: Processing complete:" + targetObject + ":" + targetMethod.toString().slice(0, 100) + "...");

            // process the next request but do it outside the current call stack
            window.setTimeout(function () {
                self._process();
            }, 0);
        });
    } else {
        $WebPortal.Diagnostics.informationLocal("AsyncOperationSerializer: All requests in queue have been processed.");
        this.isProcessing = false;
    }
}

//@ sourceURL=AsyncOperationSerializer.js
/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Utilities.RetryableServerCall = function (operation, name, retryPolicy) {
    /// <summary>
    /// Retries server AJAX calls according to the configured retry policy.
    /// </summary>
    /// <param name="operation">The server operation. The function must return a JQuery deferred object
    /// in order to be able to examine its outcome. Use $WebPortal.Helpers.ajaxCall utility function to create Ajax operations.</param>
    /// <param name="name">The server call name. Used for logging purposes (optional).</param>
    /// <param name="retryPolicy">An optional array of integers. The array length represents the number
    /// of retries and the item values represent the back off in milliseconds for each retry.</param>

    $WebPortal.Helpers.throwIfNotSet(operation, "operation", "Microsoft.WebPortal.Utilities.RetryableServerCall.Constructor");

    this._reset = function () {
        /// <summary>
        /// Private method. Resets the object's state.
        /// </summary>

        this.currentRetry = 0;
        this.deferred = $.Deferred();
        this.ajaxHandler = null;
    }

    this.name = name || "Unidentified";
    this.operation = operation;
    this.retryPolicy = retryPolicy || Microsoft.WebPortal.Utilities.RetryableServerCall.DefaultRetryPolicy;
    this.state = Microsoft.WebPortal.Utilities.RetryableServerCall.State.Idle;
    this.operationComplete = ko.observable(false);
    this._reset();

    /*
        
        @Return a 
    */
    this.execute = function () {
        /// <summary>
        /// Executes the server call.
        /// </summary>
        /// <returns type="$.Deferred">A JQuery deferred object used to listen to the server call outcome.</returns>

        var self = this;
        $WebPortal.Diagnostics.information("RetryableServerCall(" + self.name + ").execute()");

        if (self.state !== Microsoft.WebPortal.Utilities.RetryableServerCall.State.ProcessingResults) {
            // cancel any previous invokations
            self.cancel();

            // execute the operation
            self._execute(arguments);

            return self.deferred;
        } else {
            // the previous call has returned but the client is still processing this. This maybe due to calling this
            // execute function from the done handler of the previous call or using a window timeout mechanism. We
            // need to ensure the existing callbacks (done, fail, always) are complete before we reset the object state
            $WebPortal.Diagnostics.information("RetryableServerCall(" + self.name + ").execute: state is ProcessingResults.");
            var deferred = $.Deferred();

            // listen to the state observable, this will fire when the existing operation is complete
            var operationCompleteSubscription = self.operationComplete.subscribe(function () {
                $WebPortal.Diagnostics.information("RetryableServerCall(" + self.name + ").execute: Previous operation complete. Executing current operation.");

                // stop listening to future notifictions
                operationCompleteSubscription.dispose();

                self._reset();

                // set our deferred object to the one we returned to the caller previously
                self.deferred = deferred;

                // execute the operation
                self._execute(arguments);
            });

            // return a new deferred object since the current one is still in use
            return deferred;
        }
    }

    this.cancel = function () {
        /// <summary>
        /// Cancels the server call.
        /// </summary>

        $WebPortal.Diagnostics.information("RetryableServerCall(" + this.name + ").cancel()");

        if (this.state !== Microsoft.WebPortal.Utilities.RetryableServerCall.State.ProcessingResults) {
            if (this.ajaxHandler) {
                $WebPortal.Diagnostics.information("RetryableServerCall(" + this.name + "): cancelling AJAX request.");
                this.ajaxHandler.abort();
                $WebPortal.Diagnostics.information("RetryableServerCall(" + this.name + "): cancelled AJAX request.");
            }

            this._reset();
            this.state = Microsoft.WebPortal.Utilities.RetryableServerCall.State.Idle;
            $WebPortal.Diagnostics.information("RetryableServerCall(" + this.name + ").state: Idle.");
        } else {
            // do nothing, the server already sent the results back, resetting would cause unknown behavior
            // success and failure call back will execute normally
            $WebPortal.Diagnostics.information("RetryableServerCall(" + this.name + ").cancel: state is ProcessingResults. Do nothing.");
        }

        if (this.retryTimer) {
            // there is a pending timer to execute a retry, cancel it
            window.clearTimeout(this.retryTimer);
            this.retryTimer = null;
            $WebPortal.Diagnostics.information("RetryableServerCall(" + this.name + ").cancel: Cleared retry timeout.");
        }
    }

    this._execute = function () {
        /// <summary>
        /// Private method. Executes the AJAX call.
        /// </summary>

        var self = this;
        self.state = Microsoft.WebPortal.Utilities.RetryableServerCall.State.InProgress;

        if (self.currentRetry > 0) {
            $WebPortal.Diagnostics.information("RetryableServerCall(" + self.name + ")._execute(): retry: " + self.currentRetry);
        } else {
            $WebPortal.Diagnostics.information("RetryableServerCall(" + self.name + ")._execute()");
        }

        $WebPortal.Diagnostics.information("RetryableServerCall(" + self.name + ").state: InProgress");
        var parameters = arguments;

        // invoke the operation
        var startTime = new Date().getTime();
        self.ajaxHandler = self.operation.apply(self, parameters);

        self.ajaxHandler.done(function (result, status) {
            self.state = Microsoft.WebPortal.Utilities.RetryableServerCall.State.ProcessingResults;
            $WebPortal.Diagnostics.information("RetryableServerCall(" + self.name + ".done().");
            $WebPortal.Diagnostics.information("RetryableServerCall(" + self.name + ").state: ProcessingResults");

            // operation is successful, notify caller
            $WebPortal.Diagnostics.information("RetryableServerCall(" + self.name + ")._execute: success!");
            self.deferred.resolve(result, status);
            self._reset();
        }).fail(function (result, status, error) {

            // handling errors over SSL for IIS6+.             
            if (error.length == 0) {
                error = result.responseText;
            }

            self.state = Microsoft.WebPortal.Utilities.RetryableServerCall.State.ProcessingResults;
            $WebPortal.Diagnostics.information("RetryableServerCall(" + self.name + ".fail().");
            $WebPortal.Diagnostics.information("RetryableServerCall(" + self.name + ").state: ProcessingResults");
            
            if (result.status === 0) {
                switch (status) {
                    case "error":
                        $WebPortal.Diagnostics.information("result.status = 0, status = 'error', refreshing the page for relogin");

                        // result.status = 0 suggests that the call is blocked by server due to an issue and that is very well session timeout
                        // status ="error" comes up when server session times out. Refresh the whole page.
                        window.location.replace(window.location.href);
                        return;
                    case "abort":
                        // operation has been cancelled, exit
                        $WebPortal.Diagnostics.information("RetryableServerCall(" + self.name + ".fail(): exiting due to abort.");
                        return;
                }
            }

            $WebPortal.Diagnostics.error("RetryableServerCall(" + self.name + ")._execute: failed. Result: " + result.status + ". Status: " + status);

            var exceededMaximumRetries = self.currentRetry >= self.retryPolicy.length;

            if (exceededMaximumRetries || Microsoft.WebPortal.Utilities.RetryableServerCall.NonRetryableHttpErrorCodes.indexOf(result.status) != -1) {
                // we have exceeded the retry policy, this is a failure
                $WebPortal.Diagnostics.error("RetryableServerCall(" + self.name + ")._execute: Rejecting call. Retry: " +
                    self.currentRetry + ". " + (exceededMaximumRetries ? "Exceeded max retry policy." : "Non retryable Http error code."));
                self.deferred.reject(result, status, error);

                self._reset();
            } else {
                // back off and retry according to the policy
                $WebPortal.Diagnostics.information("RetryableServerCall(" + self.name + ")._execute: retrying in: " + self.retryPolicy[self.currentRetry] + " Milliseconds.");
                self.retryTimer = window.setTimeout(function () {
                    self.retryTimer = null;
                    self._execute.apply(self, parameters);
                }, self.retryPolicy[self.currentRetry]);

                self.currentRetry++;
            }
        }).always(function () {
            // log the amount of time this call took to complete
            var operationDuration = new Date().getTime() - startTime;
            $WebPortal.Diagnostics.information("RetryableServerCall(" + self.name + ".always().");
            $WebPortal.Diagnostics.information("RetryableServerCall(" + self.name + ")._execute took: " + operationDuration + " Milliseconds.");

            self.state = Microsoft.WebPortal.Utilities.RetryableServerCall.State.Idle;
            $WebPortal.Diagnostics.information("RetryableServerCall(" + self.name + ").state: Idle");

            self.operationComplete.notifySubscribers();
        });
    }
}

// the default retry policy is 2 retries with progressive back off
Microsoft.WebPortal.Utilities.RetryableServerCall.DefaultRetryPolicy = [100, 300];

// the non retryable HTTP error codes 
Microsoft.WebPortal.Utilities.RetryableServerCall.NonRetryableHttpErrorCodes = [400, 401, 403, 404, 405, 409];

// the server call states
Microsoft.WebPortal.Utilities.RetryableServerCall.State = {
    // doing nothing
    Idle: 1,
    // made a call to the server, server has not responded yet
    InProgress: 2,
    // got a response from the server, processing it and invoking callbacks
    ProcessingResults: 3
}

//@ sourceURL=RetryableServerCall.js
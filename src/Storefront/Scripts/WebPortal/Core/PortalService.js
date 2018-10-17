/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Core.PortalService = function (webPortal, name) {
    /// <summary>
    /// The base class all portal services must extend. A service can be run or stopped and is attached to the web portal instance upon running.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="name">The service name. This will be used to access the service from the web portal instance.</param>

    if (!webPortal) {
        throw new Error("Microsoft.WebPortal.Core.PortalService: Invalid webPortal instance.");
    }

    this.webPortal = webPortal;
    this.webPortal.Helpers.throwIfNotSet(name, "name", "Microsoft.WebPortal.Core.PortalService.Constructor");
    this.name = name;
    this.isRunning = false;
}

Microsoft.WebPortal.Core.PortalService.prototype.run = function () {
    /// <summary>
    /// Runs the portal service. Sub classes must provide a _runService function that will be called to start the service.
    /// </summary>

    if (this.isRunning) {
        this.webPortal.Diagnostics.informationLocal(this.name + " portal service is already running.");
        return;
    }

    this.webPortal.Helpers.throwIfNotSet(this._runService, "this._runService", "Microsoft.WebPortal.Core.PortalService.run, service name: " + this.name);
    this.isRunning = true;

    this._runService();
    this.webPortal.Diagnostics.informationLocal(this.name + " portal service is running.");

    // announce the service start
    this.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.ServiceStarted, this, this);
}

Microsoft.WebPortal.Core.PortalService.prototype.stop = function () {
    /// <summary>
    /// Stops the portal service. Sub classes must provide a _stopService that will be called to stop the service.
    /// </summary>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.informationLocal(this.name + " portal service is already stopped.");
        return;
    }

    this.webPortal.Helpers.throwIfNotSet(this._stopService, "this._stopService", "Microsoft.WebPortal.Core.PortalService.stop, service name: " + this.name);
    this._stopService();

    this.isRunning = false;
    this.webPortal.Diagnostics.informationLocal(this.name + " portal service is stopped.");

    // announce the service stop
    this.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.ServiceStopped, this, this);
}

Microsoft.WebPortal.Core.PortalService.prototype.isRunning = function () {
    /// <summary>
    /// Checks if the service is running.
    /// </summary>
    /// <returns type="boolean">true if the service is running, false otherwise.</returns>

    return this.isRunning;
}

//@ sourceURL=PortalService.js
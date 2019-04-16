Microsoft.WebPortal.Services.Dialog = function (webPortal, animation, dialogTemplate, cssClass) {
    /// <summary>
    /// The dialog service. Provides a pop up dialog which can render KO templates and show a set of standard buttons.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="animation">The animation to use in showing and hiding the dialog.</param>
    /// <param name="DialogTemplate">The template used to render the dialog. The default is the metro style dialog.</param>
    /// <param name="cssClass">The CSS class used to stylethe dialog. The default is the metro style.</param>

    this.base.constructor.call(this, webPortal, "Dialog");

    // the template and animation used to render the dialog
    this.template = ko.observable(dialogTemplate || "MetroDialog-template");
    this.cssClass = ko.observable(cssClass || "MetroDialog");
    this.animation = animation || new Microsoft.WebPortal.Utilities.Animation(Microsoft.WebPortal.Effects.Fade, 500);

    // the dialog's content specified by a template, a view model to bind to the template and a set of buttons
    this.contentTemplate = ko.observable();
    this.contentViewModel = ko.observable();
    this.buttons = ko.observableArray([]);
    this.maxHeight = ko.observable(0);
    this.width = ko.observable();
    this.height = ko.observable();

    this.isProgressShown = ko.observable(false);

    // dialog will be togglable
    Microsoft.WebPortal.Utilities.Toggler.injectToggling(this, this.show, this.hide, false);
};

$WebPortal.Helpers.inherit(Microsoft.WebPortal.Services.Dialog, Microsoft.WebPortal.Core.PortalService);

Microsoft.WebPortal.Services.Dialog.prototype._runService = function () {
    /// <summary>
    /// Runs dialog service.
    /// </summary>

    // listen to window resize events
    this.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.OnWindowResizing, this.resize, this);
    this.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.OnWindowResized, this.resize, this);

    // bind to the dialog element
    ko.applyBindings(this, $(this.webPortal.Settings.Ids.Dialog)[0]);
    ko.applyBindings(this, $(this.webPortal.Settings.Ids.DialogShader)[0]);
    ko.applyBindings(this, $(this.webPortal.Settings.Ids.DialogBackgroundOverlay)[0]);
    ko.applyBindings(this, $(this.webPortal.Settings.Ids.DialogProgressIndicator)[0]);
};

Microsoft.WebPortal.Services.Dialog.prototype._stopService = function () {
    /// <summary>
    /// Stops the dialog service.
    /// </summary>

    var self = this;

    // hide the dialog if shown
    this.hide().always(function () {
        // unbind and clean up the dialog elements
        ko.cleanNode($(self.webPortal.Settings.Ids.Dialog)[0]);
        $(self.webPortal.Settings.Ids.Dialog).empty();

        ko.cleanNode($(self.webPortal.Settings.Ids.DialogShader)[0]);
        ko.cleanNode($(self.webPortal.Settings.Ids.DialogBackgroundOverlay)[0]);
        ko.cleanNode($(self.webPortal.Settings.Ids.DialogProgressIndicator)[0]);
    });

    // stop listening to window resize events
    this.webPortal.EventSystem.unsubscribe(Microsoft.WebPortal.Event.OnWindowResizing, this.resize, this);
    this.webPortal.EventSystem.unsubscribe(Microsoft.WebPortal.Event.OnWindowResized, this.resize, this);
};

Microsoft.WebPortal.Services.Dialog.prototype.show = function (showProgress, contentTemplate, contentViewModel, dialogButtons) {
    /// <summary>
    /// Shows the dialog.
    /// </summary>
    /// <param name="showProgress">A Jquery deferred object to resolve when showing is complete.</param>
    /// <param name="contentTemplate">The content template to render.</param>
    /// <param name="contentViewModel">The content view model to bind to the template.</param>
    /// <param name="dialogButtons">An optional array of Microsoft.WebPortal.Services.Button objects. These will be used to control the wizard.</param>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The dialog service is not running.");
        showProgress.reject();
        return;
    }

    if (!contentTemplate) {
        this.webPortal.Diagnostics.error("Microsoft.WebPortal.Services.Dialog.show: Please provide a content template.");
        showProgress.reject();
        return;
    }

    if (!contentViewModel) {
        this.webPortal.Diagnostics.error("Microsoft.WebPortal.Services.Dialog.show: Please provide a content view model.");
        showProgress.reject();
        return;
    }

    dialogButtons = dialogButtons || [];

    // set up the buttons owner to the dialog instance
    for (var i in dialogButtons) {
        dialogButtons[i].owner = this;
    }

    // clear the old template and view model
    this.contentTemplate(null);
    this.contentViewModel(null);

    // set the new ones
    this.contentViewModel(contentViewModel);
    this.contentTemplate(contentTemplate);

    this.buttons(dialogButtons);

    var self = this;
    this.isShown(true);

    this.animation.show(this.webPortal.Settings.Ids.Dialog).always(function () {
        self.maxHeight($("#DialogContentContainer").height() - 65 + "px");

        // fire a dialog shown event
        self.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.DialogShown, true, self);

        showProgress.resolve();
    });
};

Microsoft.WebPortal.Services.Dialog.prototype.hide = function (hideProgress) {
    /// <summary>
    /// Hides the dialog.
    /// </summary>
    /// <param name="hideProgress">A Jquery deferred object to resolve when hiding is complete.</param>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The dialog service is not running.");
        hideProgress.reject();
        return;
    }

    this.hideProgress();
    var self = this;
    this.isShown(false);

    this.animation.hide(this.webPortal.Settings.Ids.Dialog).always(function () {
        // fire a dialog hide event
        self.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.DialogShown, false, self);

        // clear the old template and view model
        self.contentTemplate(null);
        self.contentViewModel(null);

        hideProgress.resolve();
    });
};

Microsoft.WebPortal.Services.Dialog.prototype.showProgress = function () {
    /// <summary>
    /// Shows the dialog progress.
    /// </summary>
    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The dialog service is not running.");
        return;
    }

    if (this.isShown()) {
        this.isProgressShown(true);
    }
};

Microsoft.WebPortal.Services.Dialog.prototype.hideProgress = function () {
    /// <summary>
    /// Hides the dialog progress.
    /// </summary>
    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The dialog service is not running.");
        return;
    }

    this.isProgressShown(false);
};


Microsoft.WebPortal.Services.Dialog.prototype.setTemplate = function (dialogTemplate, cssClass) {
    /// <summary>
    /// Sets the dialog template.
    /// </summary>
    /// <param name="dialogTemplate">The name of the template.</param>
    /// <param name="cssClass">The css class of the dialog.</param>

    this.webPortal.Helpers.throwIfNotSet(dialogTemplate, "dialogTemplate", "Microsoft.WebPortal.Services.Dialog.Constructor.");
    this.webPortal.Helpers.throwIfNotSet(cssClass, "cssClass", "Microsoft.WebPortal.Services.Dialog.Constructor.");
    this.template(dialogTemplate);
    this.cssClass(cssClass);
};

Microsoft.WebPortal.Services.Dialog.prototype.resize = function (eventId) {
    /// <summary>
    /// Resizes the dialog and its overlay to fit the new window dimensions.
    /// </summary>
    /// <param name="eventId">The event Id.</param>

    if (eventId === Microsoft.WebPortal.Event.OnWindowResizing) {
        // reset the dialog and its overlay dimensions to fit in the new window size      
        this.width($(window).width() + "px");
        this.height($(window).height() + "px");
    } else if (eventId === Microsoft.WebPortal.Event.OnWindowResized) {
        // there is a chance that another component is too wide to fit in the new window, causing the document size to be bigger than
        // the window size, setting the dialog and its overlay widths above ensures that if that happens then it is not because of the old 
        // overlay size, anyway, reset the dimensions to fit the new document
        var overlayWidth = $(document).width() + "px";
        var overlayHeight = $(document).height() + "px";

        this.webPortal.Diagnostics.informationLocal("Setting dialog and overlay width to: " + overlayWidth);
        this.webPortal.Diagnostics.informationLocal("Setting dialog and overlay height to: " + overlayHeight);

        this.width(overlayWidth);
        this.height(overlayHeight);
    }
};


//@ sourceURL=Dialog.js
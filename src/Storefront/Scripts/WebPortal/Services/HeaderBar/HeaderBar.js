Microsoft.WebPortal.Services.HeaderBar = function (webPortal) {
    /// <summary>
    /// The portal header bar service.
    /// </summary>
    /// <param name="webPortal"></param>

    this.base.constructor.call(this, webPortal, "HeaderBar");

    this.headerBarContainerSelector = "#HeaderBarContainer";
    this.animation = this.webPortal.Configuration.HeaderBar.getDefaultAnimation();
    this.sections = ko.observableArray([]);

    // make the header bar toggle between show and hide!
    Microsoft.WebPortal.Utilities.Toggler.injectToggling(this, this.show, this.hide, false);
};

// extend the base portal service
$WebPortal.Helpers.inherit(Microsoft.WebPortal.Services.HeaderBar, Microsoft.WebPortal.Core.PortalService);

Microsoft.WebPortal.Services.HeaderBar.prototype._runService = function () {
    /// <summary>
    /// Runs the header bar service.
    /// </summary>

    ko.applyBindings(this, $(this.headerBarContainerSelector)[0]);

    var self = this;

    // initialize the sections when the header bar is shown
    for (var i in self.sections()) {
        self.sections()[i].initialize();
    }

    this.show();
};

Microsoft.WebPortal.Services.HeaderBar.prototype._stopService = function () {
    /// <summary>
    /// Stops the header bar service.
    /// </summary>

    var self = this;

    this.hide().always(function () {
        // destroy sections
        for (var i in self.sections()) {
            self.sections()[i].destroy();
        }

        // clean up HTML
        ko.cleanNode($(self.headerBarContainerSelector)[0]);
        $(self.headerBarContainerSelector).empty();
    });
};

Microsoft.WebPortal.Services.HeaderBar.prototype.addSection = function (newSection, position) {
    /// <summary>
    /// Adds a new header bar section. All sections must extend the Microsoft.WebPortal.Services.HeaderBarSection class.
    /// </summary>
    /// <param name="newSection">The new header bar section.</param>
    /// <param name="position">Where to add this section. Default is at the end of the sections list.</param>

    this.webPortal.Helpers.throwIfNotSet(newSection, "newSection", "Microsoft.WebPortal.Services.HeaderBar.addSection");

    if (position === null || position === undefined) {
        this.sections.push(newSection);
    } else {
        this.sections.splice(position, 0, newSection);
    }

    if (this.isRunning) {
        newSection.initialize();
    }
};

Microsoft.WebPortal.Services.HeaderBar.prototype.removeSection = function (id) {
    /// <summary>
    /// Removes a section from the header bar.
    /// </summary>
    /// <param name="id">The section id.</param>

    this.webPortal.Helpers.throwIfNotSet(id, "id", "Microsoft.WebPortal.Services.HeaderBar.removeSection");

    for (var i in this.sections()) {
        if (this.sections()[i].id() === id) {
            if (this.isRunning) {
                this.sections()[i].destroy();
            }

            this.sections.splice(i, 1);
            return;
        }
    }
};

Microsoft.WebPortal.Services.HeaderBar.prototype.show = function (showProgress) {
    /// <summary>
    /// Shows the header bar.
    /// </summary>
    /// <param name="showProgress">A JQuery deferred object to resolve when the show is complete.</param>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The header bar service is not running.");
        showProgress.reject();
        return;
    }

    // the header bar will steal the top section of the window from the content panel
    this.webPortal.ContentPanel.setMargin(this.webPortal.Configuration.HeaderBar.Height, null, null, null, this.animation.animationDuration);

    this.animation.show(this.webPortal.Settings.Ids.HeaderBar).always(function () {
        showProgress.resolve();
    });
};

Microsoft.WebPortal.Services.HeaderBar.prototype.hide = function (hideProgress) {
    /// <summary>
    /// Hides the header bar.
    /// </summary>
    /// <param name="hideProgress">A JQuery deferred object to resolve when the hide is complete.</param>

    if (!this.isRunning) {
        this.webPortal.Diagnostics.warningLocal("The header bar service is not running.");
        hideProgress.reject();
        return;
    }

    // time to return the stolen top section to the control panel
    this.webPortal.ContentPanel.setMargin(0, null, null, null, this.animation.animationDuration);

    this.animation.hide(this.webPortal.Settings.Ids.HeaderBar).always(function () {
        hideProgress.resolve();
    });
};

//@ sourceURL=HeaderBar.js
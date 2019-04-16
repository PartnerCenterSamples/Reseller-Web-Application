Microsoft.WebPortal.Services.HeaderBarSection = function (webPortal, id, template) {
    /// <summary>
    /// A base class that represents a section in the header bar. Extend this to implement your custom header bar sections.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="id">The section id.</param>
    /// <param name="template">The section template.</param>

    if (!webPortal) {
        throw new Error("Microsoft.WebPortal.Services.HeaderBarSection: Invalid webPortal instance.");
    }

    this.webPortal = webPortal;

    this.webPortal.Helpers.throwIfNotSet(id, "id", "Microsoft.WebPortal.Services.HeaderBarSection.constructor.");
    this.webPortal.Helpers.throwIfNotSet(template, "template", "Microsoft.WebPortal.Services.HeaderBarSection.constructor.");

    this.id = ko.observable(id);
    this.template = ko.observable(template);
    this.style = ko.observable("");
};

Microsoft.WebPortal.Services.HeaderBarSection.prototype.initialize = function () {
    /// <summary>
    /// This function is called to initialize the header bar section.
    /// </summary>
};

Microsoft.WebPortal.Services.HeaderBarSection.prototype.destroy = function () {
    /// <summary>
    /// This function is called to destroy the header bar section.
    /// </summary>
};

//@ sourceURL=HeaderBarSection.js
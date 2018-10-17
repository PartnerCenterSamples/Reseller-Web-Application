Microsoft.WebPortal.UpdateCompanyInformationPresenter = function (webPortal, feature, context) {
    /// <summary>
    /// Manages the offers experience. 
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature for which this presenter is created.</param>
    this.base.constructor.call(this, webPortal, feature, "Update company information", "/Template/");
};

// inherit BasePresenter
$WebPortal.Helpers.inherit(Microsoft.WebPortal.UpdateCompanyInformationPresenter, Microsoft.WebPortal.Core.TemplatePresenter);

Microsoft.WebPortal.UpdateCompanyInformationPresenter.prototype.onActivate = function () {
    /// <summary>
    /// Called when the presenter is activated.
    /// </summary>
};

Microsoft.WebPortal.UpdateCompanyInformationPresenter.prototype.onRender = function () {
    /// <summary>
    /// Called when the presenter is about to be rendered.
    /// </summary>
};

Microsoft.WebPortal.UpdateCompanyInformationPresenter.prototype.onShow = function () {
    /// <summary>
    /// Called when content is shown.
    /// </summary>
};

//@ sourceURL=UpdateCompanyInformationPresenter.js
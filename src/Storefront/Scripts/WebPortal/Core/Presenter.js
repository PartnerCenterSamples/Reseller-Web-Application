/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Core.Presenter = function (webPortal, feature, title) {
    /// <summary>
    /// The presenter class is the base class all presenter must ultimately extend. Presenters can be activated, deactivated and destroyed.
    /// When a presenter is activated, it owns the UI and it can use the different portal services to render its feature. A presenter can
    /// be deactivated when the user navigates to another feature. It is still kept in memory but no longer owns the UI. A deactivated
    /// presenter can maintain its state so that when it is reactivated it will show the expected state. When a presenter is no longer needed
    /// it will be detroyed in which it should clean up all its resources.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="feature">The feature this presenter owns.</param>
    /// <param name="title">The title of the presenter. This is useful for display purposes such as breadcrumb trails.</param>

    if (!webPortal) {
        throw new Error("Microsoft.WebPortal.Core.Presenter.Constrcutor: Invalid web portal instance.");
    }

    this.webPortal = webPortal;

    this.webPortal.Helpers.throwIfNotSet(feature, "feature", "Microsoft.WebPortal.Core.Presenter.Constructor");
    this.webPortal.Helpers.throwIfNotSet(title, "title", "Microsoft.WebPortal.Core.Presenter.Constructor");

    this.feature = feature;
    this.title = ko.observable(title);

    this.state = Microsoft.WebPortal.Core.Presenter.State.Initialized;
    this.webPortal.Diagnostics.information(this.title() + " presenter created");
}

Microsoft.WebPortal.Core.Presenter.prototype.activate = function (context) {
    /// <summary>
    /// Called when the presenter is activated.
    /// </summary>
    /// <param name="context">An optional parameter sent to the presenter.</param>

    this.state = Microsoft.WebPortal.Core.Presenter.State.ForeGround;

    this.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.FeatureActivated, {
        feature: this.feature,
        presenter: this,
        context: context
    });

    this.webPortal.Diagnostics.information(this.title() + " presenter activated");
}

Microsoft.WebPortal.Core.Presenter.prototype.deactivate = function (context) {
    /// <summary>
    /// Called when the presenter is no longer active.
    /// </summary>
    /// <param name="context">An optional parameter sent to the presenter.</param>

    this.state = Microsoft.WebPortal.Core.Presenter.State.Background;

    this.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.FeatureDeactivated, {
        Feature: this.feature,
        Presenter: this,
        Context: context
    });

    this.webPortal.Diagnostics.information(this.title() + " presenter deactivated");
}

Microsoft.WebPortal.Core.Presenter.prototype.destroy = function (context) {
    /// <summary>
    /// Called when the presenter is to be destroyed.
    /// </summary>
    /// <param name="context">An optional parameter sent to the presenter.</param>

    // deactivate the presenter first
    this.deactivate();

    this.state = Microsoft.WebPortal.Core.Presenter.State.Destroyed;

    this.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.FeatureDestroyed, {
        Feature: this.feature,
        Presenter: this,
        Context: context
    });

    this.webPortal.Diagnostics.information(this.title() + " presenter destroyed");
}

/*
    The different states a presenter can be in.
*/
Microsoft.WebPortal.Core.Presenter.State = {
    /*
        Presenter initialized.
    */
    Initialized: 0,

    /*
        Presenter is now active and owns the UI.
    */
    ForeGround: 1,

    /*
        Presenter is no longer active and does not own the UI anymore.
    */
    Background: 2,

    /*
        Presenter is destroyed.
    */
    Destroyed: 3
}

//@ sourceURL=Presenter.js
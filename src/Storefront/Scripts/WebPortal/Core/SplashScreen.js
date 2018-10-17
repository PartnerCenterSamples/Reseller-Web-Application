/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Core.ISplashScreen = function () {
    /// <summary>
    /// The interface any splash screen must implement.
    /// </summary>
}

Microsoft.WebPortal.Core.ISplashScreen.prototype.show = function (retryObject, retryFunction) {
    /// <summary>
    /// Shows the splash screen. Must return a JQuery deferred object.
    /// </summary>
    /// <param name="retryObject">The object that owns the retry function.</param>
    /// <param name="retryFunction">The function to call upon retrying to load the portal due to an error.</param>
}

Microsoft.WebPortal.Core.ISplashScreen.prototype.hide = function () {
    /// <summary>
    /// Hides the splash screen. Must return a JQuery deferred object.
    /// </summary>
}

Microsoft.WebPortal.Core.ISplashScreen.prototype.handleError = function (errorMessage) {
    /// <summary>
    /// Handles portal loading errors.
    /// </summary>
    /// <param name="errorMessage">The error message to display.</param>
}

Microsoft.WebPortal.Core.StandardSplashScreen = function (webPortal, animation) {
    /// <summary>
    /// The standard implementation of a splash screen.
    /// </summary>
    /// <param name="webPortal">The web portal instance.</param>
    /// <param name="animation">The assigned animation.</param>

    if (!webPortal) {
        throw new Error("Microsoft.WebPortal.Infrastructure.StandardSplashScreen.Constructor: Invalid web portal instance.");
    }

    this.webPortal = webPortal;
    this.message = ko.observable("");
    this.showLoadingIndicator = ko.observable(false);
    this.showRetryButton = ko.observable(false);
    this.animation = animation || new Microsoft.WebPortal.Utilities.Animation(Microsoft.WebPortal.Effects.Fade, 700);
    this.template = "StandardSplashScreen-template";
    this.onRetry = null;
    this.splashScreenElementSelector = ".StandardSplashScreen";
}

// implement the ISplashScreen interface
$WebPortal.Helpers.inherit(Microsoft.WebPortal.Core.StandardSplashScreen, Microsoft.WebPortal.Core.ISplashScreen);

Microsoft.WebPortal.Core.StandardSplashScreen.prototype.show = function (retryObject, retryFunction) {
    /// <summary>
    /// Shows the splash screen.
    /// </summary>
    /// <param name="retryObject">The object that owns the retry function.</param>
    /// <param name="retryFunction">The function to call upon retrying to load the portal due to an error.</param>

    this.onRetry = function () {
        retryFunction.call(retryObject);
    }

    this.message(this.webPortal.Resources.Strings.Loading);
    this.showRetryButton(false);
    this.showLoadingIndicator(true);
    
    return this.animation.show(this.splashScreenElementSelector);
}

Microsoft.WebPortal.Core.StandardSplashScreen.prototype.hide = function () {
    /// <summary>
    /// Hides the splash screen.
    /// </summary>

    return this.animation.hide(this.splashScreenElementSelector);
}

Microsoft.WebPortal.Core.StandardSplashScreen.prototype.handleError = function (errorMessage) {
    /// <summary>
    /// Handles portal loading errors.
    /// </summary>
    /// <param name="errorMessage">The error message to display.</param>

    this.message(errorMessage);
    this.showLoadingIndicator(false);
    this.showRetryButton(true);
}

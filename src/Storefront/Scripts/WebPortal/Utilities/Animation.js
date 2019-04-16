Microsoft.WebPortal.Utilities.BaseAnimation = function (duration) {
    /// <summary>
    /// A base class for animations. Relies on sub classes to implement actual animation behavior.
    /// All animations must extend from this base class.
    /// </summary>
    /// <param name="duration">The animation duration in milliseconds.</param>

    this.animationDuration = duration || $WebPortal.Configuration.DefaultAnimationDuration;

    Microsoft.WebPortal.Utilities.BaseAnimation.prototype.animate = function (elementSelector) {
        /// <summary>
        /// Performs an animation on a given JQuery HTML element selector.
        /// </summary>
        /// <param name="elementSelector">The JQuery selector for the elements to be animated.</param>
        /// <returns type="$.Deferred">A JQuery deferred object which is resolved when the animation is complete.</returns>

        var animationProgress = $.Deferred();

        if ($(elementSelector)[0] === undefined) {
            $WebPortal.Diagnostics.warning("Microsoft.WebPortal.Utilities.BaseAnimation.animate: elementSelector does not point to HTML element. Resolving.");

            // the HTML element is no longer there, complete the animation
            animationProgress.resolve();
        } else {
            // call the sub class implementation
            this.performAnimation(elementSelector, animationProgress);
        }

        return animationProgress;
    };

    Microsoft.WebPortal.Utilities.BaseAnimation.prototype.show = function (elementSelector) {
        /// <summary>
        /// Shows a given JQuery HTML element selector.
        /// </summary>
        /// <param name="elementSelector">The JQuery selector for the elements to be shown.</param>
        /// <returns type="$.Deferred">A JQuery deferred object which is resolved when the show animation is complete.</returns>

        var animationProgress = $.Deferred();

        if ($(elementSelector)[0] === undefined) {
            $WebPortal.Diagnostics.warning("Microsoft.WebPortal.Utilities.BaseAnimation.show: elementSelector does not point to HTML element. Resolving.");

            // the HTML element is no longer there, complete the animation
            animationProgress.resolve();
        } else {
            // call the sub class implementation
            this.performShowAnimation(elementSelector, animationProgress);
        }

        return animationProgress;
    };

    Microsoft.WebPortal.Utilities.BaseAnimation.prototype.hide = function (elementSelector) {
        /// <summary>
        /// Hides a given JQuery HTML element selector.
        /// </summary>
        /// <param name="elementSelector">The JQuery selector for the elements to be hidden.</param>
        /// <returns type="$.Deferred">A JQuery deferred object which is resolved when the hide animation is complete.</returns>

        var animationProgress = $.Deferred();

        if ($(elementSelector)[0] === undefined) {
            $WebPortal.Diagnostics.warning("Microsoft.WebPortal.Utilities.BaseAnimation.hide: elementSelector does not point to HTML element. Resolving.");

            // the HTML element is no longer there, complete the animation
            animationProgress.resolve();
        } else {
            // call the sub class implementation
            this.performHideAnimation(elementSelector, animationProgress);
        }

        return animationProgress;
    };
};

Microsoft.WebPortal.Utilities.Animation = function (effect, duration) {
    /// <summary>
    /// The standard implementation of base animation. Uses an effect to achieve the animation.
    /// </summary>
    /// <param name="effect">The animation effect. Pass one of Microsoft.WebPortal.Effects members.</param>
    /// <param name="duration">The animation duration in milliseconds.</param>

    Microsoft.WebPortal.Utilities.BaseAnimation.call(this, duration);
    this.effect = effect;

    this.performAnimation = function (elementSelector, animationProgress) {
        /// <summary>
        /// Called by the base class to perform the animation.
        /// </summary>
        /// <param name="elementSelector">The JQuery selector for the elements to be animated.</param>
        /// <param name="animationProgress">A JQuery deferred object which is resolved when the animation is complete.</returns>

        $(elementSelector).effect(this.effect.Name, this.effect.ShowOptions, this.animationDuration, function () {
            animationProgress.resolve();
        });
    };

    this.performShowAnimation = function (elementSelector, animationProgress) {
        /// <summary>
        /// Called by the base class to perform the show animation.
        /// </summary>
        /// <param name="elementSelector">The JQuery selector for the elements to be shownn.</param>
        /// <param name="animationProgress">A JQuery deferred object which is resolved when the show animation is complete.</returns>

        $(elementSelector).show(this.effect.Name, this.effect.ShowOptions, this.animationDuration, function () {
            animationProgress.resolve();
        });
    };

    this.performHideAnimation = function (elementSelector, animationProgress) {
        /// <summary>
        /// Called by the base class to perform the hide animation.
        /// </summary>
        /// <param name="elementSelector">The JQuery selector for the elements to be hidden.</param>
        /// <param name="animationProgress">A JQuery deferred object which is resolved when the hide animation is complete.</returns>

        $(elementSelector).hide(this.effect.Name, this.effect.HideOptions, this.animationDuration, function () {
            animationProgress.resolve();
        });
    };
};

// Animation inherits from BaseAnimation
$WebPortal.Helpers.inherit(Microsoft.WebPortal.Utilities.Animation, Microsoft.WebPortal.Utilities.BaseAnimation);

Microsoft.WebPortal.Utilities.VerticalSlideAnimation = function (duration) {
    /// <summary>
    /// Implements standard slide up and down animations. The standard JQuery slideup and slide down methods are
    /// different from the JQuery UI effects. The standard version smoothly moves the above or below HTML elements
    /// unlike the effect based version which causes a flickery jump and hence the implementation.
    /// </summary>
    /// <param name="duration">The animation duration in milliseconds.</param>

    Microsoft.WebPortal.Utilities.BaseAnimation.call(this, duration);

    this.performAnimation = function (elementSelector, animationProgress) {
        /// <summary>
        /// Called by the base class to perform the animation.
        /// </summary>
        /// <param name="elementSelector">The JQuery selector for the elements to be animated.</param>
        /// <param name="animationProgress">A JQuery deferred object which is resolved when the animation is complete.</returns>

        // this is not applicable here, do nothing
        animationProgress.resolve();
    };

    this.performShowAnimation = function (elementSelector, animationProgress) {
        /// <summary>
        /// Called by the base class to perform the show animation.
        /// </summary>
        /// <param name="elementSelector">The JQuery selector for the elements to be shownn.</param>
        /// <param name="animationProgress">A JQuery deferred object which is resolved when the show animation is complete.</returns>

        $(elementSelector).slideDown(this.animationDuration, function () {
            animationProgress.resolve();
        });
    };

    this.performHideAnimation = function (elementSelector, animationProgress) {
        /// <summary>
        /// Called by the base class to perform the hide animation.
        /// </summary>
        /// <param name="elementSelector">The JQuery selector for the elements to be hidden.</param>
        /// <param name="animationProgress">A JQuery deferred object which is resolved when the hide animation is complete.</returns>

        $(elementSelector).slideUp(this.animationDuration, function () {
            animationProgress.resolve();
        });
    };
};

// Vertical slide animation inherits from BaseAnimation
$WebPortal.Helpers.inherit(Microsoft.WebPortal.Utilities.VerticalSlideAnimation, Microsoft.WebPortal.Utilities.BaseAnimation);

Microsoft.WebPortal.Utilities.NoAnimation = function () {
    /// <summary>
    /// An animation that does nothing. Useful when we want to disable an animation dependent operation.
    /// </summary>

    this.base.constructor.call(this, Microsoft.WebPortal.Effects.Fade, 0);
};

// NoAnimation inherits from Animation
$WebPortal.Helpers.inherit(Microsoft.WebPortal.Utilities.NoAnimation, Microsoft.WebPortal.Utilities.Animation);

//@ sourceURL=Animation.js
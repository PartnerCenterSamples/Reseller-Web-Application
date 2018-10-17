/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Core.ContentPanel = function (webPortal) {
    /// <summary>
    /// The content panel class manages the main content panel to which presenters can render their content. It provides methods
    /// to render, clear and append content to the panel.
    /// </summary>

    if (!webPortal) {
        throw new Error("Microsoft.WebPortal.Core.ContentPanel: Invalid web portal instance.");
    }

    this.webPortal = webPortal;

    this.headerBarId = this.webPortal.Settings.Ids.HeaderBar;
    this.contentPanelId = this.webPortal.Settings.Ids.ContentPanel;
    this.controlPanelId = this.webPortal.Settings.Ids.ControlPanel;
    this.contentPanelInnerContentId = this.webPortal.Settings.Ids.ContentPanelContent;

    // a flag indicating whether the progress bar is shown or not
    this.isProgressShown = ko.observable(false);

    // the animation the content panel will use to show or hide its content
    this.animation = this.webPortal.Configuration.ContentPanel.getDefaultAnimation();

    // we will serializer rendering, clearing and appending requests to ensure integrity
    this.serializer = new Microsoft.WebPortal.Utilities.AsyncOperationSerializer();

    // the content panel will occupy the whole window by default, however, other components can cause it to shrink from the top, bottom
    // left or right. The margin variable will hold the current content margin relative to the window.
    this.margin = {
        left: 0,
        right: 0,
        top: 0,
        bottom: 0
    }

    // bind the content panel to our observable
    this.content = new ko.observable("");
    ko.applyBindings(this, $(this.contentPanelId)[0]);

    var self = this;

    this.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.OnWindowResizing, function () {
        /// <summary>
        /// The window has been resized. Resize the content panel to fit it.
        /// </summary>
        var verticalMargin = this.margin.top + this.margin.bottom;
        var horizontalMargin = this.margin.left + this.margin.right;

        // reset the content panel's height and width fit in the new window size      
        $(this.contentPanelId).height($(window).height() - verticalMargin);
        $(this.contentPanelId).width($(window).width() - horizontalMargin);
    }, this);

    this.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.OnWindowResized, function () {
        /// <summary>
        /// All components have resized to window size. Resize the content panel to fit the entire document.
        /// </summary>

        var verticalMargin = this.margin.top + this.margin.bottom;
        var horizontalMargin = this.margin.left + this.margin.right;

        // there is a chance that another component is too wide to fit in the new window, causing the document size to be bigger than
        // the window size, setting the content panel's dimensions above ensures that if that happens then it is not because of the old content
        // panel size, anyway, reset the content panel width to fit the new document
        var contentPanelWidth = $(document).width() - horizontalMargin;
        var contentPanelHeight = $(window).height() - verticalMargin;

        this.webPortal.Diagnostics.informationLocal("Setting Content panel height to: " + contentPanelHeight);
        this.webPortal.Diagnostics.informationLocal("Setting Content panel width to: " + contentPanelWidth);

        $(this.contentPanelId).height(contentPanelHeight);
        $(this.contentPanelId).width(contentPanelWidth);

        // ensure the content panel shows scroll bars if overflowed
        $(this.contentPanelId).css("overflow", "auto");

        this.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.ContentPanelResized, {
            top: this.margin.top,
            left: this.margin.left,
            height: contentPanelHeight,
            width: contentPanelWidth
        });
    }, this);
}

Microsoft.WebPortal.Core.ContentPanel.prototype.setAnimation = function (animation) {
    /// <summary>
    /// Sets the animation to use for rendering and clearing the content panel.
    /// </summary>
    /// <param name="animation">The animation to use to show and hide the content. Use a derivative of Microsoft.WebPortal.Utilities.BaseAnimation. If nothing is passed,
    /// Microsoft.WebPortal.Configuration.ContentPanel.DefaultAnimation will be used.</param>

    this.animation = animation || this.webPortal.Configuration.ContentPanel.getDefaultAnimation();
}

Microsoft.WebPortal.Core.ContentPanel.prototype.resetAnimation = function () {
    /// <summary>
    /// Restores the default animation.
    /// </summary>

    this.animation = this.webPortal.Configuration.ContentPanel.getDefaultAnimation();
}

Microsoft.WebPortal.Core.ContentPanel.prototype.render = function (content, listener) {
    /// <summary>
    /// Renders content into the panel.
    /// </summary>
    /// <param name="content">The html to render.</param>
    /// <param name="listener">An optional JQuery deferred object that will get events on rendering progress.
    /// A notification (progress function on your deferred) will be sent when the content is rendered but not yet shown.
    /// The done function will be called when the content is fully shown.</param>

    this.serializer.queue(this, function (taskProgress) {
        content = content || "";
        var self = this;
        var clearingListener = $.Deferred();

        clearingListener.always(function () {
            // clearing is complete, set the content
            self.content(content);

            if (listener) {
                // notify the listener that the content is rendered but not yet shown
                listener.notify();
            }

            // show the new content
            self.animation.show(self.contentPanelInnerContentId).always(function () {
                // restore scrolling behavior
                $(self.contentPanelInnerContentId + ", html, body").css("overflow-x", "auto");
                $(self.contentPanelInnerContentId + ", html, body").css("overflow-y", "auto");

                if (listener) {
                    // notify the listener that the content has been shown
                    listener.resolve();
                }

                if (taskProgress) {
                    // signal task completion
                    taskProgress.resolve();
                }
            });

            // hide ugly scroll bars that may show as we animate the content panel
            $(self.contentPanelInnerContentId + ", html, body").css("overflow-x", "hidden");
            $(self.contentPanelInnerContentId + ", html, body").css("overflow-y", "hidden");
        });

        // clear the content panel and listen to its outcome
        this._clear(null, clearingListener);
    });
}

Microsoft.WebPortal.Core.ContentPanel.prototype.clear = function (listener) {
    /// <summary>
    /// Clears content from the panel. This does not hide the progress bar. You need to call hide progress bar for that.
    /// </summary>
    /// <param name="listener">An optional JQuery deferred object that will get events on clearing progress. If clearing is complete the done method will be called. If
    /// the content panel could not perform the clearing since another operation is still in progress, the fail method will be called.</param>

    this.serializer.queue(this, this._clear, listener);
}

Microsoft.WebPortal.Core.ContentPanel.prototype.append = function (content) {
    /// <summary>
    /// Appends content into the panel.
    /// </summary>
    /// <param name="content">The content</param>

    this.serializer.queue(this, function (taskProgress) {
        $(this.contentPanelInnerContentId).append(content);
        taskProgress.resolve();
    });
}

Microsoft.WebPortal.Core.ContentPanel.prototype.showProgress = function () {
    /// <summary>
    /// Shows a progress indicator on at the center of the content panel.
    /// </summary>

    this.isProgressShown(true);
}

Microsoft.WebPortal.Core.ContentPanel.prototype.hideProgress = function () {
    /// <summary>
    /// Hides the progress indicator.
    /// </summary>
    this.isProgressShown(false);
}

Microsoft.WebPortal.Core.ContentPanel.prototype.setMargin = function (top, bottom, left, right, animationDuration, originator) {
    /// <summary>
    /// Changes the content panel's margin giving other UI elements the needed space. This is useful if a component wants to occupy the left side of the
    /// screen for example to display a navigation menu or so.
    /// </summary>
    /// <param name="top">The top margin. Pass null to keep the existing top margin.</param>
    /// <param name="bottom">The bottom margin. Pass null to keep the existing bottom margin.</param>
    /// <param name="left">The left margin. Pass null to keep the existing left margin.</param>
    /// <param name="right">The right margin. Pass null to keep the existing right margin.</param>
    /// <param name="animationDuration">The content panel resize animation duration in milliseconds.</param>
    /// <param name="originator">The component that caused the margin change. Useful in handling content panel resize events.Optional.</param>

    var heightDelta = 0, widthDelta = 0;

    if (top || top === 0) {
        heightDelta = this.margin.top - top;
        this.margin.top = top;
    }

    if (bottom || bottom === 0) {
        heightDelta += this.margin.bottom - bottom;
        this.margin.bottom = bottom;
    }

    if (left || left === 0) {
        widthDelta = this.margin.left - left;
        this.margin.left = left;
    }

    if (right || right === 0) {
        widthDelta = this.margin.right - right;
        this.margin.right = right;
    }

    var contentPanelHeight = $(this.contentPanelId).height();
    var contentPanelWidth = $(this.contentPanelId).width();

    this.webPortal.EventSystem.broadcast(Microsoft.WebPortal.Event.ContentPanelResized, {
        top: this.margin.top,
        left: this.margin.left,
        height: contentPanelHeight + heightDelta,
        width: contentPanelWidth + widthDelta,
        animating: true,
        originator: originator
    });

    $(this.contentPanelId).animate({
        left: this.margin.left + "px",
        right: this.margin.right + "px",
        bottom: this.margin.bottom + "px",
        top: this.margin.top + "px",
        width: "+=" + widthDelta,
        height: "+=" + heightDelta,
        minHeight: this.webPortal.Configuration.MinimumResolution.Height - this.margin.top - this.margin.bottom,
        minWidth: this.webPortal.Configuration.MinimumResolution.Width - this.margin.left - this.margin.right
    }, animationDuration || this.webPortal.Configuration.DefaultAnimationDuration);
}

Microsoft.WebPortal.Core.ContentPanel.prototype._clear = function (taskProgress, listener) {
    /// <summary>
    /// Clears the content panel.
    /// </summary>
    /// <param name="taskProgress">A JQuery deferred object which must be resolved to signal task completion.</param>
    /// <param name="listener">A JQuery deferred object which will be resolved when clearing is complete.</param>

    if (this.content() && this.content().length > 0) {
        var self = this;

        this.animation.hide(this.contentPanelInnerContentId).always(function () {
            // clear content when the animation is done
            self.content("");

            // restore scrolling behavior
            $(self.contentPanelInnerContentId + ", html, body").css("overflow-x", "auto");
            $(self.contentPanelInnerContentId + ", html, body").css("overflow-y", "auto");

            if (listener) {
                // notify the listener
                listener.resolve();
            }

            if (taskProgress) {
                // signal task completion
                taskProgress.resolve();
            }
        });

        // hide ugly scroll bars that may show as we animate the content panel
        $(this.contentPanelInnerContentId + ", html, body").css("overflow-x", "hidden");
        $(this.contentPanelInnerContentId + ", html, body").css("overflow-y", "hidden");
    } else {
        if (listener) {
            // nothing to clear, notify the listener we are done
            listener.resolve();
        }

        if (taskProgress) {
            // signal task completion
            taskProgress.resolve();
        }
    }
}

//@ sourceURL=ContentPanel.js
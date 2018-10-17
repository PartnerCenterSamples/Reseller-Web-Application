/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Views.List.InfiniteScrollingRenderer = function (list) {
    /// <summary>
    /// Renders an infinite scrolling list. As the user reaches the bottom of the list, new rows will be requested from the listener.
    /// </summary>
    /// <param name="list">The list to render.</param>

    var self = this;
    self.base.constructor.call(self, list, "infiniteScrollList-template");

    self.rowsChangeSubscription = self.rows.subscribe(function (changes) {
        // listen to the rows array changes
        window.setTimeout(function (self) {
            if (list.isShown()) {
                // if rows were deleted for instance, we would like to ensure that the remaining space is filled with the next batch of rows
                // in order to guarantee a scroll bar appearance and avoid not being able to see the next rows
                self.ensureOccupancy();
            }
        }, 0, self);
    }, self, "arrayChange");

    self.onListRenderingFinished = function (elements, data) {
        // self function will be called after rendering the list is done, this is need to ensure that we fill up the list real estate
        // with rows to avoid the lack of scrollbar problem
        if (this.foreach[this.foreach.length - 1] === data) {
            // ahuh, this is the last row rendered!
            if (list.isShown()) {
                self.ensureOccupancy();
            }
        }
    };
}

// inherit ListRenderer
$WebPortal.Helpers.inherit(Microsoft.WebPortal.Views.List.InfiniteScrollingRenderer, Microsoft.WebPortal.Views.List.ListRenderer);

Microsoft.WebPortal.Views.List.InfiniteScrollingRenderer.prototype.onListScrolled = function (self) {
    /// <summary>
    /// Called when the list body is scrolled.
    /// </summary>
    /// <param name="self">The renderer instance.</param>

    var listBodySelector = self.list.elementSelector + self.list.webPortal.Settings.List.ListBodySelector;
    var verticalScroll = $(listBodySelector).scrollTop();

    if (self.isComplete() || self.isFetchingRows) {
        return;
    }

    if (verticalScroll != self.verticalScroll) {
        var totalHeight = $(listBodySelector)[0].scrollHeight;
        var currentScroll = verticalScroll + $(listBodySelector).height();
        self.verticalScroll = verticalScroll;

        if (currentScroll >= totalHeight * self.list.webPortal.Configuration.List.Sensitivity) {
            // get more items only if the scroll has nearly reached the end and we know that there are more
            // items and we are not currently in the process of fetching more items
            self.requestMoreRows(self.rows().length, self.pageSize());
        }
    }
}

Microsoft.WebPortal.Views.List.InfiniteScrollingRenderer.prototype.ensureOccupancy = function () {
    /// <summary>
    /// Called to ensure that the list body fills it real estate with rows.
    /// </summary>

    if (this.isComplete() || this.isFetchingRows) {
        return;
    }

    var listBodySelector = this.list.elementSelector + this.list.webPortal.Settings.List.ListBodySelector;
    var totalRowNumberNeeded = this.pageSize();

    var fullCapacityHeight = $(listBodySelector).height();
    var currentCapacityHeight = $(listBodySelector + " > table").height();

    if (!fullCapacityHeight) {
        $WebPortal.Diagnostics.warning("Microsoft.WebPortal.Views.List.InfiniteScrollingRenderer.ensureOccupancy: full capacity height is zero. Fetching a whole page.");
    } else if (currentCapacityHeight <= fullCapacityHeight) {
        // we have a problem, let's fetch more entities to cause the scrollbar to be enabled,
        // if the currentCapacityHeight is zero, this means that the list is not being shown and we should not worry about paging it
        var percentageOccupied = currentCapacityHeight / fullCapacityHeight;

        if (percentageOccupied) {
            // compute the number of total entities needed to cause a scroll
            totalRowNumberNeeded = Math.ceil(this.rows().length / percentageOccupied);
            // compute the number of extra entities we need to download
            totalRowNumberNeeded -= this.rows().length;
        }
    } else {
        // we are ok
        return;
    }

    // fetch the needed number of entities
    this.requestMoreRows(this.rows().length, totalRowNumberNeeded);
}

Microsoft.WebPortal.Views.List.InfiniteScrollingRenderer.prototype.destroy = function () {
    /// <summary>
    /// Destorys the renderer.
    /// </summary>

    // stop listening to row changes
    if (this.rowsChangeSubscription) {
        this.rowsChangeSubscription.dispose();
        this.rowsChangeSubscription = null;
    }
}

//@ sourceURL=InfiniteScrollingRenderer.js
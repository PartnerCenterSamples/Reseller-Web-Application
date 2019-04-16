Microsoft.WebPortal.Views.List.ListRenderer = function (list, bodyTemplate, footerTemplate) {
    /// <summary>
    /// Renders the list in a particular style.
    /// </summary>
    /// <param name="list">The list to render. Required.</param>
    /// <param name="bodyTemplate">The Knockout HTML template used to render the list rows. Required.</param>
    /// <param name="footerTemplate">The Knockout HTML template used to render the list footer. Optional.</param>

    $WebPortal.Helpers.throwIfNotSet(list, "list", "Microsoft.WebPortal.Views.List.IListRenderer");
    $WebPortal.Helpers.throwIfNotSet(bodyTemplate, "bodyTemplate", "Microsoft.WebPortal.Views.List.IListRenderer");

    this.list = list;
    this.bodyTemplate = bodyTemplate;
    this.footerTemplate = footerTemplate;

    this.rows = list.rows;
    this.pageSize = list.pageSize;
    this.isComplete = list.isComplete;
};

Microsoft.WebPortal.Views.List.ListRenderer.prototype.performSelectAll = function (allSelected) {
    /// <summary>
    /// Called to perform select all logic. Default behavior is to select all the rows in the list.
    /// </summary>
    /// <param name="allSelected">True to select all, false to deselect all.</param>

    for (var i = 0; i < this.rows().length; ++i) {
        if (this.rows()[i].isSelected) {
            this.rows()[i].isSelected(allSelected);
        }
    }
};

Microsoft.WebPortal.Views.List.ListRenderer.prototype.onListScrolled = function (self) {
    /// <summary>
    /// Called when the list body is scrolled.
    /// </summary>
    /// <param name="self">The renderer instance.</param>
};

Microsoft.WebPortal.Views.List.ListRenderer.prototype.ensureOccupancy = function () {
    /// <summary>
    /// Called to ensure that the list body fills it real estate with rows.
    /// </summary>
};

Microsoft.WebPortal.Views.List.ListRenderer.prototype.onDataRequestStarted = function (context) {
    /// <summary>
    /// Called before a request for additional data is made. Implementations can implement this to add pre-fetch behavior.
    /// </summary>
    /// <param name="context">An optional context parameter.</param>
};

Microsoft.WebPortal.Views.List.ListRenderer.prototype.onDataArrived = function (newRows, context) {
    /// <summary>
    /// Called when the additional data is received. Implementations can implement this to add their own custom behavior.
    /// </summary>
    /// <param name="newRows">The new rows received.</param>
    /// <param name="context">An optional context parameter.</param>
};

Microsoft.WebPortal.Views.List.ListRenderer.prototype.onDataRequestFinished = function (context) {
    /// <summary>
    /// Called when requesting data is finished whether in success or in failure. Implementations can implement this to add their clean up logic.
    /// </summary>
    /// <param name="context">An optional context parameter.</param>
};

Microsoft.WebPortal.Views.List.ListRenderer.prototype.requestMoreRows = function (index, count, context) {
    /// <summary>
    /// Requests more rows as needed by the list.
    /// </summary>
    /// <param name="index">The new rows starting index.</param>
    /// <param name="count">The number of rows needed.</param>
    /// <param name="context">An optional context parameter.</param>
    var self = this;

    // call the data request start hook
    self.onDataRequestStarted(context);

    // request additional data
    if (self.list.listener && self.list.listener.onMoreDataNeeded) {
        self.isFetchingRows = true;
        var needMoreDataResult = $.Deferred();
        this.list.listener.onMoreDataNeeded(needMoreDataResult, index, count);

        needMoreDataResult.done(function (newRows) {
            // call the data arrived hook
            self.onDataArrived(context);

            // append the new rows
            self.list.append(newRows);
        }).fail(function () {
            // no more data is available from the source, we have all the data!
            self.isComplete(true);
        }).always(function () {
            self.isFetchingRows = false;

            // call the data request finished hook
            self.onDataRequestFinished(context);
        });
    } else {
        // the listener is not interested in giving more data, consider the list to have all the data
        window.setTimeout(function () {
            self.isComplete(true);
            self.onDataRequestFinished(context);
        }, 0);
    }
};

Microsoft.WebPortal.Views.List.ListRenderer.prototype.destroy = function () {
    /// <summary>
    /// Destroys the renderer.
    /// </summary>
};

//@ sourceURL=ListRenderer.js
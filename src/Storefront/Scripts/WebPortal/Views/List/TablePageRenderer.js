/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Views.List.TablePageRenderer = function (list) {
    /// <summary>
    /// Renders a paged list. The footer will contain links used to navigate to pages.
    /// </summary>
    /// <param name="list">The list to render.</param>

    this.base.constructor.call(this, list, "tablePageListBody-template", "tablePageListFooter-template");

    // the current page index will start at zero
    this.pageIndex = ko.observable(0);

    // this will hold the current page rows as we do not want to render the entire list rows
    this.rowsToRender = ko.observable();

    // controls the enabled state of the next page link
    this.disableNext = ko.observable(false);

    // computes the index of the last page
    this.lastPageIndex = ko.computed(function () {
        return Math.ceil(this.rows().length / this.pageSize()) - 1;
    }, this);

    // determines whether the next link is rendered or not
    this.renderNext = ko.computed(function () {
        // render only if the data is not complete or we are not in the last page index in case it was complete
        return !this.isComplete() || this.pageIndex() < this.lastPageIndex();
    }, this);

    // determines if a row is in the current page or not
    this.inCurrentPage = function (index) {
        return index >= this.pageIndex() * this.pageSize() && index < (this.pageIndex() + 1) * this.pageSize();
    }

    this.pageSizeSubscription = this.pageSize.subscribe(function () {
        // page size changed, go to first page
        this.goToPage(0);

        // clear select all
        this.list._silentClearSelectAll();
    }, this);

    this.pageIndexSubscription = this.pageIndex.subscribe(function () {
        // page index changed, clear select all as only the previous page rows were selected (if so)
        this.list._silentClearSelectAll();
    }, this);

    this.rowsSubscription = this.rows.subscribe(function () {
        // refresh the page any time the rows update
        this.refreshPage();
    }, this);
    
    // render the first page
    this.refreshPage();
}

// inherit ListRenderer
$WebPortal.Helpers.inherit(Microsoft.WebPortal.Views.List.TablePageRenderer, Microsoft.WebPortal.Views.List.ListRenderer);

Microsoft.WebPortal.Views.List.TablePageRenderer.prototype.onDataRequestStarted = function () {
    /// <summary>
    /// Called before a request for additional data is made.
    /// </summary>

    // disable the next button
    this.disableNext(true);
}

Microsoft.WebPortal.Views.List.TablePageRenderer.prototype.onDataArrived = function () {
    /// <summary>
    /// Called when the additional data is received.
    /// </summary>

    // enable the next button
    this.disableNext(false);

    // we do not want the page to unnecessarily refresh when the rows are added as we are going to manually trigger it by moving to the new page
    this.supressPageRefresh = true;
}

Microsoft.WebPortal.Views.List.TablePageRenderer.prototype.onDataRequestFinished = function (pageIndex) {
    /// <summary>
    /// Called when requesting data is finished whether in success or in failure.
    /// </summary>
    /// <param name="pageIndex">The page index.</param>

    // navigate to the page index and reenable back page refresh
    this.supressPageRefresh = false;
    this.goToPage(pageIndex);
}

Microsoft.WebPortal.Views.List.TablePageRenderer.prototype.performSelectAll = function (allSelected) {
    /// <summary>
    /// Called to perform select all logic. Override default behavior and only select the current page rows.
    /// </summary>
    /// <param name="allSelected">True to select all, false to deselect all.</param>

    var startIndex = this.pageIndex() * this.pageSize();

    // we normally want to loop through the page size but there is a possibility that the total rows we have are less, account for this case
    var rowCountToSelect = Math.min(this.pageSize(), this.rows().length - (this.pageIndex() * this.pageSize() ));

    for (var i = 0; i < rowCountToSelect ; ++i) {
        if (this.rows()[startIndex + i].isSelected) {
            this.rows()[startIndex + i].isSelected(allSelected);
        }
    }
}

Microsoft.WebPortal.Views.List.TablePageRenderer.prototype.goToPage = function (pageIndex) {
    /// <summary>
    /// Navigates to a list page.
    /// </summary>
    /// <param name="pageIndex">The page index to go to.</param>

    pageIndex = Math.max(0, pageIndex);
    this.disableNext(false);

    if (this.isComplete()) {
        // if we have all the data, ensure we do not run past the last page index
        pageIndex = Math.min(this.lastPageIndex(), pageIndex);
    } else {
        // make sure we do not run too far ahead of the last page index
        pageIndex = Math.min(this.lastPageIndex() + 1, pageIndex);

        if (pageIndex > this.lastPageIndex()) {
            // we do not have rows for this page, get them
            this.requestMoreRows(pageIndex * this.pageSize(), this.pageSize(), pageIndex);
            return;
        } else if ((pageIndex + 1) * this.pageSize() > this.rows().length) {
            // we have partial data for this row, get the rest to fill up the page
            this.requestMoreRows(this.rows().length, (pageIndex + 1) * this.pageSize() - this.rows().length, pageIndex);
            return;
        }
    }

    // if we reach here, then we have all the rows for the requested page, update the page index and refresh the list page to show the
    // new rows
    this.pageIndex(pageIndex);
    this.refreshPage();
}

Microsoft.WebPortal.Views.List.TablePageRenderer.prototype.refreshPage = function () {
    /// <summary>
    /// Refreshes the rows that will get rendered as a result of them being in the current page.
    /// </summary>

    if (this.supressPageRefresh === true) {
        return;
    }

    var startIndex = this.pageIndex() * this.pageSize();
    this.rowsToRender(this.rows.slice(startIndex, startIndex + this.pageSize()));
}

Microsoft.WebPortal.Views.List.TablePageRenderer.prototype.destroy = function () {
    /// <summary>
    /// Destorys the renderer.
    /// </summary>

    // stop listening to page size changes
    if (this.pageSizeSubscription) {
        this.pageSizeSubscription.dispose();
        this.pageSizeSubscription = null;
    }

    // stop listening to page index changes
    if (this.pageIndexSubscription) {
        this.pageIndexSubscription.dispose();
        this.pageIndexSubscription = null;
    }

    // stop listening to row changes
    if (this.rowsSubscription) {
        this.rowsSubscription.dispose();
        this.rowsSubscription = null;
    }
}

//@ sourceURL=TablePageRenderer.js
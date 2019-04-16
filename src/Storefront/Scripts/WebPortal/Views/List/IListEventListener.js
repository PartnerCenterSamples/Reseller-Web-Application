Microsoft.WebPortal.Views.List.IListEventListener = function () {
    /// <summary>
    /// Specifies the contract a list event listener must implement.
    /// </summary>
};

Microsoft.WebPortal.Views.List.IListEventListener.prototype.onMoreDataNeeded = function (taskProgress, index, count) {
    /// <summary>
    /// Handles getting more data for the list as it needs it.
    /// </summary>
    /// <param name="taskProgress">A JQuery deferred object which should be resolved with the data or rejected if the list has the entire data set.</param>
    /// <param name="index">The starting index of the data.</param>
    /// <param name="count">The number of rows needed.</param>
};

Microsoft.WebPortal.Views.List.IListEventListener.prototype.onSelectionChanged = function (selectedRows) {
    /// <summary>
    /// Handles list selection changes.
    /// </summary>
    /// <param name="selectedRows">An array of selected rows.</param>
};

Microsoft.WebPortal.Views.List.IListEventListener.prototype.onSortChanged = function (sortColumn, sortDirection) {
    /// <summary>
    /// Handles list sort changes.
    /// </summary>
    /// <param name="sortColumn">The new sort column.</param>
    /// <param name="sortDirection">The new sort direction.</param>
};

Microsoft.WebPortal.Views.List.IListEventListener.prototype.onCellClicked = function (column, row) {
    /// <summary>
    /// Handles cell clicks.
    /// </summary>
    /// <param name="column">The cell's column.</param>
    /// <param name="row">The rows which had the cell that was clicked.</param>
};

//@ sourceURL=ListEventListener.js
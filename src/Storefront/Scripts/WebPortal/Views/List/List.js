Microsoft.WebPortal.Views.List = function (webPortal, elementSelector, listener, isShown, animation) {
    /// <summary>
    /// A view that renders a list of items. You can specify columns, sorting, clicking, row selection options as well as list rendering styles
    /// such as paged or infinite scrolling. The list will communicate the events back to the associated listener which can handle the events.
    /// </summary>
    /// <param name="webPortal">The web portal instance</param>
    /// <param name="elementSelector">The JQuery selector for the HTML element this view will own.</param>
    /// <param name="listener">An object which receives list events. Pass an object that implements: Microsoft.WebPortal.Views.List.IListEventListener. Passing null
    /// will work but will cause no list events to be fired.</param>
    /// <param name="isShown">The initial show state. Optional. Default is false.</param>
    /// <param name="animation">Optional animation to use for showing and hiding the view.</param>

    this.base.constructor.call(this, webPortal, elementSelector, isShown, null, animation);

    this.listener = listener;

    // the list view template
    this.template = this.webPortal.Settings.List.Template;

    // the list rows
    this.rows = ko.observableArray([]);

    // the list columns
    this.columns = ko.observableArray([]);

    // defines the number of rows that constitute a page
    this.pageSize = ko.observable(this.webPortal.Configuration.List.PageSize);

    this.pageSize.subscribe(function () {
        // since changing the page size may show or hide the scrollbar, we need to recheck it and align the header cells appropriately
        window.setTimeout(function (self) {
            self._updateScrollBarStatus();
        }, 0, this);
    }, this);

    this.showHeader = ko.observable(true);

    // indicates whether the list has all the rows or it still needs to notify the listener to get more rows when needed (i.e. when a new page is clicked or the user scrolls down in an infinite scrolling list)
    this.isComplete = ko.observable(false);

    // if enabled, a thin colored line will be displayed to the left of each row indicating its status
    this.isStatusBarEnabled = ko.observable(false);

    // the template and its view model to show when the list is empty
    this.emptyListTemplate = ko.observable(this.webPortal.Settings.List.DefaultEmptyListTemplate);
    this.emptyListViewModel = ko.observable(this.webPortal.Resources.Strings.EmptyListMessage);

    // a flag that indicates if the list if empty or not
    this.isEmpty = ko.computed(function () {
        return this.rows().length <= 0;
    }, this);

    // configure list capabilities
    this._configureSelection();
    this._configureSorting();
    this._configureClicking();

    // the renderer used to render the list, default it to infinite scrolling
    this.renderer = ko.observable(new Microsoft.WebPortal.Views.List.InfiniteScrollingRenderer(this));

    // internal flag which specifies if a vertical scrollbar is displayed or not, this is used to align the header cells correctly
    this.scrollBarVisible = ko.observable(false);

    this.drawTopBorder = function (index) {
        /// <summary>
        /// Determines whether the row at the given index is selected and the entity on top of it is NOT selected. This is used
        /// to render the top border line only is the previous element is not selected in order to avoid double border lines (ugly).
        /// </summary>
        /// <param name="index">The entity's index.</param>
        /// <returns type="Boolean">true or false.</returns>
        return this.isSelectable() && this.rows()[index].isSelected() && (index === 0 || !this.rows()[index - 1].isSelected());
    };

    this.rows.subscribe(function (changes) {
        window.setTimeout(function (self) {
            if (self.isShown()) {
                self._resizeBody(Microsoft.WebPortal.Event.OnWindowResizing);
                self._resizeBody();

                // since row updates may show or hide the scrollbar, we need to recheck it and align the header cells appropriately
                self._updateScrollBarStatus();
            }
        }, 0, this);
    }, this, "arrayChange");
};

// extend the base view
$WebPortal.Helpers.inherit(Microsoft.WebPortal.Views.List, Microsoft.WebPortal.Core.View);

Microsoft.WebPortal.Views.List.prototype.set = function (rows) {
    /// <summary>
    /// Sets the list rows. This will remove any existing rows.
    /// </summary>
    /// <param name="rows">An array of objects.</param>

    // deselect all selected rows
    this._silentDeselectAll();

    // clear the selected row
    this.selectedRows([]);

    // set the rows
    this._mutateToSelectable(rows);
    this.rows(rows);

    // send a selection change update
    if (!this.supressSelectEvent && this.listener && this.listener.onSelectionChanged) {
        this.listener.onSelectionChanged(this.selectedRows());
    }
};

Microsoft.WebPortal.Views.List.prototype.append = function (rows) {
    /// <summary>
    /// Appends rows to the list.
    /// </summary>
    /// <param name="rows">An array of objects.</param>

    this._mutateToSelectable(rows);
    this.rows.push.apply(this.rows, rows);
};

Microsoft.WebPortal.Views.List.prototype.remove = function (row) {
    /// <summary>
    /// Appends rows to the list.
    /// </summary>
    /// <param name="rows">An array of objects.</param>

    for (var i in this.rows()) {
        if (this.rows()[i] === row) {
            this.rows.splice(i, 1);
            break;
        }
    }
};

Microsoft.WebPortal.Views.List.prototype.clear = function () {
    /// <summary>
    /// Clears all list rows.
    /// </summary>

    // deselect all selected rows
    this._silentDeselectAll();

    // clear the selected rows
    this.selectedRows([]);

    // clear the rows
    this.rows([]);

    // send a selection change update
    if (!this.supressSelectEvent && this.listener && this.listener.onSelectionChanged) {
        this.listener.onSelectionChanged(this.selectedRows());
    }
};

Microsoft.WebPortal.Views.List.prototype.setColumns = function (columns) {
    /// <summary>
    /// Sets the list columns.
    /// </summary>
    /// <param name="columns">An array of Microsoft.WebPortal.Views.List.Column objects.</param>

    this.webPortal.Helpers.throwIfNotSet(columns, "columns", "Microsoft.WebPortal.Views.List.setColumns");
    this.columns(columns);
};

Microsoft.WebPortal.Views.List.prototype.setComplete = function (isComplete) {
    /// <summary>
    /// Tells the list that it has all the data and that no further data requests is required.
    /// </summary>
    /// <param name="isComplete">True if the list has all the data, false if not.</param>

    this.isComplete(isComplete === true);
};

Microsoft.WebPortal.Views.List.prototype.setPageSize = function (pageSize) {
    /// <summary>
    /// Sets the number of rows that represent a list page. This will affect how many rows are displayed in a paged list and how many rows are fetched when the user
    /// reaches the end of an infinite scrolling list.
    /// </summary>
    /// <param name="pageSize">The new page size.</param>

    this.pageSize(pageSize || this.webPortal.Configuration.List.PageSize);
};

Microsoft.WebPortal.Views.List.prototype.setSelectionMode = function (selectionMode) {
    /// <summary>
    /// Sets the list selection mode. By default the list is non selectable, you can set it to single or multi-select.
    /// </summary>
    /// <param name="selectionMode">Pass a Microsoft.WebPortal.Views.List.SelectionMode enum value. Default is none.</param>

    selectionMode = selectionMode || Microsoft.WebPortal.Views.List.SelectionMode.None;

    if (this.selectMode() === selectionMode) {
        return;
    }

    if (selectionMode !== Microsoft.WebPortal.Views.List.SelectionMode.Multiple) {
        // deselect all selected rows
        this._silentDeselectAll();

        // clear the selected rows
        this.selectedRows([]);

        // send a selection change update
        if (!this.supressSelectEvent && this.listener && this.listener.onSelectionChanged) {
            this.listener.onSelectionChanged(this.selectedRows());
        }
    }

    this.selectMode(selectionMode);
};

Microsoft.WebPortal.Views.List.prototype.setSorting = function (sortField, sortDirection, refreshData) {
    /// <summary>
    /// Sets the list sort information.
    /// </summary>
    /// <param name="sortField">The field of the column to sort the list by.</param>
    /// <param name="sortDirection">The sort direction. Specify a value of: Microsoft.WebPortal.Views.List.SortDirection. Default is ascending.</param>
    /// <param name="refreshData">Specify true to cause a list sort event and refresh the list data. Otherwise the list sort information will change silently without a refresh.</param>

    this.webPortal.Helpers.throwIfNotSet(sortField, "sortField", "Microsoft.WebPortal.Views.List.setSorting");
    sortDirection = sortDirection || Microsoft.WebPortal.Views.List.SortDirection.Ascending;

    var sortColumn = null;

    for (var i in this.columns()) {
        if (this.columns()[i].field() === sortField) {
            sortColumn = this.columns()[i];
            break;
        }
    }

    if (sortColumn) {
        // set the new field as the sort field
        this.sortField(sortColumn.field());
        this.sortDirection(sortDirection);

        if (refreshData === true && this.listener && this.listener.onSortChanged) {
            // notify the listener if the caller wanted to
            this.listener.onSortChanged(this.sortField(), this.sortDirection());
        }
    } else {
        this.webPortal.Diagnostics.warning("Microsoft.WebPortal.Views.List.setSorting: " + sortField + " field not found.");
    }
};

Microsoft.WebPortal.Views.List.prototype.setRenderer = function (rendererClass) {
    /// <summary>
    /// Sets the list rendering mode. Pass in: Microsoft.WebPortal.Views.List.InfiniteScrollingRenderer to display an infinite scrolling list,
    /// or : Microsoft.WebPortal.Views.List.TablePageRenderer to display a paged table list. Default is infinite scrolling list. You
    /// can implement a custom renderer by extending: Microsoft.WebPortal.Views.List.ListRenderer.
    /// </summary>
    /// <param name="rendererClass">The renderer class.</param>

    if (this.renderer()) {
        // destory the existing renderer
        this.renderer().destroy();
    }

    this.renderer(rendererClass ? new rendererClass(this) : new Microsoft.WebPortal.Views.List.InfiniteScrollingRenderer(this));

    // resize the list to correctly fit the new renderer
    this._resizeBody(Microsoft.WebPortal.Event.OnWindowResizing);
    this._resizeBody();
};

Microsoft.WebPortal.Views.List.prototype.enableStatusBar = function (isEnabled) {
    /// <summary>
    /// Enables or disables the status bar. If enabled, each row will display a status bar with a color that indicates its status. You rows must have a stateColor property
    /// which specifies the color to display. Otherwise, nothing will be displayed.
    /// </summary>
    /// <param name="isEnabled">True to enable, otherwise to disable.</param>

    this.isStatusBarEnabled(isEnabled === true);
};

Microsoft.WebPortal.Views.List.prototype.setEmptyListUI = function (viewModel, template) {
    /// <summary>
    /// Configures what to show when the list has no rows.
    /// </summary>
    /// <param name="viewModel">The view model backing the empty list template. You can pass a string for the default empty list template.</param>
    /// <param name="template">The KnockOut template used to render the empty list UI. If not provided, the default template will be used.</param>

    this.emptyListTemplate(template || this.webPortal.Settings.List.DefaultEmptyListTemplate);
    this.emptyListViewModel(viewModel || this.webPortal.Resources.Strings.EmptyListMessage);
};

Microsoft.WebPortal.Views.List.prototype.getSelectedRows = function () {
    /// <summary>
    /// Returns an array of selected rows.
    /// </summary>
    /// <returns type="Array">An array of selected rows.</returns>
    return this.selectedRows();
};

Microsoft.WebPortal.Views.List.prototype.onRender = function () {
    /// <summary>
    /// Called when the list is rendered.
    /// </summary>

    // assign the journey template to the HTML element and bind it
    $(this.elementSelector).attr("data-bind", "template: { name: '" + this.template + "' }");
    ko.applyBindings(this, $(this.elementSelector)[0]);

    this.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.OnWindowResizing, this._resizeBody, this);
    this.webPortal.EventSystem.subscribe(Microsoft.WebPortal.Event.OnWindowResized, this._resizeBody, this);
};

Microsoft.WebPortal.Views.List.prototype.onShown = function (isShown) {
    if (isShown) {
        this._resizeBody(Microsoft.WebPortal.Event.OnWindowResizing);
        this._resizeBody();
        this.renderer().ensureOccupancy();
    }
};

Microsoft.WebPortal.Views.List.prototype.onDestroy = function () {
    /// <summary>
    /// Called when the journey trail is about to be destroyed.
    /// </summary>

    if ($(this.elementSelector)[0]) {
        // if the element is there, clear its bindings and clean up its content
        ko.cleanNode($(this.elementSelector)[0]);
        $(this.elementSelector).empty();
    }

    this.webPortal.EventSystem.unsubscribe(Microsoft.WebPortal.Event.OnWindowResizing, this._resizeBody, this);
    this.webPortal.EventSystem.unsubscribe(Microsoft.WebPortal.Event.OnWindowResized, this._resizeBody, this);
};

Microsoft.WebPortal.Views.List.prototype._resizeBody = function (eventId) {
    var listBodySelector = this.elementSelector + this.webPortal.Settings.List.ListBodySelector;

    switch (eventId) {
        case Microsoft.WebPortal.Event.OnWindowResizing:
            // set the body height and width to zero to allow the parent to resize correctly to the new window size without causing overflows
            $(listBodySelector).height(0);
            $(listBodySelector).width(0);
            this._updateScrollBarStatus();
            break;
        default:
            // resize the body and width height to fully fit the parent
            $(listBodySelector).height($(this.elementSelector).height() -
                $(this.elementSelector + this.webPortal.Settings.List.ListHeaderSelector).height() -
                $(this.elementSelector + this.webPortal.Settings.List.ListFooterSelector).height());

            $(listBodySelector).width($(this.elementSelector).width());

            // since the size change may cause empty areas in the body, let the renderer ensure its occupancy
            this.renderer().ensureOccupancy();
            this._updateScrollBarStatus();
            break;
    }
};

Microsoft.WebPortal.Views.List.prototype._mutateToSelectable = function (entities) {
    /// <summary>
    /// Adds a selected observable to each of the provided entities.
    /// </summary>
    /// <param name="entities">An array of objects to be made selectable.</param>

    var self = this;

    if (entities && entities.length > 0 && self.isAllSelected()) {
        // since we have received new entities, turn off the select all flag (silently since no selection has changed)
        self._silentClearSelectAll();
    }

    for (var i = 0; entities && i < entities.length; ++i) {
        // add a selected observable to the entity
        entities[i].isSelected = ko.observable(false);

        // listen to it's updates
        entities[i].isSelected.subscribe(function (isSelected) {
            if (isSelected) {
                if (self.isSingleSelect()) {
                    // unselect all the other entities
                    self._silentDeselectAll();
                }

                // add the row to the selected rows list
                self.selectedRows.push(this);
            } else {
                // remove the row from the selected entities list
                var index = self.selectedRows.indexOf(this);

                if (index !== -1) {
                    self.selectedRows.splice(index, 1);
                }

                if (self.isAllSelected()) {
                    // clear off the select all check box (silently)
                    self._silentClearSelectAll();
                }
            }

            if (!self.supressSelectEvent && self.listener && self.listener.onSelectionChanged) {
                // notify the listener of a selection change
                self.listener.onSelectionChanged(self.selectedRows());
            }
        }, entities[i]);
    }
};

Microsoft.WebPortal.Views.List.prototype._silentDeselectAll = function () {
    /// <summary>
    /// Clears all selected rows silenty without emitting any selection events.
    /// </summary>

    var oldSupressSelectEvent = this.supressSelectEvent;
    this.supressSelectEvent = true;

    while (this.selectedRows() && this.selectedRows().length > 0) {
        // deselecting the item will remove it from the selected rows list
        this.selectedRows()[0].isSelected(false);
    }

    this.supressSelectEvent = oldSupressSelectEvent;
};

Microsoft.WebPortal.Views.List.prototype._silentClearSelectAll = function () {
    /// <summary>
    /// Clears the selected all observable without causing any selection events to be emitted.
    /// </summary>

    var oldSupressSelectEvent = this.supressSelectEvent;
    this.supressSelectEvent = true;
    this.isAllSelected(false);
    this.supressSelectEvent = oldSupressSelectEvent;
};

Microsoft.WebPortal.Views.List.prototype._configureSelection = function () {
    // define the list selection mode and default it to none
    this.selectMode = ko.observable(Microsoft.WebPortal.Views.List.SelectionMode.None);

    // a flag that indicates if the list supports row selection or not
    this.isSelectable = ko.computed(function () {
        return this.selectMode() === Microsoft.WebPortal.Views.List.SelectionMode.Multiple || this.selectMode() === Microsoft.WebPortal.Views.List.SelectionMode.Single;
    }, this);

    // a flaf that indicates if the list is in single select mode or not
    this.isSingleSelect = ko.computed(function () {
        return this.selectMode() === Microsoft.WebPortal.Views.List.SelectionMode.Single;
    }, this);

    // a list of selected rows
    this.selectedRows = ko.observableArray([]);

    // indicates whether the select all checkbox is on or off
    this.isAllSelected = ko.observable(false);

    this.isAllSelected.subscribe(function (allSelected) {
        /// <summary>
        /// Called when select all value is changed.
        /// </summary>
        /// <param name="allSelected">The new select all value.</param>

        if (this.supressSelectEvent) {
            // do not report anything
            return;
        }

        // silence select event reporting
        this.supressSelectEvent = true;

        // change select state for each entity
        this.renderer().performSelectAll(allSelected);

        // put it back on
        this.supressSelectEvent = false;

        if (this.listener && this.listener.onSelectionChanged && this.isSelectable() && !this.isSingleSelect()) {
            // notify listener of the selection change
            this.listener.onSelectionChanged(this.selectedRows());
        }
    }, this);

    // a flag to control whether to fire select change events or not.
    // this is used when the select all is clicked to prevent emitting messages for each entity.
    this.supressSelectEvent = false;
};

Microsoft.WebPortal.Views.List.prototype._configureSorting = function () {
    // list sort information
    this.sortField = ko.observable("");
    this.sortDirection = ko.observable(Microsoft.WebPortal.Views.List.SortDirection.Ascending);

    this.sortIcon = ko.computed(function () {
        return this.sortDirection() === Microsoft.WebPortal.Views.List.SortDirection.Descending ?
            this.webPortal.Resources.Images.SortDescending : this.webPortal.Resources.Images.SortAscending;
    }, this);

    var self = this;

    this.onSortChanged = function (column) {
        /// <summary>
        /// Called when the user changes the list sorting.
        /// </summary>
        /// <param name="column">The column to sort by.</param>

        if (column.sortable() === false) {
            // ignore this
            return;
        }

        if (column.field() !== self.sortField()) {
            // set the new field as the sort field and set direction to ascending
            self.sortField(column.field());
            self.sortDirection(Microsoft.WebPortal.Views.List.SortDirection.Ascending);
        } else {
            // just invert the sorting direction
            self.sortDirection(self.sortDirection() === Microsoft.WebPortal.Views.List.SortDirection.Ascending ?
                Microsoft.WebPortal.Views.List.SortDirection.Descending : Microsoft.WebPortal.Views.List.SortDirection.Ascending);
        }

        if (self.listener && self.listener.onSortChanged) {
            // notify the listener
            self.listener.onSortChanged(self.sortField(), self.sortDirection());
        }
    };
};

Microsoft.WebPortal.Views.List.prototype._configureClicking = function () {
    this.onCellClicked = function (self, row, column) {
        /// <summary>
        /// Called when the user clicks on a cell.
        /// </summary>
        /// <param name="self">The list object.</param>
        /// <param name="row">The row clicked.</param>
        /// <param name="column">The column this cell belongs to.</param>

        if (self.listener && self.listener.onCellClicked) {
            self.listener.onCellClicked(column, row);
        }
    };
};

Microsoft.WebPortal.Views.List.prototype._updateScrollBarStatus = function () {
    var listBodySelector = this.elementSelector + this.webPortal.Settings.List.ListBodySelector;
    this.scrollBarVisible($(listBodySelector).get(0) ? $(listBodySelector).get(0).scrollHeight > $(listBodySelector).height() : false);
};

Microsoft.WebPortal.Views.List.SortDirection = {
    Ascending: "Ascending",
    Descending: "Descending"
};
    
Microsoft.WebPortal.Views.List.SelectionMode = {
    None: "None",
    Single: "Single",
    Multiple: "Multiple"
};

//@ sourceURL=List.js
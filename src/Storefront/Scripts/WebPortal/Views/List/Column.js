/// <reference path="~/Scripts/_references.js" />

Microsoft.WebPortal.Views.List.Column = function (field, style, sortable, clickable, title, tooltip, headerTemplate, cellTemplate) {
    /// <summary>
    /// Defines a list column.
    /// </summary>
    /// <param name="field">The field name this column is attached to. This is used to identify the column and to bind the row property to the list user interface. Required.</param>
    /// <param name="style">The column style. You can set css styles such as width, color, etc... for this column.</param>
    /// <param name="sortable">A boolean that specifies if the column supports sorting or not.</param>
    /// <param name="clickable">A boolean that specifies if the column supports clicking or not. Clickable columns will generate events that can be handled.</param>
    /// <param name="title">The column header title. If not specified, the field will be used as the title.</param>
    /// <param name="tooltip">The field's header tooltip. If not specified then the title will be used as the tooltip.</param>
    /// <param name="headerTemplate">An optional custom header knockout template. The default template (text) will be used if not provided.</param>
    /// <param name="cellTemplate">An optional custom cell knockout template. The default template (text) will be used if not provided.</param>

    $WebPortal.Helpers.throwIfNotSet(field, "field", "Microsoft.WebPortal.Views.List.Column");

    this.field = ko.observable(field);
    this.style = ko.observable(style || "");
    this.sortable = ko.observable(sortable === true);
    this.clickable = ko.observable(clickable === true);
    this.title = ko.observable(title || this.field());
    this.tooltip = ko.observable(tooltip || this.title());
    this.headerTemplate = ko.observable(headerTemplate || $WebPortal.Settings.List.DefaultColumnHeaderTemplate);
    this.cellTemplate = ko.observable(cellTemplate || null);
}

//@ sourceURL=Column.js
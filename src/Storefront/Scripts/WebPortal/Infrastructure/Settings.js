Microsoft.WebPortal.Infrastructure.Settings = {
    /// <summary>
    /// Defines common portal settings. This class just groups there settings together and is different from configuration since
    /// the elements here are not meant to change.
    /// </summary>

    Ids: {
        SplashScreen: "#SplashScreen",
        HeaderBar: "#HeaderBar",
        PrimaryNavigation: "#PrimaryNavigation",
        ExtraActionsMenuItem: "Extra1983",
        ControlPanel: "#ControlPanel",
        ContentPanelContent: "#ContentPanel > div",
        ContextualControl: "#ContextualControlArea",
        ContentPanel: "#ContentPanel",
        BackButton: ".Backbutton",
        PortalContent: "#PortalContent",
        NotificationsSection: "NotificationsSection",
        ActionsSection: "ActionsSection",
        Dialog: "#Dialog",
        DialogShader: "#DialogShader",
        DialogBackgroundOverlay: "#DialogBackgroundOverlay",
        DialogProgressIndicator: "#DialogProgressIndicator"
    },

    Plugins: {
        Expenses: {
            Template: "/Template/Expenses/",
            ExpensesHeaderSelector: "#ExpensesHeader",
            ExpensesListVisualizationSelector: "#ExpensesListVisualization",
            ExpensesChartVisualizationSelector: "#ExpensesChartVisualization",
            ExpensesContainerSelector: "#ExpensesContainer",
            NewExpenseTemplate: "newExpense-template"
        }
    },

    List: {
        Template: "list-template",
        DefaultEmptyListTemplate: "defaultEmptyList-template",
        DefaultColumnHeaderTemplate: "defaultColumnHeader-template",
        ListHeaderSelector: " #ListHeaders",
        ListBodySelector: " .ListBodyContainer",
        ListFooterSelector: " #ListFooter"
    }
};
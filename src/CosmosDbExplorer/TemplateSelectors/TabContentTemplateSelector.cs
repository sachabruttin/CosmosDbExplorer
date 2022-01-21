using System.Windows;
using System.Windows.Controls;
using CosmosDbExplorer.ViewModels;
using CosmosDbExplorer.ViewModels.Assets;

namespace CosmosDbExplorer.TemplateSelectors
{
    public class TabContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? DocumentsTemplate { get; set; }
        public DataTemplate? QueryEditorTemplate { get; set; }
        public DataTemplate? ImportDocumentTemplate { get; set; }
        public DataTemplate? DatabaseViewTemplate { get; set; }
        public DataTemplate? StoredProcedureViewTemplate { get; set; }
        public DataTemplate? UserDefFuncViewTemplate { get; set; }
        public DataTemplate? TriggerViewTemplate { get; set; }
        public DataTemplate? ScaleAndSettingsTemplate { get; set; }
        public DataTemplate? UserEditTempalate { get; set; }
        public DataTemplate? PermissionEditTemplate { get; set; }
        public DataTemplate? MetricsTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is DatabaseViewModel)
            {
                return DatabaseViewTemplate;
            }

            if (item is DocumentsTabViewModel)
            {
                return DocumentsTemplate;
            }

            if (item is QueryEditorViewModel)
            {
                return QueryEditorTemplate;
            }

            //if (item is ImportDocumentViewModel)
            //{
            //    return ImportDocumentTemplate;
            //}

            if (item is StoredProcedureTabViewModel)
            {
                return StoredProcedureViewTemplate;
            }

            if (item is UserDefFuncTabViewModel)
            {
                return UserDefFuncViewTemplate;
            }

            if (item is TriggerTabViewModel)
            {
                return TriggerViewTemplate;
            }

            //if (item is ScaleAndSettingsTabViewModel)
            //{
            //    return ScaleAndSettingsTemplate;
            //}

            //if (item is UserEditViewModel)
            //{
            //    return UserEditTempalate;
            //}

            //if (item is PermissionEditViewModel)
            //{
            //    return PermissionEditTemplate;
            //}

            if (item is MetricsTabViewModel)
            {
                return MetricsTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}

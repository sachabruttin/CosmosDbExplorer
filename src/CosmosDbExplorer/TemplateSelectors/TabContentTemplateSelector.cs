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
        public DataTemplate? ContainerScaleSettingsTemplate { get; set; }
        public DataTemplate? DatabaseScaleTemplate { get; set; }
        public DataTemplate? UserEditTempalate { get; set; }
        public DataTemplate? PermissionEditTemplate { get; set; }
        public DataTemplate? MetricsTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                DatabaseViewModel => DatabaseViewTemplate,
                DocumentsTabViewModel => DocumentsTemplate,
                QueryEditorViewModel => QueryEditorTemplate,
                ImportDocumentViewModel => ImportDocumentTemplate,
                StoredProcedureTabViewModel => StoredProcedureViewTemplate,
                UserDefFuncTabViewModel => UserDefFuncViewTemplate,
                TriggerTabViewModel => TriggerViewTemplate,
                ContainerScaleSettingsViewModel => ContainerScaleSettingsTemplate,
                DatabaseScaleViewModel => DatabaseScaleTemplate,
                UserEditViewModel => UserEditTempalate,
                MetricsTabViewModel => MetricsTemplate,
                PermissionEditViewModel => PermissionEditTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
    }
}

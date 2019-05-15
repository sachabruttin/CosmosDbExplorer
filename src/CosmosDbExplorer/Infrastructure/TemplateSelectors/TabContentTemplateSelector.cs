using System.Windows;
using System.Windows.Controls;
using CosmosDbExplorer.ViewModel;
using CosmosDbExplorer.ViewModel.Assets;

namespace CosmosDbExplorer.Infrastructure.TemplateSelectors
{
    public class TabContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DocumentsTemplate { get; set; }
        public DataTemplate QueryEditorTemplate { get; set; }
        public DataTemplate ImportDocumentTemplate { get; set; }
        public DataTemplate DatabaseViewTemplate { get; set; }
        public DataTemplate StoredProcedureViewTemplate { get; set; }
        public DataTemplate UserDefFuncViewTemplate { get; set; }
        public DataTemplate TriggerViewTemplate { get; set; }
        public DataTemplate ScaleAndSettingsTemplate { get; set; }
        public DataTemplate UserEditTempalate { get; set; }
        public DataTemplate PermissionEditTemplate { get; set; }
        public DataTemplate CollectionMetricsTemplate { get; set; }
        public DataTemplate DatabaseScaleTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            switch (item)
            {
                case DatabaseViewModel _:
                    return DatabaseViewTemplate;
                case DocumentsTabViewModel _:
                    return DocumentsTemplate;
                case QueryEditorViewModel _:
                    return QueryEditorTemplate;
                case ImportDocumentViewModel _:
                    return ImportDocumentTemplate;
                case StoredProcedureTabViewModel _:
                    return StoredProcedureViewTemplate;
                case UserDefFuncTabViewModel _:
                    return UserDefFuncViewTemplate;
                case TriggerTabViewModel _:
                    return TriggerViewTemplate;
                case ScaleAndSettingsTabViewModel _:
                    return ScaleAndSettingsTemplate;
                case UserEditViewModel _:
                    return UserEditTempalate;
                case PermissionEditViewModel _:
                    return PermissionEditTemplate;
                case CollectionMetricsTabViewModel _:
                    return CollectionMetricsTemplate;
                case DatabaseScaleTabViewModel _:
                    return DatabaseScaleTemplate;
                default:
                    return base.SelectTemplate(item, container);
            }
        }
    }
}

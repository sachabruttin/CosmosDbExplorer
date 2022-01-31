using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.ViewModels.DatabaseNodes;
using Validar;

namespace CosmosDbExplorer.ViewModels
{
    public class ContainerScaleSettingsViewModel : PaneWithZoomViewModel<ScaleSettingsNodeViewModel>
    {
        private readonly IServiceProvider _serviceProvider;

        public ContainerScaleSettingsViewModel(IServiceProvider serviceProvider, IUIServices uiServices) 
            : base(uiServices)
        {
            _serviceProvider = serviceProvider;
        }

        public ScaleSettingsNodeViewModel Node { get; private set; }
        public CosmosConnection Connection { get; private set; }
        public CosmosContainer Container { get; private set; }

        public override void Load(string contentId, ScaleSettingsNodeViewModel node, CosmosConnection connection, CosmosContainer container)
        {
            //IsLoading = true;

            ContentId = contentId;
            Node = node;
            Title = node.Name;
            Header = node.Name;
            Connection = connection;
            Container = container;

            var split = Container.SelfLink.Split(new char[] { '/' });
            ToolTip = $"{split[1]}>{split[3]}";

            AccentColor = Connection.AccentColor;

            //SetInformation();

            //IsLoading = false;
        }
    }
}

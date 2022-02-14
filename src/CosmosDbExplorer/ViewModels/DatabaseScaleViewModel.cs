using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CosmosDbExplorer.Contracts.Services;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Core.Services;
using CosmosDbExplorer.ViewModels.DatabaseNodes;

using FluentValidation;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

using PropertyChanged;
using System.Windows.Input;
using Validar;

namespace CosmosDbExplorer.ViewModels
{
    [InjectValidation]
    public class DatabaseScaleViewModel : PaneWithZoomViewModel<DatabaseScaleNodeViewModel>
    {
        private IServiceProvider _serviceProvider;
        private readonly ISystemService _systemService;
        private ICommand _openUrlCommand;

        public DatabaseScaleViewModel(IServiceProvider serviceProvider, IUIServices uiServices, ISystemService systemService)
            : base(uiServices)
        {
            _serviceProvider = serviceProvider;
            _systemService = systemService;
        }

        public DatabaseScaleNodeViewModel Node { get; private set; }
        public CosmosConnection Connection { get; private set; }
        public CosmosDatabase Database { get; private set; }

        public bool IsThroughputAutoscale { get; set; } = true;

        public int MaxThroughput { get; set; } 
        
        public int? MinThroughput { get; set; }

        //[OnChangedMethod(nameof(UpdateSaveCommandStatus))]
        public int? Throughput { get; set; }

        public int Increment => IsThroughputAutoscale ? 1000 : 100;

        [DependsOn(nameof(IsThroughputAutoscale), nameof(Throughput))]
        public string Information => $"{Throughput * 0.1} RU / s(10 % of max RU/s) - {Throughput} RU/s";

        [DependsOn(nameof(IsThroughputAutoscale), nameof(Throughput))]
        public string DataStoredInGb => $"{Throughput * 0.01}";

        public ICommand OpenUrlCommand => _openUrlCommand ??= new RelayCommand<string>(OpenUrl);

        public override async void Load(string contentId, DatabaseScaleNodeViewModel node, CosmosConnection connection, CosmosContainer container)
        {
            ContentId = contentId;
            Node = node;
            Title = node.Name;
            Header = node.Name;
            Connection = connection;
            Database = node.Parent.Database;

            AccentColor = connection.AccentColor;
            ToolTip = $"{Connection.Label}/{Database.Id}";

            var service = ActivatorUtilities.CreateInstance<CosmosDatabaseService>(_serviceProvider, connection);
            var throughput = await service.GetThroughputAsync(Database);

            MinThroughput = throughput.MinThroughtput;
            MaxThroughput = int.MaxValue - (int.MaxValue % 1000);
            IsThroughputAutoscale = throughput.AutoscaleMaxThroughput.HasValue;
            Throughput = throughput.AutoscaleMaxThroughput ?? throughput.Throughput;
        }

        private void OpenUrl(string? url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                _systemService.OpenInWebBrowser(url);
            }
        }
    }

    public class DatabaseScaleViewModelValidator : AbstractValidator<DatabaseScaleViewModel>
    {
        public DatabaseScaleViewModelValidator()
        {
            RuleFor(x => x.Throughput)
                .NotEmpty()
                .GreaterThanOrEqualTo(x => x.MinThroughput)
                .LessThanOrEqualTo(x => x.MaxThroughput)
                .Custom((throughput, context) =>
                {
                    if (throughput%context.InstanceToValidate.Increment != 0)
                    {
                        context.AddFailure($"Value must be a multiple of {context.InstanceToValidate.Increment}.");
                    }
                });
        }
    }
}

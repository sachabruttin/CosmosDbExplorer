using System.ComponentModel;
using System.Linq;
using CosmosDbExplorer.Infrastructure;
using FluentValidation;
using GalaSoft.MvvmLight;
using Microsoft.Azure.Documents;
using Validar;

namespace CosmosDbExplorer.ViewModel.Indexes
{
    [InjectValidation]
    public class IndexingPolicyViewModel : ObservableObject
    {
        private RelayCommand<IncludedPathViewModel> _removeIncludedPathCommand;
        private RelayCommand _addIncludedPathCommand;

        private RelayCommand<ExcludedPathViewModel> _removeExcludedPathCommand;
        private RelayCommand _addExcludedPathCommand;

        public IndexingPolicyViewModel(IndexingPolicy policy)
        {
            Policy = policy;
            IncludedPaths = new BindingList<IncludedPathViewModel>(Policy.IncludedPaths.Select(ip => new IncludedPathViewModel(ip)).ToList());
            IncludedPaths.ListChanged += (s, e) =>
            {
                if (IncludedPaths.All(ip => !ip.HasErrors))
                {
                    RaisePropertyChanged(nameof(IncludedPaths));
                }
            };

            ExcludedPaths = new BindingList<ExcludedPathViewModel>(Policy.ExcludedPaths.Select(ep => new ExcludedPathViewModel(ep)).ToList());
            ExcludedPaths.ListChanged += (s, e) =>
            {
                if (ExcludedPaths.All(ep => !ep.HasErrors))
                {
                    RaisePropertyChanged(nameof(ExcludedPaths));
                }
            };
        }

        public IndexingPolicy Policy { get; }

        public bool IsAutomatic
        {
            get => Policy.Automatic;
            set
            {
                Policy.Automatic = value;
                RaisePropertyChanged(nameof(IsAutomatic));
            }
        }

        public IndexingMode Mode
        {
            get => Policy.IndexingMode;
            set
            {
                Policy.IndexingMode = value;
                RaisePropertyChanged(nameof(Mode));
            }
        }

        public BindingList<IncludedPathViewModel> IncludedPaths { get; }

        public BindingList<ExcludedPathViewModel> ExcludedPaths { get; }

        public RelayCommand<IncludedPathViewModel> RemoveIncludedPathCommand
        {
            get
            {
                return _removeIncludedPathCommand ?? (_removeIncludedPathCommand = new RelayCommand<IncludedPathViewModel>(p =>
                {
                    Policy.IncludedPaths.Remove(p.IncludedPath);
                    IncludedPaths.Remove(p);
                }));
            }
        }

        public RelayCommand AddIncludedPathCommand => _addIncludedPathCommand ?? (_addIncludedPathCommand = new RelayCommand(() => IncludedPaths.Add(new IncludedPathViewModel())));

        public RelayCommand<ExcludedPathViewModel> RemoveExcludedPathCommand
        {
            get
            {
                return _removeExcludedPathCommand ?? (_removeExcludedPathCommand = new RelayCommand<ExcludedPathViewModel>(p =>
                {
                    Policy.ExcludedPaths.Remove(p.ExcludedPath);
                    ExcludedPaths.Remove(p);
                }));
            }
        }

        public RelayCommand AddExcludedPathCommand => _addExcludedPathCommand ?? (_addExcludedPathCommand = new RelayCommand(() => ExcludedPaths.Add(new ExcludedPathViewModel())));
    }

    public class IndexingPolicyViewModelValidator : AbstractValidator<IndexingPolicyViewModel>
    {
        public IndexingPolicyViewModelValidator()
        {
            RuleFor(x => x.IncludedPaths)
                .Must(coll => coll.Distinct().Count() == coll.Count)
                .WithMessage("Only one entry per path!");

            RuleFor(x => x.ExcludedPaths)
                .Must(coll => coll.Distinct().Count() == coll.Count)
                .WithMessage("Only one entry per path!");
        }
    }
}

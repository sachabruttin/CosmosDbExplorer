using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CosmosDbExplorer.Infrastructure;
using FluentValidation;
using GalaSoft.MvvmLight;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using PropertyChanged;
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

        private bool _isCreating;

        public IndexingPolicyViewModel(IndexingPolicy policy)
        {
            _isCreating = true;

            // deep clon the indexing policy
            var json = JsonConvert.SerializeObject(policy);
            Policy = JsonConvert.DeserializeObject<IndexingPolicy>(json);

            _isCreating = false;

            IncludedPaths.ListChanged += (s, e) =>
            {
                if (IncludedPaths.All(ip => !ip.HasErrors))
                {
                    RaisePropertyChanged(nameof(IncludedPaths));
                }
            };

            ExcludedPaths.ListChanged += (s, e) =>
            {
                if (ExcludedPaths.All(ep => !ep.HasErrors))
                {
                    RaisePropertyChanged(nameof(ExcludedPaths));
                }
            };

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != nameof(IsValid))
                {
                    RaisePropertyChanged(nameof(IsValid));
                }
            };
        }

        public IndexingPolicy Policy { get; set; }

        protected void OnPolicyChanged()
        {
            IncludedPaths.Clear();
            foreach (var vm in Policy.IncludedPaths.Select(ip => new IncludedPathViewModel(ip)))
            {
                IncludedPaths.Add(vm);
            }

            ExcludedPaths.Clear();
            foreach (var vm in Policy.ExcludedPaths.Select(ep => new ExcludedPathViewModel(ep)))
            {
                ExcludedPaths.Add(vm);
            }

            if (_isCreating)
            {
                IsChanged = false;
            }
        }

        public bool IsValid
        {
            get
            {
                return !((INotifyDataErrorInfo)this).HasErrors
                    && IncludedPaths.All(ip => !ip.HasErrors)
                    && ExcludedPaths.All(ep => !ep.HasErrors);
            }
        }

        public bool IsChanged { get; set; }

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

                if (Mode == IndexingMode.None)
                {
                    Policy.IncludedPaths.Clear();
                    IncludedPaths.Clear();
                }
            }
        }

        [DependsOn(nameof(Mode))]
        public BindingList<IncludedPathViewModel> IncludedPaths { get; } = new BindingList<IncludedPathViewModel>();

        [DependsOn(nameof(Mode))]
        public BindingList<ExcludedPathViewModel> ExcludedPaths { get; } = new BindingList<ExcludedPathViewModel>();

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

        public RelayCommand AddIncludedPathCommand
        {
            get
            {
                return _addIncludedPathCommand ?? (_addIncludedPathCommand = new RelayCommand(() =>
                {
                    var path = new IncludedPath
                    {
                        Indexes = new Collection<Index>() { new HashIndex(DataType.String, 3) }
                    };

                    Policy.IncludedPaths.Add(path);
                    IncludedPaths.Add(new IncludedPathViewModel(path));
                }));
            }
        }

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

        public RelayCommand AddExcludedPathCommand
        {
            get
            {
                return _addExcludedPathCommand ?? (_addExcludedPathCommand = new RelayCommand(() =>
                {
                    var path = new ExcludedPath();
                    Policy.ExcludedPaths.Add(path);
                    ExcludedPaths.Add(new ExcludedPathViewModel(path));
                }));
            }
        }
    }

    public class IndexingPolicyViewModelValidator : AbstractValidator<IndexingPolicyViewModel>
    {
        public IndexingPolicyViewModelValidator()
        {
            RuleFor(x => x.IncludedPaths)
                .Must(coll => coll.Distinct().Count() == coll.Count)
                .WithMessage("Only one entry per path!");

            RuleFor(x => x.IncludedPaths)
                .NotEmpty()
                .When(x => x.Mode != IndexingMode.None);

            RuleFor(x => x.ExcludedPaths)
                .Must(coll => coll.Distinct().Count() == coll.Count)
                .WithMessage("Only one entry per path!");
        }
    }
}

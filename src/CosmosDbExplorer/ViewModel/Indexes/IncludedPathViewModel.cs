using System;
using System.Collections.ObjectModel;
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
    public class IncludedPathViewModel : ObservableObject, IEquatable<IncludedPathViewModel>
    {
        private RelayCommand<IndexViewModel> _removeIndexCommand;
        private RelayCommand _addIndexCommand;

        public IncludedPathViewModel(IncludedPath includedPath)
        {
            IncludedPath = includedPath;
            Indexes = new BindingList<IndexViewModel>(IncludedPath.Indexes.Select(i => new IndexViewModel(i)).ToList());
            Indexes.ListChanged += (s, e) =>
            {
                if (Indexes.Any(i => i.HasErrors))
                {
                    return;
                }

                IncludedPath.Indexes.Clear();
                foreach (var item in Indexes)
                {
                    IncludedPath.Indexes.Add(item.GetIndex());
                }

                RaisePropertyChanged(nameof(Indexes));
            };
        }

        public IncludedPath IncludedPath { get; }

        public string Path
        {
            get => IncludedPath.Path;
            set
            {
                IncludedPath.Path = value;
                RaisePropertyChanged(nameof(Path));
            }
        }

        public BindingList<IndexViewModel> Indexes { get; }

        public RelayCommand<IndexViewModel> RemoveIndexCommand => _removeIndexCommand ?? (_removeIndexCommand = new RelayCommand<IndexViewModel>(p => Indexes.Remove(p)));

        public RelayCommand AddIndexCommand => _addIndexCommand ?? (_addIndexCommand = new RelayCommand(() => Indexes.Add(new IndexViewModel())));

        public bool HasErrors => ((INotifyDataErrorInfo)this).HasErrors;

        public bool Equals(IncludedPathViewModel other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Path == other.Path;
        }

        public override int GetHashCode() => Path?.GetHashCode() ?? 0;
    }

    public class IncludedPathViewModelValidator : AbstractValidator<IncludedPathViewModel>
    {
        public IncludedPathViewModelValidator()
        {
            RuleFor(x => x.Path)
                .NotEmpty()
                .Matches(Constants.Validation.PathRegex);

            RuleFor(x => x.Indexes)
                .Must(coll => coll.Distinct().Count() == coll.Count)
                .WithMessage((vm, coll) => $"Duplicate indexes specified for the path '{vm.Path}' and data type '{string.Join("' and '", coll.GroupBy(g => g.DataType).Where(g => g.Skip(1).Any()).Select(g => g.Key.Value.ToString()))}'.");

            RuleFor(x => x.Indexes)
                .NotEmpty();
        }
    }
}

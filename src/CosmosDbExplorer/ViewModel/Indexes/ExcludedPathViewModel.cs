using System.ComponentModel;
using FluentValidation;
using GalaSoft.MvvmLight;
using Microsoft.Azure.Documents;
using Validar;

namespace CosmosDbExplorer.ViewModel.Indexes
{
    [InjectValidation]
    public class ExcludedPathViewModel : ObservableObject, System.IEquatable<ExcludedPathViewModel>
    {
        public ExcludedPathViewModel(ExcludedPath excludedPath)
        {
            ExcludedPath = excludedPath;
        }

        public ExcludedPath ExcludedPath { get; }

        public string Path
        {
            get => ExcludedPath.Path;
            set
            {
                ExcludedPath.Path = value;
                RaisePropertyChanged(nameof(Path));
            }
        }

        public bool HasErrors => ((INotifyDataErrorInfo)this).HasErrors;

        public bool Equals(ExcludedPathViewModel other)
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

    public class ExcludedPathViewModelValidator : AbstractValidator<ExcludedPathViewModel>
    {
        public ExcludedPathViewModelValidator()
        {
            RuleFor(x => x.Path)
                .NotEmpty()
                .Matches(@"^\/(\w*\/|\[\]\/)*[\*\?]$");
        }
    }
}

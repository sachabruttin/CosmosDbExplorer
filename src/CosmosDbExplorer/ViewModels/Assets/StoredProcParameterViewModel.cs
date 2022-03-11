using System;
using System.IO;
using System.Linq;
//using System.Reactive.Linq;
using FluentValidation;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json.Linq;
using Validar;

namespace CosmosDbExplorer.ViewModels.Assets
{
    [InjectValidation]
    public class StoredProcParameterViewModel : ObservableObject, IDisposable
    {
        private readonly IDisposable _textChangedObservable;

        public StoredProcParameterViewModel()
        {
            Document = string.Empty;
            Kind = StoredProcParameterKind.Array;

            //_textChangedObservable = Observable.FromEventPattern<EventArgs>(Document, nameof(Document.TextChanged))
            //                          .Select(evt => ((TextDocument)evt.Sender).Text)
            //                          .Throttle(TimeSpan.FromMilliseconds(600))
            //                          .Where(text => !string.IsNullOrEmpty(text))
            //                          .DistinctUntilChanged()
            //                          .Subscribe(OnContentTextChanged);
        }

        //private void OnContentTextChanged(string text)
        //{
        //    DispatcherHelper.RunAsync(() => RaisePropertyChanged(nameof(Document)));
        //}

        public StoredProcParameterKind Kind { get; set; }

        public string FileName { get; set; }

        public string Document { get; set; }

        public object GetValue()
        {
            return Kind switch
            {
                StoredProcParameterKind.Object => JToken.Parse(Document),
                StoredProcParameterKind.Array => JArray.Parse(Document),
                StoredProcParameterKind.File => JToken.Parse(File.ReadAllText(FileName)),
                _ => null
            };
        }

        #region IDisposable Support
        private bool _disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _textChangedObservable.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

    public class StoredProcParameterViewModelValidator : AbstractValidator<StoredProcParameterViewModel>
    {
        public StoredProcParameterViewModelValidator()
        {
            //RuleFor(x => x.Value).NotEmpty();
            When(x => x.Kind == StoredProcParameterKind.File, () => RuleFor(x => x.FileName).Must(obj => File.Exists(obj as string)).WithMessage("File not found!"));
            When(x => x.Kind == StoredProcParameterKind.Array, () => RuleFor(x => x.Document).Custom((doc, ctx) =>
            {
                try
                {
                    JArray.Parse(doc);
                }
                catch (Exception ex)
                {
                    ctx.AddFailure(ex.Message);
                }
            }));
            When(x => x.Kind == StoredProcParameterKind.Object, () => RuleFor(x => x.Document).Custom((doc, ctx) =>
            {
                try
                {
                    JToken.Parse(doc);
                }
                catch (Exception ex)
                {
                    ctx.AddFailure(ex.Message);
                }
            }));
        }
    }

    public enum StoredProcParameterKind
    {
        Object,
        Array,
        File,
    }
}

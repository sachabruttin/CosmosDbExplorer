using System.Collections.Specialized;
using CosmosDbExplorer.Core.Models;
using CosmosDbExplorer.Helpers;
using CosmosDbExplorer.Extensions;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace CosmosDbExplorer.ViewModels
{
    public abstract class JsonEditorViewModelBase : ObservableRecipient
    {
        protected JsonEditorViewModelBase()
        {
        }

        public string Text { get; set; }

        public bool IsDirty { get; set; }

        public bool IsReadOnly { get; set; }

        public virtual void SetText(object content, bool removeSystemProperties)
        {
            var text = GetDocumentContent(content, removeSystemProperties) ?? string.Empty;

            //DispatcherHelper.RunAsync(() =>
            //{
            Text = text;
                OnPropertyChanged(nameof(HasContent));
                IsDirty = false;
            //});
        }

        protected abstract string GetDocumentContent(object content, bool removeSystemProperties);

        public bool HasContent => Text?.Length != 0; //Content.TextLength != 0;
    }

    public class JsonViewerViewModel : JsonEditorViewModelBase
    {
        public JsonViewerViewModel()
        {
        }

        protected override string GetDocumentContent(object content, bool removeSystemProperties)
        {
            if (content == null)
            {
                return null;
            }

            var settings = new JsonSerializerSettings
            {
                ContractResolver = removeSystemProperties ? new DocumentDbWithoutSystemPropertyResolver() : null,
                Formatting = Formatting.Indented,
                DateParseHandling = DateParseHandling.None
            };

            try
            {
                var doc = (dynamic)content;
                return JsonConvert.SerializeObject(doc, settings);
            }
            catch
            {
                return JsonConvert.SerializeObject(content, settings);
            }
        }
    }

    public class DocumentEditorViewModel : JsonViewerViewModel
    {
        private CosmosDocument _document;

        public DocumentEditorViewModel() 
        {
        }

        public override void SetText(object content, bool removeSystemProperties)
        {
            _document = content as CosmosDocument;
            base.SetText(_document, removeSystemProperties);
        }

        protected override string GetDocumentContent(object content, bool removeSystemProperties)
        {
            if (_document == null)
            {
                return null;
            }

            var settings = new JsonSerializerSettings
            {
                ContractResolver = removeSystemProperties ? new DocumentDbWithoutSystemPropertyResolver() : null,
                Formatting = Formatting.Indented,
                DateParseHandling = DateParseHandling.None
            };

            return JsonConvert.SerializeObject(_document.Document, settings);
        }

        public string Id => _document?.Id;

        public bool IsNewDocument
        {
            get
            {
                return _document != null && _document.SelfLink == null;
            }
        }
    }

    public class HeaderEditorViewModel : JsonEditorViewModelBase
    {
        public HeaderEditorViewModel()
        {
        }

        protected override string GetDocumentContent(object content, bool removeSystemProperties)
        {
            if (content == null)
            {
                return null;
            }

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                DateParseHandling = DateParseHandling.None
            };
            settings.Converters.Add(new OrderedDictionaryConverter());

            return JsonConvert.SerializeObject(((NameValueCollection)content).ToDictionary(), settings);
        }
    }
}

using System.Collections.Specialized;
using CosmosDbExplorer.Infrastructure.Extensions;
using CosmosDbExplorer.Infrastructure.JsonHelpers;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace CosmosDbExplorer.ViewModel
{
    public abstract class JsonEditorViewModelBase : ViewModelBase
    {
        protected JsonEditorViewModelBase(IMessenger messenger) : base(messenger)
        {
            Content = new TextDocument();
        }

        public TextDocument Content { get; set; }

        public bool IsDirty { get; set; }

        public bool IsReadOnly { get; set; }

        public virtual void SetText(object content, bool removeSystemProperties)
        {
            var text = GetDocumentContent(content, removeSystemProperties) ?? string.Empty;

            DispatcherHelper.RunAsync(() =>
            {
                Content.Text = text;
                RaisePropertyChanged(() => HasContent);
                IsDirty = false;
            });
        }

        protected abstract string GetDocumentContent(object content, bool removeSystemProperties);

        public bool HasContent => Content.TextLength != 0;
    }

    public class JsonViewerViewModel : JsonEditorViewModelBase
    {
        public JsonViewerViewModel(IMessenger messenger) : base(messenger)
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
                FeedResponse<Document> doc = (dynamic)content;
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
        private Document _document;

        public DocumentEditorViewModel(IMessenger messenger) : base(messenger)
        {
        }

        public override void SetText(object content, bool removeSystemProperties)
        {
            _document = content as Document;
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

            return JsonConvert.SerializeObject(_document, settings);
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
        public HeaderEditorViewModel(IMessenger messenger) : base(messenger)
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

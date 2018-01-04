using DocumentDbExplorer.Infrastructure.Extensions;
using DocumentDbExplorer.Infrastructure.JsonHelpers;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace DocumentDbExplorer.ViewModel
{
    public abstract class JsonEditorViewModelBase : ViewModelBase
    {
        protected JsonEditorViewModelBase(IMessenger messenger) : base(messenger)
        {
            Content = new TextDocument();
        }

        public TextDocument Content { get; set; }

        public bool IsDirty { get; set; }

        public bool IsReadOnly { get; set; } = false;
        
        public virtual void SetText(object content, bool removeSystemProperties)
        {
            DispatcherHelper.RunAsync(() =>
            {
                Content.Text = GetDocumentContent(content, removeSystemProperties);
                IsDirty = false;
            });
        }

        protected string GetDocumentContent(object content, bool removeSystemProperties)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = removeSystemProperties ? new DocumentDbWithoutSystemPropertyResolver() : null,
                Formatting = Formatting.Indented
            };

            return JsonConvert.SerializeObject(content, settings);
        }
    }

    public class DocumentEditorViewModel : JsonEditorViewModelBase
    {
        private Document _document;
        
        public DocumentEditorViewModel(IMessenger messenger) : base(messenger)
        {
        }

        public override void SetText(object content, bool removeSystemProperties)
        {
            _document = content as Document;
            RaisePropertyChanged(() => IsVisible);

            base.SetText(_document, removeSystemProperties);
        }

        public string Id => _document?.Id;

        public bool IsNewDocument
        {
            get
            {
                return _document != null && _document.SelfLink == null;
            }
        }

        public bool IsVisible => _document != null;
    }

    public class JsonViewerViewModel : JsonEditorViewModelBase
    {
        public JsonViewerViewModel(IMessenger messenger) : base(messenger)
        {
        }
    }

    public class FeedResponseEditorViewModel : JsonEditorViewModelBase
    {
        public FeedResponseEditorViewModel(IMessenger messenger) : base(messenger)
        {
        }

        public override void SetText(object content, bool removeSystemProperties)
        {
            DispatcherHelper.RunAsync(() =>
            {
                Content.Text = GetHeader(content as FeedResponse<Document>);
                IsDirty = false;
            });
        }

        private string GetHeader(FeedResponse<Document> content)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };
            settings.Converters.Add(new OrderedDictionaryConverter());

            return JsonConvert.SerializeObject(content.ResponseHeaders.ToDictionary(), settings);
        }
    }
}

using System.Collections.Specialized;
using DocumentDbExplorer.Infrastructure.Extensions;
using DocumentDbExplorer.Infrastructure.JsonHelpers;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Azure.Documents;
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
                Content.Text = GetDocumentContent(content, removeSystemProperties) ?? string.Empty;
                RaisePropertyChanged(() => HasContent);
                IsDirty = false;
            });
        }

        protected virtual string GetDocumentContent(object content, bool removeSystemProperties)
        {
            if (content == null)
            {
                return null;
            }

            var settings = new JsonSerializerSettings
            {
                ContractResolver = removeSystemProperties ? new DocumentDbWithoutSystemPropertyResolver() : null,
                Formatting = Formatting.Indented
            };

            return JsonConvert.SerializeObject(content, settings);
        }

        public bool HasContent => Content.TextLength != 0;
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
            base.SetText(content, removeSystemProperties);
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

    public class JsonViewerViewModel : JsonEditorViewModelBase
    {
        public JsonViewerViewModel(IMessenger messenger) : base(messenger)
        {
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
                Formatting = Formatting.Indented
            };
            settings.Converters.Add(new OrderedDictionaryConverter());

            return JsonConvert.SerializeObject(((NameValueCollection)content).ToDictionary(), settings);
        }
    }
}

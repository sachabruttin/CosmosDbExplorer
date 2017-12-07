using System.Collections.Generic;
using System.Reflection;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
        
        public virtual void SetText(object content, bool removeSystemProperties)
        {
            DispatcherHelper.RunAsync(() =>
            {
                Content.Text = GetDocumentContent(content, true);
                IsDirty = false;
            });
        }

        protected string GetDocumentContent(object content, bool removeSystemProperties)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DocumentDbWithoutSystemPropertyResolver(),
                Formatting = Formatting.Indented
            };

            return JsonConvert.SerializeObject(content, settings);
        }
    }

    internal class DocumentDbWithoutSystemPropertyResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var systemResourceNames = new List<string> { "_rid", "_etag", "_ts", "_self", "_id", "_attachments", "_docs", "_sprocs", "_triggers", "_udfs", "_conflicts", "_colls", "_users" };
            var prop = base.CreateProperty(member, memberSerialization);

            if (systemResourceNames.Contains(prop.PropertyName))
            {
                prop.Readable = false;
            }

            return prop;
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

            base.SetText(_document, true);
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
}

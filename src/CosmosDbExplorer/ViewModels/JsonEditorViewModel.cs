using System;
using CosmosDbExplorer.Helpers;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmosDbExplorer.ViewModels
{
    public abstract class JsonEditorViewModelBase : ObservableRecipient
    {
        protected JsonEditorViewModelBase()
        {
        }

        public string? Text { get; set; }

        public bool IsDirty { get; set; }

        public bool IsReadOnly { get; set; }

        public virtual void SetText(object? content, bool removeSystemProperties)
        {
            var text = GetDocumentContent(content, removeSystemProperties) ?? string.Empty;

            Text = text;
            OnPropertyChanged(nameof(HasContent));
            IsDirty = false;
        }

        protected abstract string? GetDocumentContent(object? content, bool removeSystemProperties);

        public bool HasContent => Text?.Length != 0; //Content.TextLength != 0;
    }

    public class JsonViewerViewModel : JsonEditorViewModelBase
    {
        private static readonly string[] SystemResourceNames = new[] { "_rid", "_etag", "_ts", "_self", "_id", "_attachments", "_docs", "_sprocs", "_triggers", "_udfs", "_conflicts", "_colls", "_users" };

        public JsonViewerViewModel()
        {
        }

        protected override string? GetDocumentContent(object? content, bool removeSystemProperties)
        {
            if (content == null)
            {
                return null;
            }

            var _document = new JArray(content);

            return removeSystemProperties
                ? RemoveCosmosSystemProperties(_document)
                : _document.ToString(Formatting.Indented);
        }

        private static string RemoveCosmosSystemProperties(JArray content)
        {
            foreach (var obj in content.Values<JObject>())
            {
                if (obj != null)
                {
                    foreach (var item in SystemResourceNames)
                    {
                        obj.Remove(item);
                    }
                }
            }

            return content.ToString(Formatting.Indented);
        }
    }

    public class DocumentEditorViewModel : JsonViewerViewModel
    {
        private static readonly string[] SystemResourceNames = new [] { "_rid", "_etag", "_ts", "_self", "_id", "_attachments", "_docs", "_sprocs", "_triggers", "_udfs", "_conflicts", "_colls", "_users" };
        private JObject? _document;

        public DocumentEditorViewModel() 
        {
        }

        protected override string? GetDocumentContent(object? content, bool removeSystemProperties)
        {
            if (content == null)
            {
                return null;
            }

            _document = (JObject)content;

            return removeSystemProperties 
                ? RemoveCosmosSystemProperties(_document)
                : _document.ToString(Formatting.Indented);
        }

        private static string RemoveCosmosSystemProperties(JObject content)
        {
            var document = new JObject(content); // create a copy of the object 

            foreach (var item in SystemResourceNames)
            {
                document.Remove(item);
            }

            return document.ToString(Formatting.Indented);
        }


        public string? Id => _document?.Property("id")?.Value<string>();

        public bool IsNewDocument
        {
            get
            {
                return _document != null && _document.GetValue("_self")?.Value<string?>() == null;
            }
        }
    }

    public class HeaderEditorViewModel : JsonEditorViewModelBase
    {
        public HeaderEditorViewModel()
        {
        }

        protected override string? GetDocumentContent(object content, bool removeSystemProperties)
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

            return JsonConvert.SerializeObject(content, settings);
            //return JsonConvert.SerializeObject(((NameValueCollection)content).ToDictionary(), settings);
        }
    }
}

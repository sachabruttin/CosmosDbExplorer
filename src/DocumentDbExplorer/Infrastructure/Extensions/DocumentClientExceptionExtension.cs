using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace CosmosDbExplorer.Infrastructure.Extensions
{
    public static class DocumentClientExceptionExtension
    {
        public static string Parse(this DocumentClientException exception)
        {
            var message = exception.Message;
            if (exception.Message.StartsWith("Message: {"))
            {
                message = ParseSyntaxException(exception);
            }

            return message.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        }

        private static string ParseSyntaxException(DocumentClientException exception)
        {
            var message = exception.Message.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                            .First()
                            .Remove(0, "Message: ".Length);

            try
            {
                var obj = JsonConvert.DeserializeObject<DocumentClientExceptionMessage>(message);
                return obj.ToString();
            }
            catch
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<dynamic>(message);
                    List<string> res = obj.Errors.ToObject<List<string>>();
                    return string.Concat(res);
                }
                catch
                {
                    return exception.Message;
                }
            }
        }
    }

    public class Location
    {
        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("end")]
        public int End { get; set; }
    }

    public class Error
    {
        [JsonProperty("severity")]
        public string Severity { get; set; }

        [JsonProperty("location")]
        public Location Location { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        public override string ToString()
        {
            return $"{Code}: {Message}";
        }
    }

    public class DocumentClientExceptionMessage
    {

        [JsonProperty("errors")]
        public IList<Error> Errors { get; set; }

        public override string ToString()
        {
            var message = new StringBuilder();

            foreach (var error in Errors)
            {
                message.AppendLine(error.ToString());
            }

            return message.ToString();
        }
    }
}

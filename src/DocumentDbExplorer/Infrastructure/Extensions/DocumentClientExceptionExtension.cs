using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace DocumentDbExplorer.Infrastructure.Extensions
{
    public static class DocumentClientExceptionExtension
    {
        public static DocumentClientExceptionMessage Parse(this DocumentClientException exception)
        {
            var message = exception.Message.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                            .First()
                            .Remove(0, "Message: ".Length);

            return JsonConvert.DeserializeObject<DocumentClientExceptionMessage>(message);
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

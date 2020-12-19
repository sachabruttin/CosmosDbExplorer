using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace CosmosDbExplorer.Infrastructure.Extensions
{
    public static class DocumentClientExceptionExtension
    {
        private static readonly Regex ErrorDocumentRegex = new Regex("Message: (?<json>.*), documentdb-dotnet-sdk/.*$", RegexOptions.Compiled);

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
            try
            {
                var json = ErrorDocumentRegex.Match(exception.Message).Groups["json"].Value;

                var errorDoc = JsonConvert.DeserializeObject<dynamic>(json);

                var sb = new StringBuilder();
                foreach (dynamic e in errorDoc.errors)
                {
                    sb.Append($"{e.code}: {e.message}");
                    sb.Append(Environment.NewLine);
                }

                return sb.ToString();
            }
            catch
            {
                return exception.Message;
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

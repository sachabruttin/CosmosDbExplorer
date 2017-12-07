using System;
using Newtonsoft.Json;

namespace DocumentDbExplorer.Infrastructure.Models
{
    public class Connection : IEquatable<Connection>
    {
        public Connection(string label, Uri endpoint, string secret)
        {
            Label = label;
            DatabaseUri = endpoint;
            AuthenticationKey = secret;
        }

        [JsonProperty]
        public string Label { get; set; }

        [JsonProperty]
        public Uri DatabaseUri { get; protected set; }

        [JsonProperty]
        public string AuthenticationKey { get; protected set; }

        public bool IsLocalEmulator()
        {
            return DatabaseUri == Constants.Emulator.Endpoint
                && AuthenticationKey == Constants.Emulator.Secret;
        }

        public bool Equals(Connection other)
        {
            return Label.Equals(other.Label);
        }

        public override int GetHashCode()
        {
            return Label.GetHashCode();
        }
    }
}

using System;
using System.ComponentModel;
using DocumentDbExplorer.Infrastructure.MarkupExtensions;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace DocumentDbExplorer.Infrastructure.Models
{
    public class Connection : IEquatable<Connection>
    {
        public Connection(string label, Uri endpoint, string secret, ConnectionType connectionType)
        {
            Label = label;
            DatabaseUri = endpoint;
            AuthenticationKey = secret;
            ConnectionType = connectionType;
        }

        [JsonProperty]
        public string Label { get; set; }

        [JsonProperty]
        public Uri DatabaseUri { get; protected set; }

        [JsonProperty]
        public string AuthenticationKey { get; protected set; }

        [JsonProperty]
        public ConnectionType ConnectionType { get; protected set; }

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

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ConnectionType
    {
        Gateway,
        [Description("Direct HTTPS")]
        DirectHttps,
        [Description("Direct TCP")]
        DirectTcp
    }
}

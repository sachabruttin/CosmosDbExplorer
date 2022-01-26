using System;
using System.ComponentModel;
using System.Drawing;
using Newtonsoft.Json;

namespace CosmosDbExplorer.Core.Models
{
    public class CosmosConnection : IEquatable<CosmosConnection>
    {
        public CosmosConnection(Guid id)
        {
            Id = id;
            EnableEndpointDiscovery = true;
        }

        [JsonConstructor]
        public CosmosConnection(Guid? id, string? label, Uri? endpoint, string ?secret, ConnectionType connectionType, bool enableEndpointDiscovery, Color? accentColor)
        {
            Id = id ?? Guid.NewGuid();
            Label = label;
            DatabaseUri = endpoint;
            AuthenticationKey = secret;
            ConnectionType = connectionType;
            EnableEndpointDiscovery = enableEndpointDiscovery;
            AccentColor = accentColor;
        }

        [JsonProperty]
        public Guid Id { get; protected set; }

        [JsonProperty]
        public string? Label { get; protected set; }

        [JsonProperty]
        public Uri? DatabaseUri { get; protected set; }

        [JsonProperty]
        public string? AuthenticationKey { get; protected set; }

        [JsonProperty]
        public ConnectionType ConnectionType { get; protected set; }

        [JsonProperty]
        public bool EnableEndpointDiscovery { get; protected set; }

        [JsonProperty]
        public Color? AccentColor { get; protected set; }

        public bool IsLocalEmulator()
        {
            return Constants.Emulator.Endpoint.Equals(DatabaseUri)
                && AuthenticationKey == Constants.Emulator.Secret;
        }

        public bool Equals(CosmosConnection other)
        {
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ConnectionType
    {
        [Description("Gateway")]
        Gateway,
        [Description("Direct")]
        Direct,
    }
}

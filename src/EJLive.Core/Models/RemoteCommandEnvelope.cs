using System;

namespace EJLive.Core.Models
{
    /// <summary>
    /// Represents a cryptographically signed remote command envelope with risk metadata.
    /// </summary>
    public sealed record RemoteCommandEnvelope
    {
        /// <summary>Unique identifier for this command instance.</summary>
        public required string CommandId { get; init; }

        /// <summary>Discriminator for the command type (e.g., ExecuteScript, Restart, Screenshot).</summary>
        public required string CommandType { get; init; }

        /// <summary>Serialized JSON payload for the command.</summary>
        public required string Payload { get; init; }

        /// <summary>Cryptographic signature over the canonical command bytes.</summary>
        public required string Signature { get; init; }

        /// <summary>UTC timestamp when the command was issued.</summary>
        public required DateTime TimestampUtc { get; init; }

        /// <summary>UTC expiry after which the command must be rejected.</summary>
        public required DateTime ExpiryUtc { get; init; }

        /// <summary>Role of the issuing operator (e.g., Admin, Support).</summary>
        public required string IssuerRole { get; init; }

        /// <summary>Identity of the issuing operator.</summary>
        public required string IssuerId { get; init; }

        /// <summary>Calculated risk level for the command.</summary>
        public required Engine.CommandRiskLevel RiskLevel { get; init; }
    }
}

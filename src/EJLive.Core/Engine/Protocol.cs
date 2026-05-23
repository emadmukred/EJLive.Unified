using System;

namespace EJLive.Core.Engine
{
    public static class SecurityConfig
    {
        public const string DEFAULT_KEY = "EJLiveEnterprise2026AESKey123456";
        public const string DEFAULT_IV = "EJLive2026AESIV!";
    }

    public static class NetworkConfig
    {
        public const int CONNECTION_TIMEOUT_MS = 30000;
        public const int SOCKET_BUFFER_SIZE = 65536;
        public const int PING_TIMEOUT_MS = 5000;
        public const int HEARTBEAT_INTERVAL_MS = 30000;
    }

    public static class NCRFiles
    {
        public static readonly string[] TargetFiles =
        {
            "EJDATA.LOG",
            "EJRCPY.LOG",
            "EJDATA.LOb"
        };
    }

    public static class FilePatterns
    {
        public const string GRG_EJ_PATTERN = "EJ*.*";
        public const string GRG_TRACE_PATTERN = "TRACE*.*";
        public const string WN_EJ_PATTERN = "*.ej";
    }

    public static class Protocol
    {
        public const string HANDSHAKE = "EJLIVE_HANDSHAKE";
        public const string HANDSHAKE_ACK = "EJLIVE_ACK";
        public const string HANDSHAKE_REJECT = "EJLIVE_REJECT";
        public const string HEARTBEAT = "HEARTBEAT";
        public const string HEARTBEAT_ACK = "HB_ACK";
        public const string DATA_JOURNAL = "EJDATA";
        public const string DATA_FILE = "EJFILE";
        public const string DATA_ACK = "DATA_ACK";
        public const string STATUS_REQUEST = "STATUS_REQ";
        public const string STATUS_RESPONSE = "STATUS_RES";
        public const string CMD_RESULT = "CMD_RESULT";
        public const string CMD_RESTART = "CMD_RESTART";
        public const string CMD_SCREENSHOT = "CMD_SCREENSHOT";
        public const string CMD_TIMESYNC = "CMD_SYNC_TIME";
        public const string CMD_SHUTDOWN = "CMD_SHUTDOWN";
        public const string CMD_CHANGE_PASSWORD = "CMD_CHANGE_PASSWORD";
        public const string CMD_SEND_IMAGE = "CMD_SEND_IMAGE";
        public const string CMD_UPDATE_CONFIG = "CMD_REMOTE_CONFIG";
        public const string CMD_GET_SYSINFO = "CMD_GET_STATS";
        public const string HEADER_END = "\n";
        public const string DATA_END = "\n<<END>>";

        public static string BuildMessage(string messageType, params string[] parts)
        {
            if (parts == null || parts.Length == 0)
                return messageType ?? string.Empty;

            var values = new string[parts.Length + 1];
            values[0] = messageType ?? string.Empty;
            Array.Copy(parts, 0, values, 1, parts.Length);
            return string.Join("|", values);
        }

        public static string[] ParseMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return new string[0];
            return message.Trim().Split('|');
        }
    }
}

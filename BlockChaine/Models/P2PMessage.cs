


namespace BlockChaine.Models
{
    public class PeerInfo
    {
        public string Host { get; set; }
        public int Port { get; set; }

        public PeerInfo(string host, int port)
        {
            Host = host;
            Port = port;
        }
    }

    public class P2PMessage
    {
        public string Type { get; set; }
        public string Data { get; set; }

        public P2PMessage(string type, string data)
        {
            Type = type;
            Data = data;
        }
    }
}

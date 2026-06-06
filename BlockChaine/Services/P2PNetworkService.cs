using BlockChaine.Models;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json;


namespace BlockChaine.Services
{
    public enum NetworkCommandType
    {
        NEW_BLOCK,
        REQUEST_CHAIN,
        CHAIN_RESPONSE
    }

    public class P2PNetworkService
    {
        private readonly int _port;
        private readonly BlockChainService _blockChainService;

        private const int MaxStrikes = 3;

        private ConcurrentDictionary<string, int> _peerStrikes;
        private List<PeerInfo> _peers;


        public P2PNetworkService(int port, BlockChainService blockChainService, List<PeerInfo> peerInfos)
        {
            _port = port;
            _blockChainService = blockChainService;

            _peerStrikes = new ConcurrentDictionary<string, int>();
            _peers = peerInfos;
        }

        public int GetMaxStrikes()
        {
            return MaxStrikes;
        }

        public void Start()
        {
            Task.Run(StartServerAsync);
        }

        public async Task StartServerAsync()
        {
            var listener = new TcpListener(System.Net.IPAddress.Any, _port);
            listener.Start();
            Debug.WriteLine($"P2P Network Service start on port {_port}");
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                Debug.WriteLine("New peer connected");
                _ = Task.Run(async () => await HandlePeerAsync(client));
            }
        }

        public async Task HandlePeerAsync(TcpClient client)
        {
            bool alreadyConnected;
            int strikes;
            string? remoteEndPoint = client.Client.RemoteEndPoint?.ToString();

            try
            {
                if (remoteEndPoint == null)
                {
                    Debug.WriteLine("Failed to get remote endpoint for peer");
                    client.Close();
                    return;
                }

                alreadyConnected = _peerStrikes.TryGetValue(remoteEndPoint, out strikes);

                if (alreadyConnected && strikes >= MaxStrikes)
                {
                    Debug.WriteLine($"[Firewall] Peer {client.Client.RemoteEndPoint} is banned due to too many strikes");
                    client.Close();
                    return;
                }

                if (!alreadyConnected)
                {
                    _peerStrikes[remoteEndPoint] = 0;
                }

                using var streem = client.GetStream();
                using var reader = new StreamReader(streem);

                var json = await reader.ReadLineAsync();

                if (json == null) return;

                P2PMessage? message = JsonSerializer.Deserialize<P2PMessage>(json);

                if (message != null)
                {
                    message.RemoteEndPoint = remoteEndPoint;
                    await CommandExecutor(message);
                } else
                {
                    _peerStrikes[remoteEndPoint]++;
                    Debug.WriteLine($"[Firewall Error A ] Invalid message from peer {remoteEndPoint}.");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task CommandExecutor(P2PMessage message)
        {

            if (message.Type == NetworkCommandType.NEW_BLOCK.ToString())
            {
                Block block;
                try
                {
                    block = JsonSerializer.Deserialize<Block>(message.Data)!;
                } catch (JsonException ex)
                {
                    _peerStrikes[message.RemoteEndPoint]++;
                    Debug.WriteLine($"[Firewall Error A ] Invalid message from peer {message.RemoteEndPoint}.");
                    return;
                }
                if (_blockChainService.TryAddBlockFromPeer(block))
                {
                    Debug.WriteLine("New block received from peer");
                }
                else
                {
                    _peerStrikes[message.RemoteEndPoint] += 2;
                    Debug.WriteLine($"[Firewall Error B ] Invalid block received from peer {message.RemoteEndPoint}.");
                }
            }

            if (message.Type == NetworkCommandType.REQUEST_CHAIN.ToString())
            {
                var blockchain = _blockChainService.Chain;
                var responseMessage = new P2PMessage(NetworkCommandType.CHAIN_RESPONSE.ToString(), JsonSerializer.Serialize(blockchain));
                var json = JsonSerializer.Serialize(responseMessage);
                foreach (var peer in _peers)
                    await SendMessageAsync(peer, json);
            }

            if (message.Type == NetworkCommandType.CHAIN_RESPONSE.ToString())
            {
                var blockchain = JsonSerializer.Deserialize<List<Block>>(message.Data);
                if (blockchain != null)
                {
                    _blockChainService.ResolveConflicts(blockchain);
                    Debug.WriteLine("Blockchain updated from peer response");
                }
                else
                {
                    _peerStrikes[message.RemoteEndPoint]++;
                    Debug.WriteLine($"[Firewall Error A ] Invalid message from peer {message.RemoteEndPoint}.");
                }
            }

        }

        public async Task AddPeerByPort(int port)
        {
            
            if (!_peers.Any(p => p.Port == port && p.Host == "localhost")) {
                _peers.Add(new PeerInfo("localhost", port));
                var chainMessage = new P2PMessage("REQUEST_CHAIN", "");
                await BroadCastMessageAsync(chainMessage);
            }
        }

        public async Task BroadcastBlockAsync(Block block)
        {
            P2PMessage message = new P2PMessage(NetworkCommandType.NEW_BLOCK.ToString(), JsonSerializer.Serialize(block));
            await BroadCastMessageAsync(message);
        }

        public async Task BroadCastMessageAsync(P2PMessage message)
        {
            var json = JsonSerializer.Serialize(message);
            foreach (var peer in _peers)
            {
                await SendMessageAsync(peer, json);
            }
        }

        private async Task SendMessageAsync(PeerInfo peer, string message)
        {
            try
            {
                using var client = new TcpClient(peer.Host, peer.Port);
                using var stream = client.GetStream();
                using var writer = new StreamWriter(stream) { AutoFlush = true };
                await writer.WriteLineAsync(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to send message to peer {peer.Host}:{peer.Port} - {ex.Message}");
            }
        }

        public List<(string RemoteEndPoint, int Strikes)> GetPeerStrikes()
        {
            return _peerStrikes.Select(kvp => (kvp.Key, kvp.Value)).ToList();
        }
    }
}

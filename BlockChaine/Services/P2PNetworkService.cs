using BlockChaine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlockChaine.Services
{
    public class P2PNetworkService
    {
        private readonly int _port;
        private readonly BlockChainService _blockChainService;

        private List<PeerInfo> _peers;


        public P2PNetworkService(int port, BlockChainService blockChainService, List<PeerInfo> peerInfos)
        {
            _port = port;
            _blockChainService = blockChainService;

            _peers = peerInfos;
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
            try
            {
                using var streem = client.GetStream();
                using var reader = new StreamReader(streem);

                var json = await reader.ReadLineAsync();

                if (json == null) return;

                var message = JsonSerializer.Deserialize<P2PMessage>(json);
                await CommandExecutor(message);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task CommandExecutor(P2PMessage message)
        {
            if (message.Type == "NEW_BLOCK")
            {
                var block = JsonSerializer.Deserialize<Block>(message.Data);
                if (block != null)
                {
                    _blockChainService.TryAddBlockFromPeer(block);
                    Debug.WriteLine("New block received from peer");
                }
            }
        }

        public async Task BroadcastBlockAsync(Block block)
        {
            var message = new P2PMessage("NEW_BLOCK", JsonSerializer.Serialize(block));
            var json = JsonSerializer.Serialize(message);
            foreach (var peer in _peers)
            {
                Debug.WriteLine($"Send message to {peer.Port}");
                SendMessageAsync(peer, json);
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
    }
}

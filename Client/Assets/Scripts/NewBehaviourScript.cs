using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Grpc.Core;
using System.IO;
//using Grpc.Net.Client;

public class NewBehaviourScript : MonoBehaviour
{
    System.Threading.CancellationTokenSource cancellationTokenSource;
    public string host = "kr-trunk.abyss.stairgames.com";
    public int port = 500;
    string _playerName = "unity1";
    string token;
    Grpc.Core.Channel _channel;
    game.PlayerNetwork.PlayerNetworkClient _playerNetworkClient;
    // Start is called before the first frame update
    async void Start()
    {
        cancellationTokenSource = new System.Threading.CancellationTokenSource();
        token = System.Guid.NewGuid().ToString("N");
        var credentials = CallCredentials.FromInterceptor((context, metadata) =>
        {
            if (!string.IsNullOrEmpty(token))
            {
                metadata.Add("Authorization", $"Bearer {token}");
            }
            return Task.CompletedTask;
        });
        //_channel = new Grpc.Core.Channel(target:"https://localhost:443", credentials: ChannelCredentials.Create(new SslCredentials(),credentials));
        _channel = new Grpc.Core.Channel(host: host, port: port, credentials: ChannelCredentials.Create(new SslCredentials(), credentials));
        //_channel = new Grpc.Core.Channel(host:host,port:port, credentials: ChannelCredentials.Insecure);
        _playerNetworkClient = new game.PlayerNetwork.PlayerNetworkClient(_channel);
        var client = new NetworkClient(_playerNetworkClient, _playerName, cancellationTokenSource.Token);
        await client.TestTask();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDestroy()
    {
        cancellationTokenSource.Cancel();
    }
}

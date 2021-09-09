using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
//using Grpc.Net.Client;

public class NewBehaviourScript : MonoBehaviour
{
    System.Threading.CancellationTokenSource cancellationTokenSource;
    string host = "localhost";
    int port = 5000;
    string _playerName = "unity1";
    Grpc.Core.Channel _channel;
    game.PlayerNetwork.PlayerNetworkClient _playerNetworkClient;
    // Start is called before the first frame update
     void Start()
    {
        cancellationTokenSource = new System.Threading.CancellationTokenSource();
        _channel = new Grpc.Core.Channel(host,port, Grpc.Core.ChannelCredentials.Insecure);
        _playerNetworkClient = new game.PlayerNetwork.PlayerNetworkClient(_channel);
        var client = new NetworkClient(_playerNetworkClient, _playerName, cancellationTokenSource.Token);
        _ = client.TestTask();
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

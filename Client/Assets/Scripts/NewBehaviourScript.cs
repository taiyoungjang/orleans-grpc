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
    public string host = "localhost.abyss.stairgames.com";
    public int port = 500;
    public long _regionIndex = 1;
    string _playerName = "unity1";
    string token;
    game.PlayerNetwork.PlayerNetworkClient _playerNetworkClient;
    // Start is called before the first frame update
    async void Start()
    {
        cancellationTokenSource = new System.Threading.CancellationTokenSource();
        var client = new NetworkClient(host, port, _playerName, _regionIndex, cancellationTokenSource.Token);
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

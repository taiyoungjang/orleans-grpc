using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Grpc.Net.Client;
namespace game
{
    public class Program
    {
        async public static Task Main(string[] args)
        {
            string playerName = "player-1";
            if(args.Length > 0)
            {
                playerName = args[0];
            }
            //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            //GrpcChannelOptions grpcChannelOptions = new GrpcChannelOptions() { Credentials = new };
            using var channel = GrpcChannel.ForAddress("http://localhost:5000");
            var playerNetworkClient = new game.PlayerNetwork.PlayerNetworkClient(channel);
            var client = new Client.Client(playerNetworkClient, playerName);
            _ = Task.Run( () => client.ListenTask() );
            await Task.Delay(TimeSpan.FromMinutes(10));
        }

    }
}

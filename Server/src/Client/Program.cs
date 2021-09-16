using System;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Grpc.Core;

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
            var client = new Client.Client(playerName);
            await client.TestTask();
            await Task.Delay(TimeSpan.FromSeconds(5));
        }

    }
}

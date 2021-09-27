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
            string firebaseId = "player1";
            string playerName = "player1";
            long regionIndex = new Random().Next(1,3);
            if(args.Length > 0)
            {
                playerName = args[0];
            }
            firebaseId = $"{playerName}-firebaseId";
            if (args.Length > 1 && long.TryParse(args[1], out regionIndex))
            {
                
            }
            var client = new Client.Client(firebaseId, playerName, regionIndex: regionIndex);
            await client.TestTask();
            await Task.Delay(TimeSpan.FromSeconds(5));
        }

    }
}

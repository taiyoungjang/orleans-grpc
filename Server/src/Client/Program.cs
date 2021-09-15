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
            string token = System.Guid.NewGuid().ToString("N");
            var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            {
                if (!string.IsNullOrEmpty(token))
                {
                    metadata.Add("Authorization", $"Bearer {token}");
                }
                return Task.CompletedTask;
            });
            //System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "abyss-kr.json");
            //var googleCredentials = await Grpc.Auth.GoogleGrpcCredentials.GetApplicationDefaultAsync();
            //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            //GrpcChannelOptions grpcChannelOptions = new GrpcChannelOptions() { Credentials = new };
            using var channel = GrpcChannel.ForAddress("https://kr-trunk.abyss.stairgames.com:5000", new GrpcChannelOptions
            {
                //Credentials = googleCredentials
                Credentials = ChannelCredentials.Create(new SslCredentials(), credentials)
                //Credentials = ChannelCredentials.Create(ChannelCredentials.Insecure,credentials)
            });
            var grpcServicesClient = new game.PlayerNetwork.PlayerNetworkClient(channel);
            var client = new Client.Client(grpcServicesClient, playerName);
            await client.TestTask();
            await Task.Delay(TimeSpan.FromSeconds(5));
        }

    }
}

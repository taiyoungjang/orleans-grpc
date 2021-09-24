using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using game;
using Grpc.Core;
using Grpc.Net.Client;

namespace Client
{
    public class Client
    {
        public static global::Google.Protobuf.WellKnownTypes.Empty s_empty = new();

        private game.PlayerNetwork.PlayerNetworkClient _playerNetworkClient;
        private string _name;
        long _regionIndex;
        private GrpcChannel _channel;
        private Metadata.Entry _bearer;
        private Metadata.Entry _regionMeta;
        public Client(string name, long regionIndex)
        {
            _regionIndex = regionIndex;
            var credentials = CallCredentials.FromInterceptor(AsyncAuthInterceptor);
            //System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "abyss-kr.json");
            //var googleCredentials = await Grpc.Auth.GoogleGrpcCredentials.GetApplicationDefaultAsync();
            //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            //GrpcChannelOptions grpcChannelOptions = new GrpcChannelOptions() { Credentials = new };
            _channel = GrpcChannel.ForAddress("https://localhost.abyss.stairgames.com:5000", new GrpcChannelOptions
            {
                //Credentials = googleCredentials
                Credentials = ChannelCredentials.Create(new SslCredentials(), credentials)
                //Credentials = ChannelCredentials.Create(ChannelCredentials.Insecure,credentials)
            });
            _playerNetworkClient = new game.PlayerNetwork.PlayerNetworkClient(_channel);
            _name = name;
            _bearer = null;
            _regionMeta = new Metadata.Entry("region",_regionIndex.ToString());
        }
        private void SetBearer(Guid guid)
        {
            if (_bearer == null)
            {
                _bearer = new Metadata.Entry("authorization", guid.ToString());
            }
        }

        private Task AsyncAuthInterceptor(AuthInterceptorContext context, Metadata metadata)
        {
            if (_bearer != null)
            {
                metadata.Add(_bearer);
            }
            if(_regionMeta != null)
            {
                metadata.Add(_regionMeta);
            }
            return Task.CompletedTask;
        }
        async public Task TestTask()
        {
            var uuid = await _playerNetworkClient.GetAuthAsync(new AuthRequest() { Name = _name });
            var guid = new Guid(uuid.Value.ToByteArray());
            SetBearer(guid);
            System.Console.WriteLine($"player:{_name} guid:{guid}");
            _ = Task.Run(() => CallRpcTask());
            var responseStream = _playerNetworkClient.ServerStreamServerEvents(s_empty).ResponseStream;
            while (await responseStream.MoveNext(default))
            {
                StreamServerEventsResponse current = responseStream.Current;
                switch (current.ActionCase)
                {
                    case StreamServerEventsResponse.ActionOneofCase.OnChat:
                        System.Console.WriteLine($"OnChat Room:{current.OnChat.RoomInfo} player:{current.OnChat.OtherPlayer} message:{current.OnChat.Message}");
                        break;
                    case StreamServerEventsResponse.ActionOneofCase.OnClosed:
                        System.Console.WriteLine($"OnClosed Reason:{current.OnClosed.Reason} ");
                        break;
                }
            }
            System.Console.WriteLine($"done.");
            await _channel.ShutdownAsync();
        }

        async private Task CallRpcTask()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            var getPlayerDataList = await _playerNetworkClient.GetRegionPlayerDataListAsync(s_empty);
            System.Console.WriteLine($"GetRegionPlayerDataListAsync: getPlayerDataList.Count:{getPlayerDataList.PlayerDataList_.Count}");

            var loginPlayerData = await _playerNetworkClient.LoginPlayerDataAsync(new RegionData() { RegionIndex = _regionIndex});
            System.Console.WriteLine($"LoginPlayerData: point:{loginPlayerData.Point}");
            do
            {
                var addPoint = new System.Random().Next(1, 100);
                var addPointAsync = await _playerNetworkClient.AddPointAsync(new() { AddPoint = addPoint });
                System.Console.WriteLine($"AddPointAsync: addPoint:{addPoint} AddedPoint:{addPointAsync.AddedPoint}");

                string message = $"blah-{Guid.NewGuid()}";

                var joinResult = await _playerNetworkClient.JoinChatRoomAsync(s_empty);
                System.Console.WriteLine($"JoinChatRoomAsync: RegionIndex:{_regionIndex}");

                System.Console.WriteLine($"ChatAsync: message:{message}");
                await _playerNetworkClient.ChatAsync(new() { Message = message} );
                await Task.Delay(TimeSpan.FromSeconds(1));

                var leave = await _playerNetworkClient.LeaveChatRoomAsync(s_empty);
                System.Console.WriteLine($"LeaveChatRoomAsync: room:{_regionIndex} {leave.Success}");
            } while (true);
        }
    }
}

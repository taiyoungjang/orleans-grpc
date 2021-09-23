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
        private GrpcChannel _channel;
        private Metadata.Entry _bearer;
        public Client(string name)
        {
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
                    case StreamServerEventsResponse.ActionOneofCase.OnJoin:
                        System.Console.WriteLine($"OnJoin Room:{current.OnJoin.RoomInfo} player:{current.OnJoin.OtherPlayer}");
                        break;
                    case StreamServerEventsResponse.ActionOneofCase.OnLeave:
                        System.Console.WriteLine($"OnLeave Room:{current.OnLeave.RoomInfo} player:{current.OnLeave.OtherPlayer}");
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
            var getPlayerData = await _playerNetworkClient.GetPlayerDataAsync(s_empty);
            System.Console.WriteLine($"GetPlayerDataAsync: point:{getPlayerData.Point}");
            do
            {
                var addPoint = new System.Random().Next(1, 100);
                var addPointAsync = await _playerNetworkClient.AddPointAsync(new() { AddPoint = addPoint });
                System.Console.WriteLine($"AddPointAsync: addPoint:{addPoint} AddedPoint:{addPointAsync.AddedPoint}");

                var availableRoomResult = await _playerNetworkClient.GetAvailableRoomListAsync(s_empty);
                System.Console.WriteLine($"GetAvailableRoomListAsync: {string.Join(',', availableRoomResult.Rooms.Select(t => t.Name))}");
                string roomName = availableRoomResult.Rooms.Select(t => t.Name).FirstOrDefault();
                string message = $"blah-{Guid.NewGuid()}";
                if (string.IsNullOrEmpty(roomName))
                {
                    roomName = $"room-{Guid.NewGuid()}";
                }

                var joinResult = await _playerNetworkClient.JoinAsync(new() { Room = roomName });
                System.Console.WriteLine($"JoinAsync: {roomName} already joined: count:{joinResult.Players.Count} names:{string.Join(',', joinResult.Players)}");

                System.Console.WriteLine($"ChatAsync: {roomName}");
                await _playerNetworkClient.ChatAsync(new() { Room = roomName, Message = message });

                var result = await _playerNetworkClient.GetJoinedRoomListAsync(s_empty);
                System.Console.WriteLine($"GetJoinedRoomListAsync: {string.Join(',', result.Rooms.Select(t => t.Name))}");

                var leave = await _playerNetworkClient.LeaveAsync(new() { Room = roomName });
                System.Console.WriteLine($"LeaveAsync: room:{roomName} {leave.Success}");
            } while (true);
        }
    }
}

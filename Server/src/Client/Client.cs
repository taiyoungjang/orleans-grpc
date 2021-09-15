using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using game;

namespace Client
{
    public class Client
    {
        public static global::Google.Protobuf.WellKnownTypes.Empty s_empty = new();

        private game.PlayerNetwork.PlayerNetworkClient _playerNetworkClient;
        private string _name;
        public Client(game.PlayerNetwork.PlayerNetworkClient playerNetworkClient, string name)
        {
            _playerNetworkClient = playerNetworkClient;
            _name = name;
        }
        async public Task TestTask()
        {
            var uuid = await _playerNetworkClient.GetAuthAsync( new AuthRequest() { Name = _name });
            System.Console.WriteLine($"player:{_name} uuid:{new Guid(uuid.Value.ToByteArray())}");
            _ = Task.Run(() => CallRpcTask());
            var responseStream = _playerNetworkClient.GetAsyncStreams(s_empty).ResponseStream;
            while (await responseStream.MoveNext(default))
            {
                GrpcStreamResponse current = responseStream.Current;
                switch (current.ActionCase)
                {
                    case GrpcStreamResponse.ActionOneofCase.OnChat:
                        System.Console.WriteLine($"OnChat Room:{current.OnChat.RoomInfo} player:{current.OnChat.OtherPlayer} message:{current.OnChat.Message}");
                        break;
                    case GrpcStreamResponse.ActionOneofCase.OnJoin:
                        System.Console.WriteLine($"OnJoin Room:{current.OnJoin.RoomInfo} player:{current.OnJoin.OtherPlayer}");
                        break;
                    case GrpcStreamResponse.ActionOneofCase.OnLeave:
                        System.Console.WriteLine($"OnLeave Room:{current.OnLeave.RoomInfo} player:{current.OnLeave.OtherPlayer}");
                        break;
                    case GrpcStreamResponse.ActionOneofCase.OnClosed:
                        System.Console.WriteLine($"OnClosed Reason:{current.OnClosed.Reason} ");
                        break;
                }
            }
            System.Console.WriteLine($"done.");
        }
        async private Task CallRpcTask()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            var getPlayerData = await _playerNetworkClient.GetPlayerDataAsync(s_empty);
            System.Console.WriteLine($"GetPlayerDataAsync: point:{getPlayerData.Point}");
            var addPoint = new System.Random().Next(1, 100);
            var addPointAsync = await _playerNetworkClient.AddPointAsync(new() { AddPoint = addPoint });
            System.Console.WriteLine($"AddPointAsync: addPoint:{addPoint} AddedPoint:{addPointAsync.AddedPoint}");

            var availableRoomResult = await _playerNetworkClient.GetAvailableRoomListAsync(s_empty);
            System.Console.WriteLine($"GetAvailableRoomListAsync: {string.Join(',', availableRoomResult.Rooms.Select(t => t.Name))}");
            string roomName = availableRoomResult.Rooms.Select(t=>t.Name).FirstOrDefault();
            string message = $"blah-{Guid.NewGuid()}";
            if(string.IsNullOrEmpty(roomName))
            {
                roomName = $"room-{Guid.NewGuid()}";
            }

            var joinResult = await _playerNetworkClient.JoinAsync(new() { Room = roomName });
            System.Console.WriteLine($"JoinAsync: {roomName} already joined: count:{joinResult.Players.Count} names:{string.Join(',',joinResult.Players)}");

            System.Console.WriteLine($"ChatAsync: {roomName}");
            await _playerNetworkClient.ChatAsync(new() { Room = roomName, Message = message });

            var result = await _playerNetworkClient.GetJoinedRoomListAsync(s_empty);
            System.Console.WriteLine($"GetJoinedRoomListAsync: {string.Join(',', result.Rooms.Select(t => t.Name))}");

            var leave = await _playerNetworkClient.LeaveAsync(new() {Room = roomName });
            System.Console.WriteLine($"LeaveAsync: room:{roomName} {leave.Success}");
        }
    }
}

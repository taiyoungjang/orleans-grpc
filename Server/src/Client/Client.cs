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

        Grpc.Core.CallOptions _callOptions;
        private game.PlayerNetwork.PlayerNetworkClient _playerNetworkClient;
        private string _name;
        public Client(game.PlayerNetwork.PlayerNetworkClient playerNetworkClient, string name)
        {
            _playerNetworkClient = playerNetworkClient;
            _name = name;
        }
        async public Task ListenTask()
        {
            var uuid = await _playerNetworkClient.GetAuthAsync( new AuthRequest() { Name = _name });
            System.Console.WriteLine($"player:{_name} uuid:{new Guid(uuid.Value.ToByteArray())}");
            Grpc.Core.Metadata headers = new Grpc.Core.Metadata();
            headers.Add(new Grpc.Core.Metadata.Entry("uuid-bin", uuid.Value.ToByteArray()));
            headers.Add(new Grpc.Core.Metadata.Entry("name", _name));
            this._callOptions = new Grpc.Core.CallOptions(headers: headers);
            _ = Task.Run(() => CallRpcTask());
            var responseStream = _playerNetworkClient.GetAsyncStreams(s_empty,_callOptions).ResponseStream;
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
                }
            }
        }
        async private Task CallRpcTask()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            var getPlayerData = await _playerNetworkClient.GetPlayerDataAsync(s_empty,_callOptions);
            System.Console.WriteLine($"GetPlayerDataAsync: point:{getPlayerData.Point}");
            var addPoint = new System.Random().Next(1, 100);
            var addPointAsync = await _playerNetworkClient.AddPointAsync(new() { AddPoint = addPoint }, _callOptions);
            System.Console.WriteLine($"AddPointAsync: addPoint:{addPoint} AddedPoint:{addPointAsync.AddedPoint}");

            var availableRoomResult = await _playerNetworkClient.GetAvailableRoomListAsync(s_empty, _callOptions);
            System.Console.WriteLine($"GetAvailableRoomListAsync: {string.Join(',', availableRoomResult.Rooms.Select(t => t.Name))}");
            string roomName = availableRoomResult.Rooms.Select(t=>t.Name).FirstOrDefault();
            string message = $"blah-{Guid.NewGuid()}";
            if(string.IsNullOrEmpty(roomName))
            {
                roomName = $"room-{Guid.NewGuid()}";
            }

            var joinResult = await _playerNetworkClient.JoinAsync(new() { Room = roomName }, _callOptions);
            System.Console.WriteLine($"JoinAsync: {roomName} already joined: count:{joinResult.Players.Count} names:{string.Join(',',joinResult.Players)}");

            System.Console.WriteLine($"ChatAsync: {roomName}");
            await _playerNetworkClient.ChatAsync(new() { Room = roomName, Message = message }, _callOptions);

            var result = await _playerNetworkClient.GetJoinedRoomListAsync(s_empty, _callOptions);
            System.Console.WriteLine($"GetJoinedRoomListAsync: {string.Join(',', result.Rooms.Select(t => t.Name))}");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using game;

public class NetworkClient
{
    public static global::Google.Protobuf.WellKnownTypes.Empty s_empty = new global::Google.Protobuf.WellKnownTypes.Empty();

    Grpc.Core.CallOptions _callOptions;
    private game.PlayerNetwork.PlayerNetworkClient _playerNetworkClient;
    private string _name;
    private System.Threading.CancellationToken _token;
    public NetworkClient(game.PlayerNetwork.PlayerNetworkClient playerNetworkClient, string name, System.Threading.CancellationToken token
        )
    {
        _playerNetworkClient = playerNetworkClient;
        _name = name;
        _token = token;
    }
    async public Task TestTask()
    {
        var uuid = await _playerNetworkClient.GetAuthAsync(new AuthRequest() { Name = _name });
        UnityEngine.Debug.Log($"player:{_name} uuid:{new Guid(uuid.Value.ToByteArray())}");
        Grpc.Core.Metadata headers = new Grpc.Core.Metadata();
        headers.Add(new Grpc.Core.Metadata.Entry("uuid-bin", uuid.Value.ToByteArray()));
        headers.Add(new Grpc.Core.Metadata.Entry("name", _name));
        this._callOptions = new Grpc.Core.CallOptions(headers: headers);
        _ = Task.Run(() => CallRpcTask());
        var responseStream = _playerNetworkClient.GetAsyncStreams(s_empty, _callOptions).ResponseStream;
        while (await responseStream.MoveNext(_token))
        {
            GrpcStreamResponse current = responseStream.Current;
            switch (current.ActionCase)
            {
                case GrpcStreamResponse.ActionOneofCase.OnChat:
                    UnityEngine.Debug.Log($"OnChat Room:{current.OnChat.RoomInfo} player:{current.OnChat.OtherPlayer} message:{current.OnChat.Message}");
                    break;
                case GrpcStreamResponse.ActionOneofCase.OnJoin:
                    UnityEngine.Debug.Log($"OnJoin Room:{current.OnJoin.RoomInfo} player:{current.OnJoin.OtherPlayer}");
                    break;
                case GrpcStreamResponse.ActionOneofCase.OnLeave:
                    UnityEngine.Debug.Log($"OnLeave Room:{current.OnLeave.RoomInfo} player:{current.OnLeave.OtherPlayer}");
                    break;
                case GrpcStreamResponse.ActionOneofCase.OnClosed:
                    UnityEngine.Debug.Log($"OnClosed Reason:{current.OnClosed.Reason} ");
                    break;
            }
        }
        UnityEngine.Debug.Log($"done.");
    }
    async private Task CallRpcTask()
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        var getPlayerData = await _playerNetworkClient.GetPlayerDataAsync(s_empty, _callOptions);
        UnityEngine.Debug.Log($"GetPlayerDataAsync: point:{getPlayerData.Point}");
        var addPoint = new System.Random().Next(1, 100);
        var addPointAsync = await _playerNetworkClient.AddPointAsync(new AddPointRequest() { AddPoint = addPoint }, _callOptions);
        UnityEngine.Debug.Log($"AddPointAsync: addPoint:{addPoint} AddedPoint:{addPointAsync.AddedPoint}");

        var availableRoomResult = await _playerNetworkClient.GetAvailableRoomListAsync(s_empty, _callOptions);
        UnityEngine.Debug.Log($"GetAvailableRoomListAsync: {string.Join(",", availableRoomResult.Rooms.Select(t => t.Name))}");
        string roomName = availableRoomResult.Rooms.Select(t => t.Name).FirstOrDefault();
        string message = $"blah-{Guid.NewGuid()}";
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = $"room-{Guid.NewGuid()}";
        }

        var joinResult = await _playerNetworkClient.JoinAsync(new JoinRequest() { Room = roomName }, _callOptions);
        UnityEngine.Debug.Log($"JoinAsync: {roomName} already joined: count:{joinResult.Players.Count} names:{string.Join(",", joinResult.Players)}");

        UnityEngine.Debug.Log($"ChatAsync: {roomName}");
        await _playerNetworkClient.ChatAsync(new ChatRequest() { Room = roomName, Message = message }, _callOptions);

        var result = await _playerNetworkClient.GetJoinedRoomListAsync(s_empty, _callOptions);
        UnityEngine.Debug.Log($"GetJoinedRoomListAsync: {string.Join(",", result.Rooms.Select(t => t.Name))}");
    }
}

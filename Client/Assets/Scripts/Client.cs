using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using game;
using Grpc.Core;
public class NetworkClient
{
    public static global::Google.Protobuf.WellKnownTypes.Empty s_empty = new global::Google.Protobuf.WellKnownTypes.Empty();

    private game.PlayerNetwork.PlayerNetworkClient _playerNetworkClient;
    private string _name;
    private long _regionIndex;
    private System.Threading.CancellationToken _token;
    private Metadata.Entry _bearer;
    private Metadata.Entry _regionMeta;
    private Grpc.Core.Channel _channel;
    public NetworkClient(string host, int port, string name, long regionIndex, System.Threading.CancellationToken token)
    {
        _regionIndex = regionIndex;
        var credentials = CallCredentials.FromInterceptor(AsyncAuthInterceptor);
        //System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "abyss-kr.json");
        //var googleCredentials = await Grpc.Auth.GoogleGrpcCredentials.GetApplicationDefaultAsync();
        //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        //GrpcChannelOptions grpcChannelOptions = new GrpcChannelOptions() { Credentials = new };
        _channel = new Grpc.Core.Channel(host: host, port: port, credentials: ChannelCredentials.Create(new SslCredentials(), credentials));
        _playerNetworkClient = new game.PlayerNetwork.PlayerNetworkClient(_channel);
        _name = name;
        _token = token;
        _bearer = null;
        _regionMeta = new Metadata.Entry("region", _regionIndex.ToString());
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
        if (_regionMeta != null)
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
        _ = Task.Run(() => CallRpcTask());
        var responseStream = _playerNetworkClient.ServerStreamServerEvents(s_empty).ResponseStream;
        while (await responseStream.MoveNext(_token))
        {
            StreamServerEventsResponse current = responseStream.Current;
            switch (current.ActionCase)
            {
                case StreamServerEventsResponse.ActionOneofCase.OnChat:
                    UnityEngine.Debug.Log($"OnChat Room:{current.OnChat.RoomInfo} player:{current.OnChat.OtherPlayer} message:{current.OnChat.Message}");
                    break;
                case StreamServerEventsResponse.ActionOneofCase.OnClosed:
                    UnityEngine.Debug.Log($"OnClosed Reason:{current.OnClosed.Reason} ");
                    break;
            }
        }
        UnityEngine.Debug.Log($"done.");
    }
    async private Task CallRpcTask()
    {
        await Task.Delay(TimeSpan.FromSeconds(1));
        var getPlayerDataList = await _playerNetworkClient.GetRegionPlayerDataListAsync(s_empty);
        UnityEngine.Debug.Log($"GetRegionPlayerDataListAsync: getPlayerDataList.Count:{getPlayerDataList.PlayerDataList_.Count}");
        var loginPlayerData = await _playerNetworkClient.LoginPlayerDataAsync(new RegionData() { RegionIndex = _regionIndex });
        UnityEngine.Debug.Log($"GetPlayerDataAsync: point:{loginPlayerData.Point}");
        var addPoint = new System.Random().Next(1, 100);
        var addPointAsync = await _playerNetworkClient.AddPointAsync(new AddPointRequest() { AddPoint = addPoint });
        UnityEngine.Debug.Log($"AddPointAsync: addPoint:{addPoint} AddedPoint:{addPointAsync.AddedPoint}");

        string message = $"blah-{Guid.NewGuid()}";

        var joinResult = await _playerNetworkClient.JoinChatRoomAsync(s_empty);
        UnityEngine.Debug.Log($"JoinChatRoomAsync: RegionIndex:{_regionIndex}");

        UnityEngine.Debug.Log($"ChatAsync: {message}");
        await _playerNetworkClient.ChatAsync(new ChatRequest() {  Message = message });

    }
}

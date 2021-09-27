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
    private game.PlayerData _playerData;

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
        var getPlayerDataList = await _playerNetworkClient.GetRegionPlayerDataListAsync(s_empty);
        System.Console.WriteLine($"GetRegionPlayerDataListAsync: getPlayerDataList.Count:{getPlayerDataList.PlayerDataList_.Count}");

        _playerData = await _playerNetworkClient.LoginPlayerDataAsync(new RegionData() { RegionIndex = _regionIndex });
        System.Console.WriteLine($"LoginPlayerData: Stage:{_playerData.Stage}");

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
        var random = new System.Random();
        do
        {
            int addStage = random.Next(1, 4);
            _playerData.Stage += addStage;
            var updateStageAsync = await _playerNetworkClient.UpdateStageAsync(new UpdateStageRequest() { Stage = _playerData.Stage });
            System.Console.WriteLine($"UpdateStageAsync: addStage:{addStage} Stage:{updateStageAsync.Stage}");

            if (random.Next(0, 1) == 0)
            {
                string message = $"blah-{Guid.NewGuid()}";
                UnityEngine.Debug.Log($"ChatAsync: message:{message}");
                await _playerNetworkClient.ChatAsync(new ChatRequest() { Message = message });
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            var rankings = await _playerNetworkClient.GetTopRankListAsync(s_empty);
            foreach (var rank in rankings.Ranks)
            {
                UnityEngine.Debug.Log($"rank:{rank.Rank} name:{rank.Name} Stage:{rank.Stage}");
            }
        } while (true);
    }
}

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
    private string _firebaseId;
    private string _name;
    private long _regionIndex;
    private System.Threading.CancellationToken _token;
    private Metadata.Entry _regionMeta;
    private Metadata.Entry _otpMeta;
    private Grpc.Core.Channel _channel;
    private game.PlayerData _playerData;

    public NetworkClient(string host, int port, string firebaseId, string name, long regionIndex, System.Threading.CancellationToken token)
    {
        _regionIndex = regionIndex;
        var credentials = CallCredentials.FromInterceptor(AsyncAuthInterceptor);
        //System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "abyss-kr.json");
        //var googleCredentials = await Grpc.Auth.GoogleGrpcCredentials.GetApplicationDefaultAsync();
        //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        //GrpcChannelOptions grpcChannelOptions = new GrpcChannelOptions() { Credentials = new };
        _channel = new Grpc.Core.Channel(host: host, port: port, credentials: ChannelCredentials.Create(new SslCredentials(), credentials));
        _playerNetworkClient = new game.PlayerNetwork.PlayerNetworkClient(_channel);
        _firebaseId = firebaseId;
        _name = name;
        _token = token;
        _regionMeta = new Metadata.Entry("region", _regionIndex.ToString());
    }
    private void SetOtp(Guid otp)
    {
        if (_otpMeta == null)
        {
            _otpMeta = new Metadata.Entry("otp", otp.ToString());
        }
    }

    private Task AsyncAuthInterceptor(AuthInterceptorContext context, Metadata metadata)
    {
        if (_otpMeta != null)
        {
            metadata.Add(_otpMeta);
        }
        if (_regionMeta != null)
        {
            metadata.Add(_regionMeta);
        }
        return Task.CompletedTask;
    }
    async public Task TestTask()
    {
        AuthResponse authResponse = await _playerNetworkClient.GetAuthAsync(new AuthRequest() { FirebaseId = _firebaseId });
        var otp = new Guid(authResponse.Otp.Value.ToByteArray());
        SetOtp(otp);
        var getPlayerDataList = await _playerNetworkClient.GetRegionPlayerDataListAsync(s_empty);
        System.Console.WriteLine($"GetRegionPlayerDataListAsync: getPlayerDataList.Count:{getPlayerDataList.PlayerDataList_.Count}");
        if (!getPlayerDataList.PlayerDataList_.Any(t => t.RegionIndex == _regionIndex))
        {
            var createPlayerAsyncResult = await _playerNetworkClient.CreatePlayerAsync(new CreatePlayerRequest() { RegionIndex = _regionIndex, Name = _name });
            System.Console.WriteLine($"name:{_name} createPlayerAsyncResult:{createPlayerAsyncResult.ErrorCode}");
        }

        _playerData = await _playerNetworkClient.LoginPlayerDataAsync(new RegionData() { RegionIndex = _regionIndex });
        System.Console.WriteLine($"LoginPlayerData: Stage:{_playerData.Stage}");

        System.Threading.SynchronizationContext.Current.Post( async _ => await CallRpcTask(),null);
        var responseStream = _playerNetworkClient.ServerStreamServerEvents(s_empty).ResponseStream;
        while (await responseStream.MoveNext(_token))
        {
            StreamServerEventsResponse current = responseStream.Current;
            switch (current.ActionCase)
            {
                case StreamServerEventsResponse.ActionOneofCase.OnChat:
                    UnityEngine.Debug.Log($"OnChat RegionIndex:{current.OnChat.RegionIndex} player:{current.OnChat.OtherPlayer} message:{current.OnChat.Message}");
                    break;
                case StreamServerEventsResponse.ActionOneofCase.OnClosed:
                    UnityEngine.Debug.Log($"OnClosed Reason:{current.OnClosed.Reason} ");
                    break;
                case StreamServerEventsResponse.ActionOneofCase.OnUpdateRanking:
                    {
                        var rankings = await _playerNetworkClient.GetTopRankListAsync(s_empty);
                        foreach (var pair in rankings.TopRanks)
                        {
                            UnityEngine.Debug.Log($"rank:{pair.Value.Rank} name:{pair.Key} Value:{pair.Value.Value}");
                        }
                        UnityEngine.Debug.Log($"myRank rank:{rankings.MyRank.Rank} Value:{rankings.MyRank.Value}");
                    }
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
            System.Console.WriteLine($"UpdateStageAsync: addStage:{addStage} ErrorCode:{updateStageAsync.ErrorCode}");

            if (random.Next(0, 1) == 0)
            {
                string message = $"blah-{Guid.NewGuid()}";
                UnityEngine.Debug.Log($"ChatAsync: message:{message}");
                await _playerNetworkClient.ChatAsync(new ChatRequest() { Message = message });
                await Task.Delay(TimeSpan.FromSeconds(1),_token);
            }
        } while (!_token.IsCancellationRequested);
    }
}

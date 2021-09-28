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
        private string _firebaseId;
        private string _name;
        long _regionIndex;
        private GrpcChannel _channel;
        private Metadata.Entry _regionMeta;
        private Metadata.Entry _otpMeta;
        private game.PlayerData _playerData;
        public Client(string firebaseId, string name, long regionIndex)
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
            _firebaseId = firebaseId;
            _name = name;
            _regionMeta = new Metadata.Entry("region",_regionIndex.ToString());
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
            if(_regionMeta != null)
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
            if(!getPlayerDataList.PlayerDataList_.Any(t=>t.RegionIndex == _regionIndex) )
            {
                var createPlayerAsyncResult = await _playerNetworkClient.CreatePlayerAsync(new CreatePlayerRequest() { RegionIndex = _regionIndex, Name = _name });
                System.Console.WriteLine($"name:{_name} createPlayerAsyncResult:{createPlayerAsyncResult.ErrorCode}");
            }

            _playerData = await _playerNetworkClient.LoginPlayerDataAsync(new RegionData() { RegionIndex = _regionIndex });
            System.Console.WriteLine($"LoginPlayerData: Stage:{_playerData.Stage}");


            System.Console.WriteLine($"player:{_name} otp:{otp}");
            _ = Task.Run(() => CallRpcTask());
            var responseStream = _playerNetworkClient.ServerStreamServerEvents(s_empty).ResponseStream;
            while (await responseStream.MoveNext(default))
            {
                StreamServerEventsResponse current = responseStream.Current;
                switch (current.ActionCase)
                {
                    case StreamServerEventsResponse.ActionOneofCase.OnChat:
                        System.Console.WriteLine($"OnChat RegionIndex:{current.OnChat.RegionIndex} player:{current.OnChat.OtherPlayer} message:{current.OnChat.Message}");
                        break;
                    case StreamServerEventsResponse.ActionOneofCase.OnClosed:
                        System.Console.WriteLine($"OnClosed Reason:{current.OnClosed.Reason} ");
                        break;
                    case StreamServerEventsResponse.ActionOneofCase.OnUpdateRanking:
                        {
                            var rankings = await _playerNetworkClient.GetTopRankListAsync(s_empty);
                            foreach (var pair in rankings.TopRanks)
                            {
                                System.Console.WriteLine($"rank:{pair.Value.Rank} name:{pair.Key} Value:{pair.Value.Value}");
                            }
                            System.Console.WriteLine($"myRank rank:{rankings.MyRank.Rank} Value:{rankings.MyRank.Value}");
                        }
                        break;
                }
            }
            System.Console.WriteLine($"done.");
            await _channel.ShutdownAsync();
        }

        async private Task CallRpcTask()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            var random = new System.Random();
            try
            {
                do
                {
                    int addStage = random.Next(1, 4);
                    _playerData.Stage += addStage;
                    var updateStageAsync = await _playerNetworkClient.UpdateStageAsync(new() { Stage = _playerData.Stage });
                    System.Console.WriteLine($"UpdateStageAsync: addStage:{addStage} ErrorCode:{updateStageAsync.ErrorCode}");

                    if (random.Next(0, 1) == 0)
                    {
                        string message = $"blah-{Guid.NewGuid()}";
                        System.Console.WriteLine($"ChatAsync: message:{message}");
                        await _playerNetworkClient.ChatAsync(new() { Message = message });
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }

                    var mails = await _playerNetworkClient.GetMailAsync(s_empty);
                    System.Console.WriteLine($"GetMailAsync: Count:{mails.Mails.Mails.Count}");
                    if (mails.Mails.Mails.Count > 0)
                    {
                        var first = mails.Mails.Mails.First().Value.Uuid;
                        var DeleteMailAsyncResult = await _playerNetworkClient.DeleteMailAsync(new DeleteMailRequest() { Uuid = first });
                        System.Console.WriteLine($"DeleteMailAsync: {DeleteMailAsyncResult.ErrorCode}");
                    }

                } while (true);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }
    }
}

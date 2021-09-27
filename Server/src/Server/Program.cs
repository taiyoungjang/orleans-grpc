using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;

namespace Server
{
    public class Program
    {
        public static string s_streamProviderName = "AzureQueueProvider";
        public static List<long> s_regionList = new List<long>() { 1,2 };
        [STAThread]
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            string connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
            return Host.CreateDefaultBuilder(args)
                .UseOrleans(siloBuilder =>
                {
                    siloBuilder.ConfigureApplicationParts(manager =>
                    {
                        manager.AddApplicationPart(typeof(PlayerGrain).Assembly).WithReferences();
                    });
                    siloBuilder.UseLocalhostClustering();
                    siloBuilder.AddAzureQueueStreams(s_streamProviderName, configurator =>
                    {
                        configurator.ConfigureAzureQueue(
                          ob => ob.Configure(options =>
                          {
                              options.ConnectionString = connectionString;
                              options.QueueNames = new List<string>();
                              options.QueueNames.Add(PlayerGrain.GetRegionQueueStreamNamespace(1));
                              options.QueueNames.Add(PlayerGrain.GetRegionQueueStreamNamespace(2));
                          }));
                        configurator.ConfigureCacheSize(1024);
                        configurator.ConfigurePullingAgent(ob => ob.Configure(options =>
                        {
                            options.GetQueueMsgsTimerPeriod = TimeSpan.FromMilliseconds(500);
                        }));
                    });
                    siloBuilder.AddAzureBlobGrainStorage("PubSubStore", options =>
                    {
                        options.UseJson = true;
                        options.ConnectionString = connectionString;
                    });
                    //siloBuilder.AddMemoryGrainStorage("PubSubStore");
                    //.AddSimpleMessageStreamProvider(RoomGrain.s_streamProviderName, options =>
                    //{
                    //    options.FireAndForgetDelivery = true;
                    //})
                    //.AddSimpleMessageStreamProvider(PlayerGrain.s_streamProviderName, options =>
                    //{
                    //    options.FireAndForgetDelivery = true;
                    //});

                    //var redisAddress = $"{Environment.GetEnvironmentVariable("REDIS")}:6379";
                    //siloBuilder.AddRedisGrainStorage("player", options => options.ConnectionString = redisAddress);
                    siloBuilder.AddAzureBlobGrainStorage("player", options =>
                    {
                        options.UseJson = true;
                        options.ConnectionString = connectionString;
                    });
                    siloBuilder.AddAzureBlobGrainStorage("stagerank", options =>
                    {
                        options.UseJson = true;
                        options.ConnectionString = connectionString;
                    });
                    siloBuilder.AddAzureBlobGrainStorage("stageupdaterank", options =>
                    {
                        options.UseJson = true;
                        options.ConnectionString = connectionString;
                    });
                    siloBuilder.UseAzureTableReminderService(options =>
                    {
                        options.ConnectionString = connectionString;
                    });
                    //siloBuilder.AddAzureTableGrainStorage("PubSubStore", options =>
                    //{
                    //    options.UseJson = true;
                    //    options.DeleteStateOnClear = true;
                    //    options.ConnectionString = connectionString;
                    //});

                    //siloBuilder.ConfigureServices(x => x.AddSingleton<IExternalSerializer, ProtobufSerializer>());
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(options =>
                    {
                        var certificate = LoadPemCertificate("cert.crt", "key.pem");
                        options.ListenAnyIP(5000, o => o.UseHttps(certificate));
                    });
                    webBuilder.UseStartup<Startup>();
                });
        }
        public static X509Certificate2 LoadPemCertificate(string certificatePath, string privateKeyPath)
        {
            using var publicKey = new X509Certificate2(certificatePath);

            var privateKeyText = File.ReadAllText(privateKeyPath);
            var privateKeyBlocks = privateKeyText.Split("-", StringSplitOptions.RemoveEmptyEntries);
            var privateKeyBytes = Convert.FromBase64String(privateKeyBlocks[1]);
            using var rsa = RSA.Create();

            if (privateKeyBlocks[0] == "BEGIN PRIVATE KEY")
            {
                rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
            }
            else if (privateKeyBlocks[0] == "BEGIN RSA PRIVATE KEY")
            {
                rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
            }

            var keyPair = publicKey.CopyWithPrivateKey(rsa);
            return new X509Certificate2(keyPair.Export(X509ContentType.Pfx));
        }
    }
}

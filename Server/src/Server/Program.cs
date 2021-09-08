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
using Orleans.Serialization;

namespace Server
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>

            Host.CreateDefaultBuilder(args)
                .UseOrleans(siloBuilder =>
                {
                    siloBuilder.ConfigureApplicationParts(manager =>
                    {
                        manager.AddApplicationPart(typeof(PlayerGrain).Assembly).WithReferences();
                    });
                    siloBuilder.UseLocalhostClustering()
                    .AddMemoryGrainStorage("PubSubStore")
                    .AddSimpleMessageStreamProvider(PlayerGrain.s_streamProviderName, options =>
                    {
                        options.FireAndForgetDelivery = true;
                    });
                    var redisAddress = $"{Environment.GetEnvironmentVariable("REDIS")}:6379";
                    siloBuilder.AddRedisGrainStorage("player", options => options.ConnectionString = redisAddress);

                    //siloBuilder.ConfigureServices(x => x.AddSingleton<IExternalSerializer, ProtobufSerializer>());
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

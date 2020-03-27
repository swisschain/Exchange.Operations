using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Swisschain.Sdk.Server.Common;
using Swisschain.Sdk.Server.Logging;

namespace Operations
{
    public class Program
    {
        private sealed class RemoteSettingsConfig
        {
            public IReadOnlyCollection<string> RemoteSettingsUrls { get; set; } = Array.Empty<string>();
        }

        public static void Main(string[] args)
        {
            Console.Title = "Exchange Operations";

            var remoteSettingsConfig = ApplicationEnvironment.Config.Get<RemoteSettingsConfig>();

            using var loggerFactory = LogConfigurator.Configure(
                "Exchange", 
                ApplicationEnvironment.Config["SeqUrl"],
                remoteSettingsConfig.RemoteSettingsUrls);
            
            var logger = loggerFactory.CreateLogger<Program>();

            try
            {
                logger.LogInformation("Application is being started");

                CreateHostBuilder(loggerFactory, remoteSettingsConfig).Build().Run();

                logger.LogInformation("Application has been stopped");
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Application has been terminated unexpectedly");
            }
        }

        private static IHostBuilder CreateHostBuilder(ILoggerFactory loggerFactory, RemoteSettingsConfig remoteSettingsConfig) =>
            new HostBuilder()
                .SwisschainService<Startup>(options =>
                {
                    options.UseLoggerFactory(loggerFactory);
                    options.AddWebJsonConfigurationSources(remoteSettingsConfig.RemoteSettingsUrls);
                });
    }
}

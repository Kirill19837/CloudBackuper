using DbBackuper.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace DbBackuper;

internal class Program
{
    static async Task Main(string[] args) 
    {
        var host = CreateHostBuilder(args).Build();
        
        var workerInstance = host.Services.GetRequiredService<Worker>();
        await workerInstance.ExecuteAsync();
        await host.RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureAppConfiguration((hostingContext, configuration) =>
            {
                
                configuration.Sources.Clear();

                var env = hostingContext.HostingEnvironment;
                configuration
                    .AddProtectedJsonFile("appsettings.json", true, true)
                    .AddEnvironmentVariables()
                    .Build();
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging
                    .AddConfiguration(context.Configuration.GetSection("Logging"))
                    .AddConsole();
            })
            .ConfigureServices((context, services) =>
            {
                services.Configure<WorkerSettings>(context.Configuration.GetSection("Settings"));
                services.AddTransient<Worker>();
            });
}
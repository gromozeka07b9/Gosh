using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using Serilog;
using Serilog.Events;

namespace QuestHelper.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().Enrich.FromLogContext().MinimumLevel.Debug().WriteTo.ColoredConsole(
                LogEventLevel.Debug,
                "{NewLine}{Timestamp:HH:mm:ss} [{Level}] ({CorrelationToken}) {Message}{NewLine}{Exception}"
            )
                .WriteTo.File("server.log", rollingInterval:RollingInterval.Day)
                .CreateLogger();
            BuildWebHost(args).Run();
            Log.Information($"QuestHelper server started ver:{typeof(Startup).Assembly.GetName().Version.ToString()}");
        }

/*        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                    .ReadFrom.Configuration(hostingContext.Configuration))
                .Build();*/
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog()
                .Build();
    }
}

using System;
using System.Reflection;
using DSharpPlus;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QcaaDiscordBot.Core.Models;
using QcaaDiscordBot.Core.Services;
using QcaaDiscordBot.Discord;
using QcaaDiscordBot.Discord.Helpers;
using Serilog;
using Serilog.Events;

using var logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

var logFactory = new LoggerFactory().AddSerilog(logger);

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton(new DiscordClient(new DiscordConfiguration
        {
            Token = hostContext.Configuration["Discord:Token"],
            TokenType = TokenType.Bot,
            AutoReconnect = true,
            MinimumLogLevel = LogLevel.None,
            LoggerFactory = logFactory,
            MessageCacheSize = 512,
            Intents = DiscordIntents.All
        }));

        services.InjectBotServices(Assembly.GetEntryAssembly());

        services.AddMarten(options =>
        {
            options.Connection(hostContext.Configuration.GetConnectionString("Marten"));

            // Use the more permissive schema auto create behavior
            // while in development
            if (hostContext.HostingEnvironment.IsDevelopment())
            {
                options.AutoCreateSchemaObjects = AutoCreate.All;
            }
        });
        
        services.AddSingleton<IUserReportService, UserReportService>();
        
        services.AddHostedService<Bot>();
    });

host.Build().Run();
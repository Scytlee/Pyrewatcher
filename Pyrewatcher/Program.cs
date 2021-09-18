using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pyrewatcher.Handlers;
using Pyrewatcher.Helpers;
using Pyrewatcher.Riot.Interfaces;
using Pyrewatcher.Riot.Services;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using TwitchLib.Client;

namespace Pyrewatcher
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      var host = CreateHostBuilder(args).Build();

      // Check if app is running on Azure, the WEBSITE_SITE_NAME environment variable will be set if it is.
      if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME")))
      {
        Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
                                              .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                                              .Enrich.FromLogContext()
                                              .WriteTo.File(@"D:\home\LogFiles\Application\log.txt", flushToDiskInterval: TimeSpan.FromSeconds(5),
                                                            shared: true, restrictedToMinimumLevel: LogEventLevel.Debug)
                                              .CreateLogger();
      }
      else
      {
        // Not Azure, just log to Console, no need to persist
        Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
                                              .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                                              .WriteTo.Console()
                                              .CreateLogger();
      }

      new Task(async () =>
      {
        await host.RunAsync();
      }).Start();

      try
      {
        var bot = host.Services.GetService<Bot>();
        await bot.Setup();
        bot.Connect();

        await Task.Delay(-1);
      }
      catch (Exception ex)
      {
        Log.Fatal(ex, "There was a major problem that crashed the application.");
      }
      finally
      {
        Log.CloseAndFlush();
      }
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
      return Host.CreateDefaultBuilder(args)
                 .ConfigureWebHostDefaults(webBuilder =>
                  {
                    webBuilder.UseStartup<Startup>().UseSerilog();
                  })
                 .ConfigureServices((_, services) =>
                  {
                    services.AddSingleton<CommandHandler>();
                    services.AddSingleton<TemplateCommandHandler>();
                    services.AddSingleton<ActionHandler>();
                    services.AddSingleton<CyclicTasksHandler>();

                    services.AddSingleton<CommandHelpers>();
                    services.AddSingleton<DatabaseHelpers>();
                    services.AddSingleton<TwitchApiHelper>();
                    services.AddSingleton<RiotLolApiHelper>();
                    services.AddSingleton<RiotTftApiHelper>();
                    services.AddSingleton<Utilities>();

                    services.AddSingleton<IMatchV5Client, MatchV5Client>();

                    services.AddSingleton<TwitchClient>();

                    services.AddSingleton<ILoggerFactory>(x =>
                    {
                      var providerCollection = x.GetService<LoggerProviderCollection>();
                      var factory = new SerilogLoggerFactory(null, true, providerCollection);

                      foreach (var provider in x.GetServices<ILoggerProvider>())
                      {
                        factory.AddProvider(provider);
                      }

                      return factory;
                    });

                    foreach (var commandType in Globals.CommandTypes)
                    {
                      services.AddSingleton(commandType);
                    }

                    foreach (var actionType in Globals.ActionTypes)
                    {
                      services.AddSingleton(actionType);
                    }

                    var repositoryTypes = Assembly.GetExecutingAssembly()
                                                  .GetTypes()
                                                  .Where(x => x.IsClass)
                                                  .Where(x => x.Name.EndsWith("Repository") && x.Name != "Repository")
                                                  .ToList();

                    foreach (var repositoryType in repositoryTypes)
                    {
                      services.AddSingleton(repositoryType);
                    }

                    services.AddSingleton<Bot>();
                  });
    }
  }
}
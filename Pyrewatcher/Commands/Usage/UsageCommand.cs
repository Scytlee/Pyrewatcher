﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess;
using Pyrewatcher.DatabaseModels;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class UsageCommand : CommandBase<UsageCommandArguments>
  {
    private readonly TwitchClient _client;
    private readonly CommandRepository _commands;
    private readonly ILogger<UsageCommand> _logger;

    public UsageCommand(TwitchClient client, ILogger<UsageCommand> logger, CommandRepository commands)
    {
      _client = client;
      _logger = logger;
      _commands = commands;
    }

    public override UsageCommandArguments ParseAndValidateArguments(List<string> argsList, ChatMessage message)
    {
      if (argsList.Count == 0)
      {
        _logger.LogInformation("Command not provided - returning");

        return null;
      }

      var args = new UsageCommandArguments {Command = argsList[0].ToLower()};

      return args;
    }

    public override async Task<bool> ExecuteAsync(UsageCommandArguments args, ChatMessage message)
    {
      var command = await _commands.FindAsync("Name = @Name", new Command {Name = args.Command});

      if (command == null)
      {
        _logger.LogInformation("Command {command} does not exist - returning", args.Command);

        return false;
      }

      _client.SendMessage(message.Channel, string.Format(Globals.Locale["usage_response"], message.DisplayName, command.Name, command.UsageCount));

      return true;
    }
  }
}
﻿using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Pyrewatcher.DataAccess.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  public class UsageCommandArguments
  {
    public string Command { get; set; }
  }

  [UsedImplicitly]
  public class UsageCommand : ICommand
  {
    private readonly TwitchClient _client;
    private readonly ILogger<UsageCommand> _logger;

    private readonly ICommandsRepository _commandsRepository;

    public UsageCommand(TwitchClient client, ILogger<UsageCommand> logger, ICommandsRepository commandsRepository)
    {
      _client = client;
      _logger = logger;
      _commandsRepository = commandsRepository;
    }

    private UsageCommandArguments ParseAndValidateArguments(List<string> argsList)
    {
      if (argsList.Count == 0)
      {
        _logger.LogInformation("Command not provided - returning");

        return null;
      }

      var args = new UsageCommandArguments {Command = argsList[0].ToLower()};

      return args;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var args = ParseAndValidateArguments(argsList);

      if (args is null)
      {
        return false;
      }
      
      var command = await _commandsRepository.GetCommandByName(args.Command);

      if (command is null)
      {
        _logger.LogInformation("Command {command} does not exist - returning", args.Command);

        return false;
      }

      _client.SendMessage(message.Channel, string.Format(Globals.Locale["usage_response"], message.DisplayName, command.Name, command.UsageCount));

      return true;
    }
  }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Pyrewatcher.DataAccess.Interfaces;
using Pyrewatcher.Models;
using Pyrewatcher.Riot.Enums;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace Pyrewatcher.Commands
{
  [UsedImplicitly]
  public class RangaCommand : ICommand
  {
    private readonly TwitchClient _client;

    private readonly IRiotAccountsRepository _riotAccountsRepository;

    public RangaCommand(TwitchClient client, IRiotAccountsRepository riotAccountsRepository)
    {
      _client = client;
      _riotAccountsRepository = riotAccountsRepository;
    }

    public async Task<bool> ExecuteAsync(List<string> argsList, ChatMessage message)
    {
      var broadcasterId = long.Parse(message.RoomId);
      var accounts = await _riotAccountsRepository.GetActiveAccountsWithRankByChannelIdAsync(broadcasterId);

      if (accounts.Any())
      {
        var displayableAccounts = new List<string>();

        foreach (var account in accounts)
        {
          var displayableAccountBuilder = new StringBuilder(account.DisplayName);
          displayableAccountBuilder.Append(": ");
          displayableAccountBuilder.Append(account.DisplayableRank ?? Globals.Locale["ranga_value_unavailable"]);
          displayableAccountBuilder.Append(" ➔ ");
          displayableAccountBuilder.Append(GenerateAccountUrl(account));

          displayableAccounts.Add(displayableAccountBuilder.ToString());
        }

        _client.SendMessage(message.Channel, string.Join(" | ", displayableAccounts));
      }
      else
      {
        _client.SendMessage(message.Channel, string.Format(Globals.Locale["ranga_noaccounts"], message.Channel));
      }

      return true;
    }

    private static string GenerateAccountUrl(RiotAccount account)
    {
      var url = account.Game switch
      {
        Game.LeagueOfLegends => $"https://{account.Server.ToString().ToLower()}.op.gg/summoner/userName={account.SummonerName.Replace(" ", "+")}",
        Game.TeamfightTactics => $"https://lolchess.gg/profile/{account.Server.ToString().ToLower()}/{account.SummonerName.Replace(" ", "")}",
        _ => ""
      };

      return EncodeUrl(url);
    }

    private static string EncodeUrl(string url)
    {
      var bytes = Encoding.UTF8.GetBytes(url);
      var urlEncoded = string.Join("", bytes.Select(b => b > 127 ? Uri.HexEscape((char) b) : ((char) b).ToString()));

      return urlEncoded;
    }
  }
}

﻿using Newtonsoft.Json;

namespace Pyrewatcher.Riot.LeagueOfLegends.Models
{
  public class MatchTeamV5Dto
  {
    [JsonProperty("teamId")]
    public int TeamId { get; set; }
    [JsonProperty("win")]
    public bool IsWinningTeam { get; set; }
  }
}

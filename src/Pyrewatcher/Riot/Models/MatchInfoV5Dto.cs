﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pyrewatcher.Riot.Models
{
  public class MatchInfoV5Dto
  {
    [JsonProperty("gameId")]
    public long Id { get; set; }
    [JsonProperty("gameDuration")]
    public long Duration { get; set; }
    [JsonProperty("gameStartTimestamp")]
    public long Timestamp { get; set; }
    [JsonProperty("participants")]
    public IEnumerable<MatchParticipantV5Dto> Players { get; set; }
  }
}
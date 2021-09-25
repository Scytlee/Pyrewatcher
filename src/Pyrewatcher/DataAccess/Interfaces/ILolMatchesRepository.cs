﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Pyrewatcher.DatabaseModels;
using Pyrewatcher.Riot.Models;

namespace Pyrewatcher.DataAccess.Interfaces
{
  public interface ILolMatchesRepository
  {
    Task<IEnumerable<string>> GetMatchesNotInDatabase(List<string> matches, long accountId);
    Task<IEnumerable<LolMatch>> GetTodaysMatchesByAccountId(long accountId);
    Task<bool> InsertFromDto(long accountId, string fullMatchId, MatchV5Dto match, MatchParticipantV5Dto participant);
  }
}
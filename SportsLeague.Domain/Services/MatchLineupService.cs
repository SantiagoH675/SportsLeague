using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Enums;
using SportsLeague.Domain.Helpers;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Domain.Services;

public class MatchLineupService : IMatchLineupService
{
    private readonly IMatchRepository _matchRepository;
    private readonly IMatchLineupRepository _matchLineupRepository;
    private readonly MatchValidationHelper _validationHelper;
    private readonly ILogger<MatchLineupService> _logger;

    public MatchLineupService(
        IMatchRepository matchRepository,
        IMatchLineupRepository matchLineupRepository,
        MatchValidationHelper validationHelper,
        ILogger<MatchLineupService> logger)
    {
        _matchRepository = matchRepository;
        _matchLineupRepository = matchLineupRepository;
        _validationHelper = validationHelper;
        _logger = logger;
    }

    public async Task<MatchLineup> AddPlayerAsync(int matchId, MatchLineup lineup)
    {
        // V1: El partido debe existir
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

        // V6: El partido debe estar en estado Scheduled
        if (match.Status != MatchStatus.Scheduled)
            throw new InvalidOperationException(
                "Solo se pueden registrar alineaciones en partidos Scheduled");

        // V2 y V3: El jugador debe existir y pertenecer a HomeTeam o AwayTeam
        var player = await _validationHelper.ValidatePlayerInMatchAsync(
            lineup.PlayerId,
            match);

        // V4: El jugador no puede estar dos veces en la misma alineación
        var alreadyExists = await _matchLineupRepository
            .ExistsByMatchAndPlayerAsync(matchId, lineup.PlayerId);

        if (alreadyExists)
            throw new InvalidOperationException(
                "El jugador ya está registrado en la alineación de este partido");

        // V5: Máximo 11 titulares por equipo por partido
        if (lineup.IsStarter)
        {
            var startersCount = await _matchLineupRepository
                .CountStartersByMatchAndTeamAsync(matchId, player.TeamId);

            if (startersCount >= 11)
                throw new InvalidOperationException(
                    "El equipo ya tiene 11 titulares registrados en este partido");
        }

        lineup.MatchId = matchId;
        lineup.Position = lineup.Position.Trim();

        _logger.LogInformation(
            "Adding player {PlayerId} to lineup for match {MatchId}",
            lineup.PlayerId,
            matchId);

        var created = await _matchLineupRepository.CreateAsync(lineup);

        return await _matchLineupRepository.GetByIdWithDetailsAsync(created.Id)
               ?? created;
    }

    public async Task<IEnumerable<MatchLineup>> GetByMatchAsync(int matchId)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

        return await _matchLineupRepository.GetByMatchAsync(matchId);
    }

    public async Task<IEnumerable<MatchLineup>> GetByMatchAndTeamAsync(
        int matchId,
        int teamId)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

        return await _matchLineupRepository.GetByMatchAndTeamAsync(matchId, teamId);
    }

    public async Task DeleteAsync(int matchId, int id)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);
        if (match == null)
            throw new KeyNotFoundException($"No se encontró el partido con ID {matchId}");

        var lineup = await _matchLineupRepository.GetByIdAsync(id);
        if (lineup == null || lineup.MatchId != matchId)
            throw new KeyNotFoundException(
                $"No se encontró la alineación con ID {id}");

        _logger.LogInformation(
            "Deleting lineup record {LineupId} from match {MatchId}",
            id,
            matchId);

        await _matchLineupRepository.DeleteAsync(id);
    }
}
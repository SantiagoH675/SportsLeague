using Microsoft.EntityFrameworkCore;
using SportsLeague.DataAccess.Context;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.DataAccess.Repositories;

public class MatchLineupRepository : GenericRepository<MatchLineup>, IMatchLineupRepository
{
    public MatchLineupRepository(LeagueDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MatchLineup>> GetByMatchAsync(int matchId)
    {
        return await _dbSet
            .Where(ml => ml.MatchId == matchId)
            .Include(ml => ml.Player)
                .ThenInclude(p => p.Team)
            .OrderBy(ml => ml.Player.Team.Name)
            .ThenByDescending(ml => ml.IsStarter)
            .ThenBy(ml => ml.Position)
            .ToListAsync();
    }

    public async Task<IEnumerable<MatchLineup>> GetByMatchAndTeamAsync(int matchId, int teamId)
    {
        return await _dbSet
            .Where(ml => ml.MatchId == matchId && ml.Player.TeamId == teamId)
            .Include(ml => ml.Player)
                .ThenInclude(p => p.Team)
            .OrderByDescending(ml => ml.IsStarter)
            .ThenBy(ml => ml.Position)
            .ToListAsync();
    }

    public async Task<MatchLineup?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(ml => ml.Player)
                .ThenInclude(p => p.Team)
            .Include(ml => ml.Match)
            .FirstOrDefaultAsync(ml => ml.Id == id);
    }

    public async Task<bool> ExistsByMatchAndPlayerAsync(int matchId, int playerId)
    {
        return await _dbSet
            .AnyAsync(ml => ml.MatchId == matchId && ml.PlayerId == playerId);
    }

    public async Task<int> CountStartersByMatchAndTeamAsync(int matchId, int teamId)
    {
        return await _dbSet
            .CountAsync(ml =>
                ml.MatchId == matchId &&
                ml.IsStarter &&
                ml.Player.TeamId == teamId);
    }
}
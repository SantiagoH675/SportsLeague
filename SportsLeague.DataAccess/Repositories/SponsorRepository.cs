using Microsoft.EntityFrameworkCore;
using SportsLeague.DataAccess.Context;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;

namespace SportsLeague.DataAccess.Repositories;

public class SponsorRepository : GenericRepository<Sponsor>, ISponsorRepository
{
    public SponsorRepository(LeagueDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.Sponsors.AnyAsync(s => s.Name == name);
    }

    public async Task<bool> ExistsByNameAsync(string name, int excludeId)
    {
        return await _context.Sponsors.AnyAsync(s => s.Name == name && s.Id != excludeId);
    }
}
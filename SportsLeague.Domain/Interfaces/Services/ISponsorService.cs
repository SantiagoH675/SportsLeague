using SportsLeague.Domain.Entities;

namespace SportsLeague.Domain.Interfaces.Services;

public interface ISponsorService
{
    Task<IEnumerable<Sponsor>> GetAllAsync();
    Task<Sponsor?> GetByIdAsync(int id);
    Task<Sponsor> CreateAsync(Sponsor sponsor);
    Task UpdateAsync(int id, Sponsor sponsor);
    Task DeleteAsync(int id);

    Task<IEnumerable<TournamentSponsor>> GetTournamentsBySponsorAsync(int sponsorId);
    Task<TournamentSponsor> LinkTournamentAsync(int sponsorId, int tournamentId, decimal contractAmount);
    Task UnlinkTournamentAsync(int sponsorId, int tournamentId);
}
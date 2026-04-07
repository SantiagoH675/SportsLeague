using Microsoft.Extensions.Logging;
using System.Net.Mail;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Domain.Services;

public class SponsorService : ISponsorService
{
    private readonly ISponsorRepository _sponsorRepository;
    private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
    private readonly ITournamentRepository _tournamentRepository;
    private readonly ILogger<SponsorService> _logger;

    public SponsorService(
        ISponsorRepository sponsorRepository,
        ITournamentSponsorRepository tournamentSponsorRepository,
        ITournamentRepository tournamentRepository,
        ILogger<SponsorService> logger)
    {
        _sponsorRepository = sponsorRepository;
        _tournamentSponsorRepository = tournamentSponsorRepository;
        _tournamentRepository = tournamentRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Sponsor>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all sponsors");
        return await _sponsorRepository.GetAllAsync();
    }

    public async Task<Sponsor?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving sponsor with ID: {SponsorId}", id);

        var sponsor = await _sponsorRepository.GetByIdAsync(id);

        if (sponsor == null)
            _logger.LogWarning("Sponsor with ID {SponsorId} not found", id);

        return sponsor;
    }

    public async Task<Sponsor> CreateAsync(Sponsor sponsor)
    {
        if (await _sponsorRepository.ExistsByNameAsync(sponsor.Name))
        {
            _logger.LogWarning("Sponsor with name '{SponsorName}' already exists", sponsor.Name);
            throw new InvalidOperationException(
                $"Ya existe un sponsor con el nombre '{sponsor.Name}'");
        }

        ValidateEmail(sponsor.ContactEmail);

        _logger.LogInformation("Creating sponsor: {SponsorName}", sponsor.Name);
        return await _sponsorRepository.CreateAsync(sponsor);
    }

    public async Task UpdateAsync(int id, Sponsor sponsor)
    {
        var existingSponsor = await _sponsorRepository.GetByIdAsync(id);

        if (existingSponsor == null)
        {
            _logger.LogWarning("Sponsor with ID {SponsorId} not found for update", id);
            throw new KeyNotFoundException(
                $"No se encontró el sponsor con ID {id}");
        }

        if (await _sponsorRepository.ExistsByNameAsync(sponsor.Name, id))
        {
            _logger.LogWarning("Sponsor with name '{SponsorName}' already exists", sponsor.Name);
            throw new InvalidOperationException(
                $"Ya existe un sponsor con el nombre '{sponsor.Name}'");
        }

        ValidateEmail(sponsor.ContactEmail);

        existingSponsor.Name = sponsor.Name;
        existingSponsor.ContactEmail = sponsor.ContactEmail;
        existingSponsor.Phone = sponsor.Phone;
        existingSponsor.WebsiteUrl = sponsor.WebsiteUrl;
        existingSponsor.Category = sponsor.Category;

        _logger.LogInformation("Updating sponsor with ID: {SponsorId}", id);
        await _sponsorRepository.UpdateAsync(existingSponsor);
    }

    public async Task DeleteAsync(int id)
    {
        var exists = await _sponsorRepository.ExistsAsync(id);

        if (!exists)
        {
            _logger.LogWarning("Sponsor with ID {SponsorId} not found for deletion", id);
            throw new KeyNotFoundException(
                $"No se encontró el sponsor con ID {id}");
        }

        _logger.LogInformation("Deleting sponsor with ID: {SponsorId}", id);
        await _sponsorRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<TournamentSponsor>> GetTournamentsBySponsorAsync(int sponsorId)
    {
        var sponsorExists = await _sponsorRepository.ExistsAsync(sponsorId);

        if (!sponsorExists)
        {
            _logger.LogWarning("Sponsor with ID {SponsorId} not found", sponsorId);
            throw new KeyNotFoundException(
                $"No se encontró el sponsor con ID {sponsorId}");
        }

        _logger.LogInformation("Retrieving tournaments for sponsor ID: {SponsorId}", sponsorId);
        return await _tournamentSponsorRepository.GetBySponsorIdAsync(sponsorId);
    }

    public async Task<TournamentSponsor> LinkTournamentAsync(int sponsorId, int tournamentId, decimal contractAmount)
    {
        var sponsorExists = await _sponsorRepository.ExistsAsync(sponsorId);
        if (!sponsorExists)
        {
            _logger.LogWarning("Sponsor with ID {SponsorId} not found", sponsorId);
            throw new KeyNotFoundException(
                $"No se encontró el sponsor con ID {sponsorId}");
        }

        var tournamentExists = await _tournamentRepository.ExistsAsync(tournamentId);
        if (!tournamentExists)
        {
            _logger.LogWarning("Tournament with ID {TournamentId} not found", tournamentId);
            throw new KeyNotFoundException(
                $"No se encontró el torneo con ID {tournamentId}");
        }

        var existingLink = await _tournamentSponsorRepository
            .GetBySponsorAndTournamentAsync(sponsorId, tournamentId);

        if (existingLink != null)
        {
            _logger.LogWarning(
                "Sponsor {SponsorId} is already linked to tournament {TournamentId}",
                sponsorId, tournamentId);

            throw new InvalidOperationException(
                "Este sponsor ya está vinculado al torneo");
        }

        if (contractAmount <= 0)
        {
            _logger.LogWarning(
                "Invalid contract amount {ContractAmount} for sponsor {SponsorId} and tournament {TournamentId}",
                contractAmount, sponsorId, tournamentId);

            throw new InvalidOperationException(
                "ContractAmount debe ser mayor a 0");
        }

        var tournamentSponsor = new TournamentSponsor
        {
            SponsorId = sponsorId,
            TournamentId = tournamentId,
            ContractAmount = contractAmount,
            JoinedAt = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Linking sponsor {SponsorId} to tournament {TournamentId}",
            sponsorId, tournamentId);

        var created = await _tournamentSponsorRepository.CreateAsync(tournamentSponsor);

        var createdWithIncludes = await _tournamentSponsorRepository
            .GetBySponsorAndTournamentAsync(sponsorId, tournamentId);

        return createdWithIncludes ?? created;
    }

    public async Task UnlinkTournamentAsync(int sponsorId, int tournamentId)
    {
        var existingLink = await _tournamentSponsorRepository
            .GetBySponsorAndTournamentAsync(sponsorId, tournamentId);

        if (existingLink == null)
        {
            _logger.LogWarning(
                "Sponsor-tournament link not found for sponsor {SponsorId} and tournament {TournamentId}",
                sponsorId, tournamentId);

            throw new KeyNotFoundException(
                "No existe la vinculación entre sponsor y torneo");
        }

        _logger.LogInformation(
            "Unlinking sponsor {SponsorId} from tournament {TournamentId}",
            sponsorId, tournamentId);

        await _tournamentSponsorRepository.DeleteAsync(existingLink.Id);
    }

    private static void ValidateEmail(string email)
    {
        try
        {
            var _ = new MailAddress(email);
        }
        catch
        {
            throw new InvalidOperationException(
                "El formato de ContactEmail no es válido");
        }
    }
}
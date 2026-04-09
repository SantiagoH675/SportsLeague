using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SponsorController : ControllerBase
{
    private readonly ISponsorService _sponsorService;
    private readonly IMapper _mapper;
    private readonly ILogger<SponsorController> _logger;

    public SponsorController( 
        ISponsorService sponsorService,
        IMapper mapper,
        ILogger<SponsorController> logger)
    {
        _sponsorService = sponsorService;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SponsorResponseDTO>>> GetAll()
    {
        var sponsors = await _sponsorService.GetAllAsync();
        var sponsorsDto = _mapper.Map<IEnumerable<SponsorResponseDTO>>(sponsors);
        return Ok(sponsorsDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SponsorResponseDTO>> GetById(int id)
    {
        var sponsor = await _sponsorService.GetByIdAsync(id);

        if (sponsor == null)
            return NotFound(new { message = $"Sponsor con ID {id} no encontrado" });
        var sponsorDto = _mapper.Map<SponsorResponseDTO>(sponsor);
        return Ok(sponsorDto);
    }





}
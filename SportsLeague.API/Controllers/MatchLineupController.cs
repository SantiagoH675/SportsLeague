using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SportsLeague.API.DTOs.Request;
using SportsLeague.API.DTOs.Response;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.API.Controllers;

[ApiController]
[Route("api/match/{matchId:int}/lineup")]
public class MatchLineupController : ControllerBase
{
    private readonly IMatchLineupService _matchLineupService;
    private readonly IMapper _mapper;

    public MatchLineupController(
        IMatchLineupService matchLineupService,
        IMapper mapper)
    {
        _matchLineupService = matchLineupService;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<ActionResult<MatchLineupDto>> AddPlayer(
        int matchId,
        CreateMatchLineupDto dto)
    {
        try
        {
            var lineup = _mapper.Map<MatchLineup>(dto);
            var created = await _matchLineupService.AddPlayerAsync(matchId, lineup);
            var responseDto = _mapper.Map<MatchLineupDto>(created);

            return CreatedAtAction(
                nameof(GetLineup),
                new { matchId },
                responseDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MatchLineupDto>>> GetLineup(int matchId)
    {
        try
        {
            var lineups = await _matchLineupService.GetByMatchAsync(matchId);
            var responseDto = _mapper.Map<IEnumerable<MatchLineupDto>>(lineups);

            return Ok(responseDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("team/{teamId:int}")]
    public async Task<ActionResult<IEnumerable<MatchLineupDto>>> GetLineupByTeam(
        int matchId,
        int teamId)
    {
        try
        {
            var lineups = await _matchLineupService
                .GetByMatchAndTeamAsync(matchId, teamId);

            var responseDto = _mapper.Map<IEnumerable<MatchLineupDto>>(lineups);

            return Ok(responseDto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int matchId, int id)
    {
        try
        {
            await _matchLineupService.DeleteAsync(matchId, id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
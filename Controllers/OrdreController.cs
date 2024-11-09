using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ordreChange.Services.Interfaces;
using System.Security.Claims;

//[Authorize] //! Pour exiger que l’utilisateur soit authentifié
[Route("api/[controller]")]
[ApiController]
public class OrdreController : ControllerBase
{
    private readonly IOrdreService _ordreService;

    public OrdreController(IOrdreService ordreService)
    {
        _ordreService = ordreService;
    }

    [HttpPost("creer")]
    public async Task<IActionResult> CreerOrdre([FromBody] CreerOrdreDto dto)
    {
        var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Récupérer l'ID de l'agent depuis le token
        if (agentId == null) return Unauthorized();

        var ordre = await _ordreService.CreerOrdreAsync(int.Parse(agentId), dto.TypeTransaction, dto.Montant, dto.Devise);
        return CreatedAtAction(nameof(GetOrdre), new { id = ordre.IdOrdre }, ordre);
    }

    // Autres actions
}

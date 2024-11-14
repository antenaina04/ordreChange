using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ordreChange.Models;
using ordreChange.Services.Interfaces;
using System.Security.Claims;

namespace ordreChange.Controllers
{
    [Authorize]
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
            // ID via JWT
            var agentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (agentId == 0) return Unauthorized("Agent non valide.");

            try
            {
                var ordre = await _ordreService.CreerOrdreAsync(agentId, dto.TypeTransaction, dto.Montant, dto.Devise, dto.DeviseCible);
                return CreatedAtAction(nameof(GetOrdre), new { id = ordre.IdOrdre }, new OrdreDto
                {
                    IdOrdre = ordre.IdOrdre,
                    Montant = ordre.Montant,
                    Devise = ordre.Devise,
                    Statut = ordre.Statut,
                    TypeTransaction = ordre.TypeTransaction,
                    DateCreation = ordre.DateCreation,
                    MontantConverti = ordre.MontantConverti,
                    IdAgent = ordre.IdAgent,
                    Agent = new AgentDto
                    {
                        IdAgent = ordre.Agent.IdAgent,
                        Nom = ordre.Agent.Nom,
                        Role = ordre.Agent.Role
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrdre(int id)
        {
            var ordre = await _ordreService.GetOrdreByIdAsync(id);
            if (ordre == null) return NotFound();

            return Ok(new OrdreDto
            {
                IdOrdre = ordre.IdOrdre,
                Montant = ordre.Montant,
                Devise = ordre.Devise,
                Statut = ordre.Statut,
                TypeTransaction = ordre.TypeTransaction,
                DateCreation = ordre.DateCreation,
                MontantConverti = ordre.MontantConverti,
                IdAgent = ordre.IdAgent,
                Agent = new AgentDto
                {
                    IdAgent = ordre.Agent.IdAgent,
                    Nom = ordre.Agent.Nom,
                    Role = ordre.Agent.Role
                }
            });
        }
        [HttpPost("{id}/annuler")]
        public async Task<IActionResult> AnnulerOrdre(int id)
        {
            // ID via JWT
            var agentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            try
            {
                var result = await _ordreService.UpdateStatusOrdreAsync(id, agentId, "Annulé");
                if (!result)
                    return BadRequest("Le statut de l'ordre ne peut pas être changé.");

                return Ok("Annulation de l'ordre effectué avec succès");
            }
            catch (InvalidOperationException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpPost("{id}/valider")]
        public async Task<IActionResult> ValiderOrdre(int id)
        {
            // ID via JWT
            var agentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            try
            {
                var result = await _ordreService.ValiderOrdreAsync(id, agentId);
                if (!result)
                    return BadRequest("L'ordre ne peut pas être validé.");

                return Ok("Ordre validé avec succès.");
            }
            catch (InvalidOperationException ex)
            {
                return Forbid(ex.Message); // AUTHORIZATION ERROR
            }
        }
        [HttpPost("{id}/a_modifier")]
        public async Task<IActionResult> RefuserOrdre(int id)
        {
            // ID via JWT
            var agentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            try
            {
                var result = await _ordreService.UpdateStatusOrdreAsync(id, agentId, "A modifier");
                if (!result)
                    return BadRequest("Le statut de l'ordre ne peut pas être changé.");

                return Ok("Refus de l'ordre effectué avec succès.");
            }
            catch (InvalidOperationException ex)
            {
                return Forbid(ex.Message);  // AUTHORIZATION ERROR
            }
        }
        [HttpPut("{id}/modifier")]
        public async Task<IActionResult> ModifierOrdre(int id, [FromBody] ModifierOrdreDto dto)
        {
            var agentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (agentId == 0) return Unauthorized("Agent non valide.");

            try
            {
                // On envoie directement les informations de modification au service
                var result = await _ordreService.ModifierOrdreAsync(id, agentId, dto);

                if (!result)
                    return BadRequest("L'ordre ne peut pas être modifié.");

                return Ok("Modification de l'ordre effectuée avec succès.");
            }
            catch (InvalidOperationException ex)
            {
                return Forbid(ex.Message);
            }
        }

    }

    // DTO pour création ordre
    public class CreerOrdreDto
    {
        public required string TypeTransaction { get; set; }
        public required float Montant { get; set; }
        public required string Devise { get; set; }
        public required string DeviseCible { get; set; }
    }
    public class ModifierOrdreDto
    {
        public required float Montant { get; set; }
        public required string Devise { get; set; }
        public required string Statut { get; set; }
        public required string DeviseCible { get; set; }
        public required string TypeTransaction { get; set; }
    }
}

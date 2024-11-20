using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ordreChange.DTOs;
using ordreChange.Models;
using ordreChange.Services.Interfaces;
using OrdreChange.Dtos;
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
                var response = await _ordreService.CreerOrdreAsync(agentId, dto.TypeTransaction, dto.Montant, dto.Devise, dto.DeviseCible);
                return CreatedAtAction(nameof(CreerOrdre), new { id = response.IdOrdre }, response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrdre(int id)
        {
            // Utilisation du service pour obtenir le DTO mappé
            var ordreDto = await _ordreService.GetOrdreDtoByIdAsync(id);

            if (ordreDto == null)
                return NotFound(); // Retourne 404 si l'ordre n'existe pas

            return Ok(ordreDto); // Retourne directement le DTO mappé
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
                var result = await _ordreService.UpdateStatusOrdreAsync(id, agentId, "Validé");
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

        [HttpGet("statut-counts")]
        public async Task<IActionResult> GetOrdreStatutCounts()
        {
            var agentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var counts = await _ordreService.GetOrdreStatutCountsAsync(agentId);
            return Ok(counts);
        }

        [HttpGet("{id}/historique")]
        public async Task<IActionResult> GetHistoriqueOrdre(int id)
        {
            var agentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var historiqueDto = await _ordreService.GetHistoriqueDtoByOrdreIdAsync(agentId, id);
            if (historiqueDto == null)
                return NotFound("Aucun historique trouvé pour cet ordre.");

            return Ok(historiqueDto);
        }

        [HttpGet("statut/{statut}")]
        public async Task<IActionResult> GetOrdresByStatut(string statut)
        {
            var agentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var ordreDto = await _ordreService.GetOrdreDtoByStatutAsync(agentId, statut);
            
            if (ordreDto == null || ordreDto.Count == 0)
                return NotFound($"Aucun ordre trouvé avec le statut '{statut}'.");

            return Ok(ordreDto);
        }

    }
}

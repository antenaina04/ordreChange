using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
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
        private static readonly NLog.ILogger Logger = LogManager.GetCurrentClassLogger();

        public OrdreController(IOrdreService ordreService)
        {
            _ordreService = ordreService;
        }

        [HttpPost("creer")]
        public async Task<IActionResult> CreerOrdre([FromBody] CreerOrdreDto dto)
        {
            // ID via JWT
            var agentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (agentId == 0)
            {
                Logger.Warn("Unauthorized access attempt to create order");
                return Unauthorized("Agent non valide.");
            }
            try
            {
                Logger.Info("Creating order for agent {AgentId}", agentId);
                var ordre = await _ordreService.CreerOrdreAsync(agentId, dto.TypeTransaction, dto.Montant, dto.Devise, dto.DeviseCible);
                Logger.Info("Order created successfully for agent {AgentId}", agentId);
                return CreatedAtAction(nameof(CreerOrdre), new { id = ordre.IdOrdre }, ordre);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(ex, "Error creating order for agent {AgentId}", agentId);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrdre(int id)
        {
            Logger.Info("Fetching order with ID {OrdreId}", id);

            var agentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");


            var ordreDto = await _ordreService.GetOrdreDtoByIdAsync(agentId, id);

            if (ordreDto == null)
            {
                Logger.Warn("Order with ID {OrdreId} not found", id);
                return NotFound(); // Retourne 404 si not exist ordre
            }
            Logger.Info("Order with ID {OrdreId} fetched successfully", id);
            return Ok(ordreDto); 
        }

        [HttpPost("{id}/annuler")]
        public async Task<IActionResult> AnnulerOrdre(int id)
        {
            // ID via JWT
            var agentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            try
            {
                Logger.Info("Cancelling order {OrdreId} by agent {AgentId}", id, agentId);

                var result = await _ordreService.UpdateStatusOrdreAsync(id, agentId, "Annulé");
                if (!result)
                {
                    Logger.Warn("Failed to cancel order {OrdreId} by agent {AgentId}", id, agentId);
                    return BadRequest("Le statut de l'ordre ne peut pas être changé.");
                }
                Logger.Info("Order {OrdreId} cancelled successfully by agent {AgentId}", id, agentId);
                return Ok("Annulation de l'ordre effectué avec succès");
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(ex, "Error cancelling order {OrdreId} by agent {AgentId}", id, agentId);
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
                {
                    Logger.Warn("Failed to validate order {OrdreId} by agent {AgentId}", id, agentId);
                    return BadRequest("L'ordre ne peut pas être validé.");
                }
                Logger.Info("Order {OrdreId} validated successfully by agent {AgentId}", id, agentId);
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
                Logger.Info("Refusing order {OrdreId} by agent {AgentId}", id, agentId);

                var result = await _ordreService.UpdateStatusOrdreAsync(id, agentId, "A modifier");
                if (!result)
                {
                    Logger.Warn("Failed to refuse order {OrdreId} by agent {AgentId}", id, agentId);
                    return BadRequest("Le statut de l'ordre ne peut pas être changé.");
                }
                Logger.Info("Order {OrdreId} refused successfully by agent {AgentId}", id, agentId);
                return Ok("Refus de l'ordre effectué avec succès.");
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(ex, "Error validating order {OrdreId} by agent {AgentId}", id, agentId);
                return Forbid(ex.Message);  // AUTHORIZATION ERROR
            }
        }
        [HttpPut("{id}/modifier")]
        public async Task<IActionResult> ModifierOrdre(int id, [FromBody] ModifierOrdreDto dto)
        {
            var agentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            if (agentId == 0)
            {
                Logger.Warn("Unauthorized access attempt to modify order");
                return Unauthorized("Agent non valide.");
            }

            try
            {
                Logger.Info("Modifying order {OrdreId} by agent {AgentId}", id, agentId);
                var result = await _ordreService.ModifierOrdreAsync(id, agentId, dto);

                if (!result)
                {
                    Logger.Warn("Failed to modify order {OrdreId} by agent {AgentId}", id, agentId);
                    return BadRequest("L'ordre ne peut pas être modifié.");
                }
                Logger.Info("Order {OrdreId} modified successfully by agent {AgentId}", id, agentId);
                return Ok("Modification de l'ordre effectuée avec succès.");
            }
            catch (InvalidOperationException ex)
            {
                Logger.Error(ex, "Error modifying order {OrdreId} by agent {AgentId}", id, agentId);
                return Forbid(ex.Message);
            }
        }

        [HttpGet("statut-counts")]
        public async Task<IActionResult> GetOrdreStatutCounts()
        {
            var agentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            Logger.Info("Fetching order status counts for agent {AgentId}", agentId);
            var counts = await _ordreService.GetOrdreStatutCountsAsync(agentId);
            return Ok(counts);
        }

        [HttpGet("{id}/historique")]
        public async Task<IActionResult> GetHistoriqueOrdre(int id)
        {
            var agentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            Logger.Info("Fetching history for order {OrdreId} by agent {AgentId}", id, agentId);
            var historiqueDto = await _ordreService.GetHistoriqueDtoByOrdreIdAsync(agentId, id);
            if (historiqueDto == null)
            {
                Logger.Warn("No history found for order {OrdreId} by agent {AgentId}", id, agentId);
                return NotFound("Aucun historique trouvé pour cet ordre.");
            }
            Logger.Info("History for order {OrdreId} fetched successfully by agent {AgentId}", id, agentId);
            return Ok(historiqueDto);
        }

        [HttpGet("statut/{statut}")]
        public async Task<IActionResult> GetOrdresByStatut(string statut)
        {
            var agentId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            Logger.Info("Fetching orders with status {Statut} for agent {AgentId}", statut, agentId);
            var ordreDto = await _ordreService.GetOrdreDtoByStatutAsync(agentId, statut);

            if (ordreDto == null || ordreDto.Count == 0)
            {
                Logger.Warn("No orders found with status {Statut} for agent {AgentId}", statut, agentId);
                return NotFound($"Aucun ordre trouvé avec le statut '{statut}'.");
            }
            Logger.Info("Orders with status {Statut} fetched successfully for agent {AgentId}", statut, agentId);
            return Ok(ordreDto);
        }

    }
}

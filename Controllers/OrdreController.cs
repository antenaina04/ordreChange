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
                    DeviseCible = ordre.DeviseCible,
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
            var historique = await _ordreService.GetHistoriqueByOrdreIdAsync(id);
            if (historique == null || historique.Count == 0)
                return NotFound("Aucun historique trouvé pour cet ordre.");

            return Ok(historique);
        }

        [HttpGet("statut/{statut}")]
        public async Task<IActionResult> GetOrdresByStatut(string statut)
        {
            var ordres = await _ordreService.GetOrdresByStatutAsync(statut);
            if (ordres == null || ordres.Count == 0)
                return NotFound($"Aucun ordre trouvé avec le statut '{statut}'.");

            var ordreDtos = ordres.Select(o => new OrdreDto
            {
                IdOrdre = o.IdOrdre,
                Montant = o.Montant,
                Devise = o.Devise,
                Statut = o.Statut,
                TypeTransaction = o.TypeTransaction,
                DateCreation = o.DateCreation,
                MontantConverti = o.MontantConverti,
                IdAgent = o.IdAgent,
                Agent = new AgentDto
                {
                    IdAgent = o.Agent.IdAgent,
                    Nom = o.Agent.Nom,
                    Role = o.Agent.Role
                }
            }).ToList();

            return Ok(ordreDtos);
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
        public required string DeviseCible { get; set; }
        public required string TypeTransaction { get; set; }
    }
    public class HistoriqueOrdreDto
    {
        public int IdHistorique { get; set; }
        public DateTime Date { get; set; }
        public string? Statut { get; set; }
        public string? Action { get; set; }
        public float Montant { get; set; }
        public int IdOrdre { get; set; }
    }
}

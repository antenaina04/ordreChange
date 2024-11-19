namespace ordreChange.Services.Interfaces
{
    public interface IValidateurService : IBaseRoleService
    {
        Task<bool> ValiderOrdreAsync(int ordreId, int agentId);
    }
}

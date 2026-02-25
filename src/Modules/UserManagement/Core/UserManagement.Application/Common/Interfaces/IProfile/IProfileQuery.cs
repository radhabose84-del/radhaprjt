using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.IProfile
{
    public interface IProfileQuery
    {
        Task<List<Unit>> GetUnit(int userId);
    }
}
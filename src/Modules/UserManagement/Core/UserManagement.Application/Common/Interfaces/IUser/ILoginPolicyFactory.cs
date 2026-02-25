using UserManagement.Domain.Entities;

namespace UserManagement.Application.Common.Interfaces.IUser
{
    public interface ILoginPolicyFactory
    {
        Task<ILoginPolicy> GetPolicyAsync(User user);
    }
}
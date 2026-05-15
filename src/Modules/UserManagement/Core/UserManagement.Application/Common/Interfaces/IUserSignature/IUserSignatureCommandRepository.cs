namespace UserManagement.Application.Common.Interfaces.IUserSignature
{
    public interface IUserSignatureCommandRepository
    {
        Task<int> CreateAsync(UserManagement.Domain.Entities.UserSignature userSignature);

        Task<bool> UpdateAsync(int id, UserManagement.Domain.Entities.UserSignature userSignature);

        Task<bool> DeleteAsync(int id, UserManagement.Domain.Entities.UserSignature userSignature);
    }
}

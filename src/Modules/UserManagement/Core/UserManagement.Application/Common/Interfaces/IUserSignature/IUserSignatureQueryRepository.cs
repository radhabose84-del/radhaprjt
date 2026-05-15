namespace UserManagement.Application.Common.Interfaces.IUserSignature
{
    public interface IUserSignatureQueryRepository
    {
        Task<(List<UserManagement.Domain.Entities.UserSignature>, int)> GetAllUserSignatureAsync(int pageNumber, int pageSize, string? searchTerm);

        Task<UserManagement.Domain.Entities.UserSignature?> GetUserSignatureByIdAsync(int id);

        Task<UserManagement.Domain.Entities.UserSignature?> GetUserSignatureByUserIdAsync(int userId);

        Task<bool> NotFoundAsync(int id);

        Task<bool> UserExistsAsync(int userId);

        Task<bool> UserHasSignatureAsync(int userId);
    }
}

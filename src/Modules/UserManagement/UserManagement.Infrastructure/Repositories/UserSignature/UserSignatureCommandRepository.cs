using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Infrastructure.Data;

namespace UserManagement.Infrastructure.Repositories.UserSignature
{
    public class UserSignatureCommandRepository : IUserSignatureCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public UserSignatureCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> CreateAsync(UserManagement.Domain.Entities.UserSignature userSignature)
        {
            await _applicationDbContext.UserSignature.AddAsync(userSignature);
            await _applicationDbContext.SaveChangesAsync();
            return userSignature.Id;
        }

        public async Task<bool> UpdateAsync(int id, UserManagement.Domain.Entities.UserSignature userSignature)
        {
            var existing = await _applicationDbContext.UserSignature
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existing == null)
            {
                return false;
            }

            // UserId is immutable — never copied from request
            existing.FileName = userSignature.FileName;
            existing.OriginalFileName = userSignature.OriginalFileName;
            existing.FilePath = userSignature.FilePath;
            existing.FileType = userSignature.FileType;
            existing.FileSize = userSignature.FileSize;
            existing.IsActive = userSignature.IsActive;

            _applicationDbContext.UserSignature.Update(existing);
            return await _applicationDbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id, UserManagement.Domain.Entities.UserSignature userSignature)
        {
            var existing = await _applicationDbContext.UserSignature
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existing == null)
            {
                return false;
            }

            existing.IsDeleted = userSignature.IsDeleted;
            return await _applicationDbContext.SaveChangesAsync() > 0;
        }
    }
}

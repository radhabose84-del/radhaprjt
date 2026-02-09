using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Application.Common.Interfaces.IProjectMaster
{
    public interface IUploadDocumentQueryRepository
    {
        Task<string> GetDocumentDirectoryAsync();

        Task<string> GetBaseDirectoryAsync();
        Task<IReadOnlyCollection<int>> GetUploadDocumentIdsAsync(int Id);     

        Task<bool> DeleteFileDetailsDocumentAsync(int Id,int ProjectId ,string filename);
    }
}
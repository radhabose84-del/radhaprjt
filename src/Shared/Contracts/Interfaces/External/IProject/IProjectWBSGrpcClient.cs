using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Project;

namespace Contracts.Interfaces.External.IProject
{
    public interface IProjectWBSGrpcClient 
    {
        Task<List<ProjectWbsDto>> GetAllAsync(  int projectId,  CancellationToken ct = default);

        Task<ProjectWbsDto?> GetByIdAsync( int id, CancellationToken ct = default);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FAM.Application.Common.Interfaces.IAssetTransferIssueApproval;
using FAM.Domain.Entities.AssetMaster;
using FAM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FAM.Infrastructure.Repositories.AssetTransferIssueApproval
{
    public class AssetTransferIssueCommandRepository : IAssetTransferIssueApprovalCommandRepository
    {
         private readonly ApplicationDbContext _applicationDbContext;

        public AssetTransferIssueCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<int> ExecuteBulkUpdateAsync(List<int> ids, string status, int userId, DateTimeOffset currentTime, string username, string currentIp)
        {
        //     return await _applicationDbContext.AssetTransferIssueHdr
        //     .Where(t => ids.Contains(t.Id) && t.Status == "Pending" && t.AckStatus == 0) // 🔹 Additional Conditions
        //     .ExecuteUpdateAsync(setters => setters
        //         .SetProperty(t => t.Status, status)
        //         .SetProperty(t => t.AuthorizedBy, userId)
        //         .SetProperty(t => t.AuthorizedDate, currentTime)
        //         .SetProperty(t => t.AuthorizedByName, username)
        //         .SetProperty(t => t.AuthorizedIP, currentIp)
        // );

                var issueHdrsToUpdate = await _applicationDbContext.AssetTransferIssueHdr
                .Where(t => ids.Contains(t.Id) && t.Status == "Pending")
                .Select(t => new
                {
                    Entity = t
                })
                .ToListAsync();

            issueHdrsToUpdate
                .Select(x =>
                {
                    x.Entity.Status = status;
                    x.Entity.AuthorizedBy = userId;
                    x.Entity.AuthorizedDate = currentTime;
                    x.Entity.AuthorizedByName = username;
                    x.Entity.AuthorizedIP = currentIp;
                    return x;
                })
                .ToList(); // Ensures projection executes

            await _applicationDbContext.SaveChangesAsync();

            return issueHdrsToUpdate.Count;
        }

        public async Task<List<AssetTransferIssueHdr>> GetByIdsAsync(List<int> ids)
        {
            return await _applicationDbContext.AssetTransferIssueHdr
            .Where(x => ids.Contains(x.Id) && x.Status == "Pending") // Filter by Pending status
            .ToListAsync();
        }

        // public async Task<int> UpdateRangeAsync(List<AssetTransferIssueHdr> transfers)
        // {
        //     _applicationDbContext.AssetTransferIssueHdr.UpdateRange(transfers);
        //     return await _applicationDbContext.SaveChangesAsync(); // Return number of affected rows
        // }
    }
}
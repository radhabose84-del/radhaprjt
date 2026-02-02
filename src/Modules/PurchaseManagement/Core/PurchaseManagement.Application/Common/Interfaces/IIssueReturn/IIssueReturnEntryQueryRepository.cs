using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.IssueReturn.Queries.GetIssueReturnDetailsById;
using PurchaseManagement.Application.IssueReturn.Queries.GetIssueDetailsById;
// using PurchaseManagement.Application.IssueReturn.Queries.GetIssueReturnDetailsById;
using PurchaseManagement.Domain.Entities.Issue;
using PurchaseManagement.Domain.Entities.IssueReturn;

namespace PurchaseManagement.Application.Common.Interfaces.IIssueReturn
{
    public interface IIssueReturnEntryQueryRepository
    {
        Task<List<GetIssueDetailsByIdDto>> GetIssueDetailsByIssueId(int issueId, int? itemid);
        Task<GetIssueReturnDetailsByIdDto> GetByIdWithDetails(int id);
    }
}
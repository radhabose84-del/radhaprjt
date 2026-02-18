using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Queries.GetExternalRequestById
{
     public class GetExternalRequestsByIdsQuery : IRequest<ApiResponseDTO<List<GetExternalRequestByIdDto>>>
    {
        public List<int>? Ids { get; set; }  // List of IDs to fetch multiple records
    }
}
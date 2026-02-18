using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.ActivityCheckListMaster.Queries.GetActivityCheckListMaster
{
    public class GetAllActivityCheckListMasterQuery  : IRequest<ApiResponseDTO<List<GetAllActivityCheckListMasterDto>>>
    {

         public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }

        
    }
}
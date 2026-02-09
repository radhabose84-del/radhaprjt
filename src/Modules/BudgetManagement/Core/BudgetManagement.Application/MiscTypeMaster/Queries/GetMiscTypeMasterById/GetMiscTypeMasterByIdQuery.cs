using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetManagement.Application.Common.HttpResponse;
using BudgetManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace BudgetManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById
{
    public class GetMiscTypeMasterByIdQuery :  IRequest<ApiResponseDTO<GetMiscTypeMasterDto>>
    {
        public int Id { get; set; }
        
    }
}
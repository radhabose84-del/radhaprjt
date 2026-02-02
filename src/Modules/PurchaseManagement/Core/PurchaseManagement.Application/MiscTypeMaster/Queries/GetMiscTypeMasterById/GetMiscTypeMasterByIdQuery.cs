using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.HttpResponse;
using PurchaseManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace PurchaseManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById
{
    public class GetMiscTypeMasterByIdQuery :  IRequest<ApiResponseDTO<GetMiscTypeMasterDto>>
    {
        public int Id { get; set; }
        
    }
}
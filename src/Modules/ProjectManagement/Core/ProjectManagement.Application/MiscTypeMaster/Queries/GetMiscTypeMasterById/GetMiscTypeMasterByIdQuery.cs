using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectManagement.Application.Common.HttpResponse;
using ProjectManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace ProjectManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById
{
    public class GetMiscTypeMasterByIdQuery :  IRequest<ApiResponseDTO<GetMiscTypeMasterDto>>
    {
        public int Id { get; set; }
        
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace Core.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById
{
    public class GetMiscTypeMasterByIdQuery  :  IRequest<ApiResponseDTO<GetMiscTypeMasterDto>>
    {

        public int Id { get; set; }
        
    }
}
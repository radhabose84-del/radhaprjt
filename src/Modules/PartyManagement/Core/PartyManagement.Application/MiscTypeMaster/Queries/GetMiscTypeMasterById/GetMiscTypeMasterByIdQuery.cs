using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using PartyManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace PartyManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById
{
    public class GetMiscTypeMasterByIdQuery :  IRequest<ApiResponseDTO<GetMiscTypeMasterDto>>
    {
         public int Id { get; set; }
    }
}
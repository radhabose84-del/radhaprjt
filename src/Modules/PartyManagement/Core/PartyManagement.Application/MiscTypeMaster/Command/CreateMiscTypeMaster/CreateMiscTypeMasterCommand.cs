using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using PartyManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace PartyManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster
{
    public class CreateMiscTypeMasterCommand : IRequest<ApiResponseDTO<GetMiscTypeMasterDto>> 
    {
       public string? MiscTypeCode { get; set; }
       public string? Description { get; set; }
    }
}
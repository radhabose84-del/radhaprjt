using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById
{
    public class GetMiscTypeMasterByIdQuery :  IRequest<GetMiscTypeMasterDto>
    {

        public int Id { get; set; }
        
    }
}
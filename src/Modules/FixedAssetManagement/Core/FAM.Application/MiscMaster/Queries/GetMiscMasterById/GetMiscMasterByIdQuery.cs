using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace FAM.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQuery  :  IRequest<GetMiscMasterDto>
    { 
         public int Id { get; set; }
    }
}
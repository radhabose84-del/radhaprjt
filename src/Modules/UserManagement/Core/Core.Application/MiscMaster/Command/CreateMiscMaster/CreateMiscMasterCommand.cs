using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace Core.Application.MiscMaster.Command.CreateMiscMaster
{
    public class CreateMiscMasterCommand: IRequest<GetMiscMasterDto>
    {

         public int MiscTypeId { get; set; }  
        public string? Code { get; set;}
        public string? Description { get; set;}
        
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace FAM.Application.MiscMaster.Command.UpdateMiscMaster
{
    public class UpdateMiscMasterCommand : IRequest<bool>
    {
        
         
        public int Id { get; set; }
        public int MiscTypeId { get; set; }  
        public string? Code { get; set;}
        public string? Description { get; set;}
        public int SortOrder  { get; set;}
        public byte IsActive { get; set; }
    }
}
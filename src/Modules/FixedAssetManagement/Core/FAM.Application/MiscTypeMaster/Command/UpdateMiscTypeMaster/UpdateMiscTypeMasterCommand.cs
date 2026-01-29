using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster
{
    public class UpdateMiscTypeMasterCommand  : IRequest<bool>
    {

        public int Id { get; set; }
        public string? MiscTypeCode { get; set; }
        public string? Description { get; set; }
        public byte IsActive { get; set; }
        
    }
}
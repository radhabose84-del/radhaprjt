using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.UOM.Command.UpdateUOM
{
    public class UpdateUOMCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? UOMName { get; set; }
        public int SortOrder { get; set; }
        public int UOMTypeId { get; set; }
        public byte IsActive { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using FAM.Application.UOM.Queries.GetUOMs;
using MediatR;

namespace FAM.Application.UOM.Command.DeleteUOM
{
    public class DeleteUOMCommand : IRequest<bool>
    {
        public int Id { get; set; }
        
    }
}
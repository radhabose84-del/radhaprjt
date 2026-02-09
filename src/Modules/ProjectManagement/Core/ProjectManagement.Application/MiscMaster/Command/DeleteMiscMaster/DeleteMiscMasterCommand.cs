using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace ProjectManagement.Application.MiscMaster.Command.DeleteMiscMaster
{
    public class DeleteMiscMasterCommand : IRequest<bool>
    {
          public int Id { get; set; }
        
    }
}
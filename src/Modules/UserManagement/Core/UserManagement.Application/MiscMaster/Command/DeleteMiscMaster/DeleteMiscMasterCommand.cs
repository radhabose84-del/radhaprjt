using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace UserManagement.Application.MiscMaster.Command.DeleteMiscMaster
{
    public class DeleteMiscMasterCommand: IRequest<bool>
    {
          public int Id { get; set; }
        
    }
}
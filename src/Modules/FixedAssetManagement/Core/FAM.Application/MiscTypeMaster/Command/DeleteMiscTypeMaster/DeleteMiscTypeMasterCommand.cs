using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using MediatR;

namespace FAM.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster
{
    public class DeleteMiscTypeMasterCommand  : IRequest<bool>
    {
          public int Id { get; set; }
    }
}
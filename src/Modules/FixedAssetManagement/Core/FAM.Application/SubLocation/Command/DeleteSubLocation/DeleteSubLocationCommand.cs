using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using FAM.Application.SubLocation.Queries.GetSubLocations;
using MediatR;

namespace FAM.Application.Location.Command.DeleteAubLocation
{
    public class DeleteSubLocationCommand : IRequest<bool>
    {
        public int Id { get; set; }
        
    }
}
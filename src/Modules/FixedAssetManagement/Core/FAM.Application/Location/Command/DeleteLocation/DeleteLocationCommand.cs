using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using FAM.Application.Location.Queries.GetLocations;
using MediatR;

namespace FAM.Application.Location.Command.DeleteLocation
{
    public class DeleteLocationCommand : IRequest<LocationDto>
    {
        public int Id { get; set; }
        
    }
}
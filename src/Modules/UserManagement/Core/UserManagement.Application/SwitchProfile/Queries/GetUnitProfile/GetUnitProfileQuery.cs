using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.SwitchProfile.Queries.GetUnitProfile
{
    public class GetUnitProfileQuery : IRequest<List<GetUnitProfileDTO>>
    {
        
    }
}
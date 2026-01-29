using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.SwitchProfile.Queries.GetUnitProfile
{
    public class GetUnitProfileQuery : IRequest<List<GetUnitProfileDTO>>
    {
        
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Party;

namespace Contracts.Interfaces.External.IParty
{
    public interface ILocationGrpcClient
    {
        Task<LocationDto?> GetOrCreateLocationAsync(string city, string state, string country);
    }
}
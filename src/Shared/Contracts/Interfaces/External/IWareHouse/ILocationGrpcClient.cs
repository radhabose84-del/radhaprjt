using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.WareHouse;

namespace Contracts.Interfaces.External.IWareHouse
{
    public interface ILocationGrpcClient
    {
        Task<LocationDto?> GetOrCreateLocationAsync(string city, string state, string country); 
        
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Users;

namespace Contracts.Interfaces.External.IUser
{
    public interface ICityGrpcClient
    {
         Task<List<CityDto>> GetAllCityAsync();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contracts.Dtos.Users;

namespace Contracts.Interfaces.External.IUser
{
    public interface IMenuGrpcClient
    {
        Task<List<MenuDto>> GetMenuIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
        Task<int> GetMenuByNameAsync(string MenuName);
    }
}
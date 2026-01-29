using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Purchase;

namespace Contracts.Interfaces.External.IPurchase
{
    public interface IIndentServiceClient
    {
        Task<IndentDto> GetIndentById(int Id);
    }
}
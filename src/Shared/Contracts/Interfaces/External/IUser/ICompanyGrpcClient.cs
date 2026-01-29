using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Dtos.Users;

namespace Contracts.Interfaces.External.IUser
{
    public interface ICompanyGrpcClient
    {
         Task<List<CompanyDto>> GetAllCompanyAsync();
    }
}
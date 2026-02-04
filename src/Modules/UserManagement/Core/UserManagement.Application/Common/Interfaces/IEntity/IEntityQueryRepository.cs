using UserManagement.Application.Entity.Queries.GetCompanyBasedUnit;
using UserManagement.Application.Entity.Queries.GetEntityBasedCompany;
using UserManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace UserManagement.Application.Common.Interfaces.IEntity
{
  using Entity = UserManagement.Domain.Entities.Entity;
  public interface IEntityQueryRepository
  {

    Task<(List<Entity>, int)> GetAllEntityAsync(int PageNumber, int PageSize, string? SearchTerm);
    Task<Entity> GetByIdAsync(int Id);
    Task<List<Entity>> GetByEntityNameAsync(string entity);
    Task<string> GenerateEntityCodeAsync();
    Task<bool> SoftDeleteValidation(int Id);
    Task<List<Entity>> GetByEntityName_SuperAdmin(string entity);
    Task<List<GetEntityBasedCompanyDto>> GetCompanyNames(int EntityId);
    Task<List<GetCompanyBasedUnitDto>> GetCompanyBasedUnits(List<int> companyIds);
  }
}
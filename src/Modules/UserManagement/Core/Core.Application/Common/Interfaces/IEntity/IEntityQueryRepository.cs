using Core.Application.Entity.Queries.GetCompanyBasedUnit;
using Core.Application.Entity.Queries.GetEntityBasedCompany;
using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Core.Application.Common.Interfaces.IEntity
{
  using Entity = Core.Domain.Entities.Entity;
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
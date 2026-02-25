#nullable disable
using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;
using UserManagement.Domain.Entities;
using AutoMapper;
using UserManagement.Application.Common.Interfaces.ICompany;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Infrastructure.Repositories.Companies
{
    public class CompanyCommandRepository : ICompanyCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IMapper _imapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompanyCommandRepository(ApplicationDbContext applicationDbContext, IMapper imapper, IHttpContextAccessor httpContextAccessor)
        {
            _applicationDbContext = applicationDbContext;
            _imapper = imapper;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<int> CreateAsync(Company company)
        {
            var entry = _applicationDbContext.Entry(company);
            await _applicationDbContext.Companies.AddAsync(company);
            await _applicationDbContext.SaveChangesAsync();

            // var context = _httpContextAccessor.HttpContext;
            //  var userId =   _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //  var key = $"Companylogo-{userId}";
            // if (context != null && context.Session != null)
            //  {
            //      context.Session.Remove(key);

            //  }
            return company.Id;
        }
        public async Task<bool> UpdateAsync(int id, Company company)
        {
            var existingCompany = await _applicationDbContext.Companies
            .Include(c => c.CompanyAddress)
            .Include(c => c.CompanyContact)
            .AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

            if (existingCompany != null)
            {
                existingCompany.CompanyName = company.CompanyName;
                existingCompany.LegalName = company.LegalName;
                existingCompany.GstNumber = company.GstNumber;
                existingCompany.TIN = company.TIN;
                existingCompany.TAN = company.TAN;
                existingCompany.CSTNo = company.CSTNo;
                existingCompany.YearOfEstablishment = company.YearOfEstablishment;
                existingCompany.Website = company.Website;
                existingCompany.Logo = company.Logo;
                existingCompany.EntityId = company.EntityId;
                existingCompany.IsActive = company.IsActive;
                existingCompany.PanNumber = company.PanNumber;
                existingCompany.CompanyAddress.AddressLine1 = company.CompanyAddress.AddressLine1;
                existingCompany.CompanyAddress.AddressLine2 = company.CompanyAddress.AddressLine2;
                existingCompany.CompanyAddress.PinCode = company.CompanyAddress.PinCode;
                existingCompany.CompanyAddress.AlternatePhone = company.CompanyAddress.AlternatePhone;
                existingCompany.CompanyAddress.Phone = company.CompanyAddress.Phone;
                existingCompany.CompanyAddress.CountryId = company.CompanyAddress.CountryId;
                existingCompany.CompanyAddress.CityId = company.CompanyAddress.CityId;
                existingCompany.CompanyAddress.StateId = company.CompanyAddress.StateId;
                existingCompany.CompanyContact.Designation = company.CompanyContact.Designation;
                existingCompany.CompanyContact.Email = company.CompanyContact.Email;
                existingCompany.CompanyContact.Name = company.CompanyContact.Name;
                existingCompany.CompanyContact.Phone = company.CompanyContact.Phone;
                existingCompany.CompanyContact.Remarks = company.CompanyContact.Remarks;
                _applicationDbContext.Companies.Update(existingCompany);

                // var context = _httpContextAccessor.HttpContext;
                //  var userId =   _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                //  var key = $"Companylogo-{userId}";
                // if (context != null && context.Session != null)
                //  {
                //      context.Session.Remove(key);

                //  }

                return await _applicationDbContext.SaveChangesAsync() > 0;
            }

            return false;
        }
        public async Task<bool> DeleteAsync(int id, Company company)
        {
            var companyToDelete = await _applicationDbContext.Companies.FirstOrDefaultAsync(u => u.Id == id);
            if (companyToDelete != null)
            {
                companyToDelete.IsDeleted = company.IsDeleted;
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }
            return false;
        }
    }
}
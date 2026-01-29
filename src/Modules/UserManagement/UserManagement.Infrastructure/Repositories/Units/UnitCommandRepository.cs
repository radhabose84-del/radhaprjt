using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;
using Core.Domain.Entities;
using Core.Application.Common.Interfaces.IUnit;
using OfficeOpenXml.Style.XmlAccess;

namespace UserManagement.Infrastructure.Repositories.Units
{
    public class UnitCommandRepository : IUnitCommandRepository
    {
    private readonly ApplicationDbContext _applicationDbContext;

        public UnitCommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
     

       public async Task<int> CreateUnitAsync(Unit unit)
       {
        var entry =_applicationDbContext.Entry(unit);
        await _applicationDbContext.Unit.AddAsync(unit);
        await _applicationDbContext.SaveChangesAsync();
        return unit.Id;
      }



    public async Task<int> UpdateUnitAsync(int Id, Unit unit)
{
     // Try to find the existing unit by ID
    var existingUnit = await _applicationDbContext.Unit
            .Include(c => c.UnitAddress)
            .Include(c => c.UnitContacts)
            .AsNoTracking().FirstOrDefaultAsync(u => u.Id == Id);
   
    

    // If no unit is found, return -1 (indicating failure)
    if (existingUnit == null)
    {
        return -1;
    }

    // Update the existing unit with the new data
    existingUnit.UnitName = unit.UnitName;
    existingUnit.ShortName = unit.ShortName;
    existingUnit.CompanyId = unit.CompanyId;
    existingUnit.DivisionId = unit.DivisionId;
    existingUnit.UnitHeadName = unit.UnitHeadName;
    existingUnit.CINNO = unit.CINNO;
    existingUnit.IsActive = unit.IsActive;
    existingUnit.OldUnitId=unit.OldUnitId;
    existingUnit.IsMaintenanceStopStart=unit.IsMaintenanceStopStart;
    existingUnit.SpindlesCapacity=unit.SpindlesCapacity;


    // Update the UnitAddress
            existingUnit.UnitAddress.CountryId = unit.UnitAddress.CountryId;
    existingUnit.UnitAddress.StateId = unit.UnitAddress.StateId;
    existingUnit.UnitAddress.CityId = unit.UnitAddress.CityId;
    existingUnit.UnitAddress.AddressLine1 = unit.UnitAddress.AddressLine1;
    existingUnit.UnitAddress.AddressLine2 = unit.UnitAddress.AddressLine2;
    existingUnit.UnitAddress.PinCode = unit.UnitAddress.PinCode;
    existingUnit.UnitAddress.ContactNumber = unit.UnitAddress.ContactNumber;
    existingUnit.UnitAddress.AlternateNumber = unit.UnitAddress.AlternateNumber;

    // Update the UnitContacts
    existingUnit.UnitContacts.Name = unit.UnitContacts.Name;    
    existingUnit.UnitContacts.Designation = unit.UnitContacts.Designation;
    existingUnit.UnitContacts.Email = unit.UnitContacts.Email;
    existingUnit.UnitContacts.PhoneNo = unit.UnitContacts.PhoneNo;
    existingUnit.UnitContacts.Remarks = unit.UnitContacts.Remarks;

    _applicationDbContext.Unit.Update(existingUnit);

    // Save the changes to the database
    await _applicationDbContext.SaveChangesAsync();
    
    // Return the ID of the updated unit (indicating success)
     return existingUnit.Id;
}

       public async Task<int> DeleteUnitAsync(int Id, Unit unit)
        {
            var unitToDelete = await _applicationDbContext.Unit.FirstOrDefaultAsync(u => u.Id == Id);
            // If the Unit does not exist, throw a CustomException
            if (unitToDelete == null)
            {
                return -1;
            }
                unitToDelete.IsDeleted = unit.IsDeleted;
                await _applicationDbContext.SaveChangesAsync();
                return unitToDelete.Id;                        
        }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        return await _applicationDbContext.Unit.AnyAsync(c => c.UnitName == code);
    }

    public async Task<bool> ExistsByNameupdateAsync(string name, int id)
    {
        return await _applicationDbContext.Unit.AnyAsync(c => c.UnitName == name && c.Id != id);
    }

        
    }
}
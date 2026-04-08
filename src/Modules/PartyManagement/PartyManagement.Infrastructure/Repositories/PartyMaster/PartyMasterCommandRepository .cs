#nullable disable
using Contracts.Interfaces;
using PartyManagement.Application.Common.Interfaces;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using PartyManagement.Application.PartyMaster.Command.CreatePartyMaster;
using PartyManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PartyManagement.Infrastructure.Data;

namespace PartyManagement.Infrastructure.Repositories.PartyMaster
{
    public class PartyMasterCommandRepository : IPartyMasterCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IIPAddressService _ipAddressService;
        public PartyMasterCommandRepository(ApplicationDbContext applicationDbContext, IIPAddressService ipAddressService)
        {
            _applicationDbContext = applicationDbContext;
            _ipAddressService = ipAddressService;
        }

        public async Task<int> CreateAsync(PartyManagement.Domain.Entities.PartyMaster partyMaster)
        {
            // Fetch Pending StatusId
            var pendingStatusId = await (
                        from m in _applicationDbContext.MiscMaster
                        join mt in _applicationDbContext.MiscTypeMaster
                            on m.MiscTypeId equals mt.Id
                        where mt.MiscTypeCode == "ApprovalStatus" && m.Code == "Pending"
                        select m.Id
                    ).FirstOrDefaultAsync();

            partyMaster.StatusId = pendingStatusId;

            // Add main PartyMaster
            await _applicationDbContext.PartyMaster.AddAsync(partyMaster);

            // EF will automatically save non-null child collections
            await _applicationDbContext.SaveChangesAsync();

            return partyMaster.Id; ;
        }

        public async Task<bool> DeleteAsync(int Id, PartyManagement.Domain.Entities.PartyMaster partyMaster)
        {
            // Fetch the PartyMaster to delete from the database
            var partymasterToDelete = await _applicationDbContext.PartyMaster.FirstOrDefaultAsync(u => u.Id == Id);

            // If the PartyMaster does not exist
            if (partymasterToDelete is null)
            {
                return false; //indicate failure
            }

            // Update the IsActive status to indicate deletion (or soft delete)
            partymasterToDelete.IsDeleted = partyMaster.IsDeleted;

            // Save changes to the database 
            return await _applicationDbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteFileDetailsDocumentAsync(int id, int partyId, string fileName)
        {
            var entity = await _applicationDbContext.PartyDocument
                .FirstOrDefaultAsync(x => x.Id == id && x.PartyId == partyId && x.FileName == fileName);

            if (entity == null)
                return false;

            _applicationDbContext.PartyDocument.Remove(entity);
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }


        public async Task<string> GetNextPartyCodeAsync()
        {
            var lastCode = await _applicationDbContext.PartyMaster
                .OrderByDescending(p => p.PartyCode)
                .Select(p => p.PartyCode)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(lastCode))
                return "P0001";

            int lastNumber = int.Parse(lastCode.Substring(1));
            return $"P{(lastNumber + 1).ToString("D4")}";
        }

        public async Task<List<int>> GetPartyDocumentIdsAsync(int partyId)
        {
            return await _applicationDbContext.PartyDocument
                .Where(d => d.PartyId == partyId)
                .Select(d => d.DocumentId)
                .ToListAsync();
        }


        public async Task<bool> UpdateAsync(int Id, PartyManagement.Domain.Entities.PartyMaster partyMaster)
        {
            var existingParty = await _applicationDbContext.PartyMaster
            .Include(p => p.PartyTypes)
            .Include(p => p.PartyContactTypes)
            .Include(p => p.PartyAddressTypes)
            .Include(p => p.PartyBankTypes)
            .Include(p => p.PartyDocumentTypes)
            .Include(p => p.PartyUnitCompanyMappings)
            .Include(p => p.SalesTypes)
            .Include(p => p.AgentConfigs)
            .Include(p => p.TransportDetails)
            .FirstOrDefaultAsync(p => p.Id == Id);

            if (existingParty == null)
                return false;

            // ✅ Track changes for PartyMaster 
            await TrackChanges(existingParty, partyMaster, existingParty.Id, "PartyMaster");

            // Update the existing PartyMaster properties

            existingParty.PartyName = partyMaster.PartyName;
            existingParty.PartyZoneId = partyMaster.PartyZoneId;
            existingParty.RegistrationTypeId = partyMaster.RegistrationTypeId;
            existingParty.GSTNumber = partyMaster.GSTNumber;
            existingParty.GSTStateCode = partyMaster.GSTStateCode;
            existingParty.PAN = partyMaster.PAN;
            existingParty.Website = partyMaster.Website;
            existingParty.TAN = partyMaster.TAN;
            existingParty.TDSCategoryId = partyMaster.TDSCategoryId;
            existingParty.MSMETypeId = partyMaster.MSMETypeId;
            existingParty.MSMENO = partyMaster.MSMENO;
            existingParty.MSMEValidUpto = partyMaster.MSMEValidUpto;
            existingParty.IsMsmeCompliant = partyMaster.IsMsmeCompliant;
            existingParty.IsTDSApplicable = partyMaster.IsTDSApplicable;
            existingParty.IsTCSApplicable = partyMaster.IsTCSApplicable;
            existingParty.IsGstReverseCharge = partyMaster.IsGstReverseCharge;
            existingParty.Is206AB206CCAApplicable = partyMaster.Is206AB206CCAApplicable;
            existingParty.PayementModeId = partyMaster.PayementModeId;
            existingParty.FavourOf = partyMaster.FavourOf;
            existingParty.PreferredCurrencyPurchase = partyMaster.PreferredCurrencyPurchase;
            existingParty.CreditLimit = partyMaster.CreditLimit;
            existingParty.SellingPriceListId = partyMaster.SellingPriceListId;
            existingParty.CustomerTypeId = partyMaster.CustomerTypeId;
            existingParty.IsInternalCustomer = partyMaster.IsInternalCustomer;
            existingParty.IsInternalSupplier = partyMaster.IsInternalSupplier;
            existingParty.IsStopPayment = partyMaster.IsStopPayment;
            existingParty.GSTRegistrationDate = partyMaster.GSTRegistrationDate;
            existingParty.MSMERegistrationDate = partyMaster.MSMERegistrationDate;
            existingParty.CIN = partyMaster.CIN;
            existingParty.IECode = partyMaster.IECode;
            existingParty.IsGroup = partyMaster.IsGroup;
            existingParty.IsSubsidiary = partyMaster.IsSubsidiary;
            existingParty.InsuranceLimit = partyMaster.InsuranceLimit;
            existingParty.IsActive = partyMaster.IsActive;
            existingParty.IsPortalAccessEnabled = partyMaster.IsPortalAccessEnabled;
            existingParty.SalesFreightId = partyMaster.SalesFreightId;
            existingParty.PurchaseFreightId = partyMaster.PurchaseFreightId;
            existingParty.FreightExpensesGl = partyMaster.FreightExpensesGl;

            // Check if any related collection has at least one record
            bool hasRelatedRecords =
                (partyMaster.PartyTypes?.Any() ?? false) ||
                (partyMaster.PartyContactTypes?.Any() ?? false) ||
                (partyMaster.PartyAddressTypes?.Any() ?? false) ||
                (partyMaster.PartyBankTypes?.Any() ?? false) ||
                (partyMaster.PartyDocumentTypes?.Any() ?? false) ||
                (partyMaster.PartyUnitCompanyMappings?.Any() ?? false) ||
                (partyMaster.SalesTypes?.Any() ?? false) ||
                (partyMaster.AgentConfigs?.Any() ?? false) ||
                (partyMaster.TransportDetails?.Any() ?? false);

            if (hasRelatedRecords)
            {
                existingParty.IsUpdate = partyMaster.IsUpdate;
            }




            // PartyTypes - Update if exists, else Insert

            if (partyMaster.PartyTypes != null)
            {
                foreach (var incoming in partyMaster.PartyTypes)
                {
                    if (incoming.Id > 0 && incoming.PartyId > 0)
                    {
                        var existingChildpartytypes = existingParty.PartyTypes
                            .FirstOrDefault(pt => pt.Id == incoming.Id && pt.PartyId == Id);

                        if (existingChildpartytypes != null)
                        {
                            // Update Log - PartyType
                            await TrackChanges(existingChildpartytypes, incoming, existingParty.Id, "PartyType");
                            existingChildpartytypes.PartyGroupId = incoming.PartyGroupId;
                            existingChildpartytypes.PartyTypeId = incoming.PartyTypeId;


                        }
                    }
                    else // New record (Id == 0)
                    {
                        existingParty.PartyTypes.Add(new PartyType
                        {
                            PartyId = existingParty.Id,
                            PartyTypeId = incoming.PartyTypeId,
                            PartyGroupId = incoming.PartyGroupId
                        });

                        // Insert Log - PartyType
                        await LogChange(existingParty.Id, "PartyType", "PartyTypeId-PartyGroupId", "", incoming.PartyTypeId + "," + incoming.PartyGroupId, "Insert");
                    }
                }
            }


            // PartyUnitCompanyMappings - Update if exists, else Insert

            if (partyMaster.PartyUnitCompanyMappings != null)
            {
                foreach (var incoming in partyMaster.PartyUnitCompanyMappings)
                {
                    if (incoming.Id > 0 && incoming.PartyId > 0)
                    {
                        var existingChildpartytypes = existingParty.PartyUnitCompanyMappings
                            .FirstOrDefault(pt => pt.Id == incoming.Id && pt.PartyId == Id);

                        if (existingChildpartytypes != null)
                        {
                            // Update Log - PartyType
                            await TrackChanges(existingChildpartytypes, incoming, existingParty.Id, "PartyUnitCompanyMappings");
                            existingChildpartytypes.CompanyId = incoming.CompanyId;
                            existingChildpartytypes.UnitId = incoming.UnitId;


                        }
                    }
                    else // New record (Id == 0)
                    {
                        existingParty.PartyUnitCompanyMappings.Add(new PartyUnitCompanyMapping
                        {
                            PartyId = existingParty.Id,
                            CompanyId = incoming.CompanyId,
                            UnitId = incoming.UnitId
                        });

                        // Insert Log - PartyType
                        await LogChange(existingParty.Id, "PartyUnitCompanyMappings", "PartyTypeId-PartyGroupId", "", incoming.CompanyId + "," + incoming.UnitId, "Insert");
                    }
                }
            }



            // Partycontacts - Update if exists, else Insert

            if (partyMaster.PartyContactTypes != null)
            {
                foreach (var incoming in partyMaster.PartyContactTypes)
                {
                    if (incoming.Id > 0 && incoming.PartyId > 0)
                    {
                        var existingChildpartyciontact = existingParty.PartyContactTypes
                            .FirstOrDefault(pt => pt.Id == incoming.Id && pt.PartyId == Id);

                        if (existingChildpartyciontact != null)
                        {
                            // Update Log - PartyType
                            await TrackChanges(existingChildpartyciontact, incoming, existingParty.Id, "PartyContact");
                            existingChildpartyciontact.FirstName = incoming.FirstName;
                            existingChildpartyciontact.LastName = incoming.LastName;
                            existingChildpartyciontact.GenderId = incoming.GenderId;
                            existingChildpartyciontact.Designation = incoming.Designation;
                            existingChildpartyciontact.EmailID = incoming.EmailID;
                            existingChildpartyciontact.MobileNo = incoming.MobileNo;
                            existingChildpartyciontact.Phone = incoming.Phone;
                            existingChildpartyciontact.PreferredChannelId = incoming.PreferredChannelId;
                            existingChildpartyciontact.ContactTypeId = incoming.ContactTypeId;
                            existingChildpartyciontact.ContactBy = incoming.ContactBy;

                        }
                    }
                    else // New record (Id == 0)
                    {
                        existingParty.PartyContactTypes.Add(new PartyContact
                        {
                            PartyId = existingParty.Id,
                            FirstName = incoming.FirstName,
                            LastName = incoming.LastName,
                            GenderId = incoming.GenderId,
                            Designation = incoming.Designation,
                            EmailID = incoming.EmailID,
                            MobileNo = incoming.MobileNo,
                            Phone = incoming.Phone,
                            PreferredChannelId = incoming.PreferredChannelId,
                            ContactTypeId = incoming.ContactTypeId,
                            ContactBy = incoming.ContactBy
                        });

                        // Insert Log - PartyContact
                        await LogChange(existingParty.Id, "PartyContact", "ContactBy", "", incoming.ContactBy ?? string.Empty, "Insert");
                    }
                }
            }

            // PartyAddresses - Update if exists, else Insert

            if (partyMaster.PartyAddressTypes != null)
            {
                foreach (var incoming in partyMaster.PartyAddressTypes)
                {
                    if (incoming.Id > 0 && incoming.PartyId > 0)
                    {
                        var existingChildpartyaddress = existingParty.PartyAddressTypes
                            .FirstOrDefault(pt => pt.Id == incoming.Id && pt.PartyId == Id);

                        if (existingChildpartyaddress != null)
                        {
                            // Update Log - PartyAddress
                            await TrackChanges(existingChildpartyaddress, incoming, existingParty.Id, "PartyAddress");
                            existingChildpartyaddress.AddressType = incoming.AddressType;
                            existingChildpartyaddress.AddressLine1 = incoming.AddressLine1;
                            existingChildpartyaddress.AddressLine2 = incoming.AddressLine2;
                            existingChildpartyaddress.CityId = incoming.CityId;
                            existingChildpartyaddress.StateId = incoming.StateId;
                            existingChildpartyaddress.PostalCode = incoming.PostalCode;
                            existingChildpartyaddress.CountryId = incoming.CountryId;


                        }
                    }
                    else // New record (Id == 0)
                    {
                        existingParty.PartyAddressTypes.Add(new PartyAddress
                        {
                            PartyId = existingParty.Id,
                            AddressType = incoming.AddressType,
                            AddressLine1 = incoming.AddressLine1,
                            AddressLine2 = incoming.AddressLine2,
                            CityId = incoming.CityId,
                            StateId = incoming.StateId,
                            PostalCode = incoming.PostalCode,
                            CountryId = incoming.CountryId
                        });

                        // Insert Log - PartyAddress
                        await LogChange(existingParty.Id, "PartyAddress", "AddressType", "", incoming.AddressType ?? string.Empty, "Insert");
                    }
                }
            }

            // PartyBanks - Update if exists, else Insert

            if (partyMaster.PartyBankTypes != null)
            {
                foreach (var incoming in partyMaster.PartyBankTypes)
                {
                    if (incoming.Id > 0 && incoming.PartyId > 0)
                    {
                        var existingChildpartybank = existingParty.PartyBankTypes
                            .FirstOrDefault(pt => pt.Id == incoming.Id && pt.PartyId == Id);

                        if (existingChildpartybank != null)
                        {
                            // Update Log - PartyBank
                            await TrackChanges(existingChildpartybank, incoming, existingParty.Id, "PartyBank");
                            existingChildpartybank.BankName = incoming.BankName;
                            existingChildpartybank.BankAccountNumber = incoming.BankAccountNumber;
                            existingChildpartybank.BankBranch = incoming.BankBranch;
                            existingChildpartybank.IFSCCode = incoming.IFSCCode;
                            existingChildpartybank.SWIFTCode = incoming.SWIFTCode;
                            existingChildpartybank.AccountTypeId = incoming.AccountTypeId;
                            existingChildpartybank.IsDefaultAccount = incoming.IsDefaultAccount;
                            existingChildpartybank.IsPrimaryAccount = incoming.IsPrimaryAccount;


                        }
                    }
                    else // New record (Id == 0)
                    {
                        existingParty.PartyBankTypes.Add(new PartyBank
                        {
                            PartyId = existingParty.Id,
                            BankName = incoming.BankName,
                            BankAccountNumber = incoming.BankAccountNumber,
                            BankBranch = incoming.BankBranch,
                            IFSCCode = incoming.IFSCCode,
                            SWIFTCode = incoming.SWIFTCode,
                            AccountTypeId = incoming.AccountTypeId,
                            IsDefaultAccount = incoming.IsDefaultAccount,
                            IsPrimaryAccount = incoming.IsPrimaryAccount
                        });

                        // Insert Log - PartyBank
                        await LogChange(existingParty.Id, "PartyBank", "BankName", "", incoming.BankName ?? string.Empty, "Insert");
                    }
                }
            }

            // PartyDocuments - Update if exists, else Insert

            if (partyMaster.PartyDocumentTypes != null && partyMaster.PartyDocumentTypes.Any())
            {
                partyMaster.PartyDocumentTypes = partyMaster.PartyDocumentTypes
                                                .Where(d => !(d.Id == 0 &&
                                                            (d.DocumentId == 0 ||
                                                            string.IsNullOrWhiteSpace(d.FileName) ||
                                                            d.FileName == "string")))
                                                .ToList();
                // ✅ If no valid documents left, set null
                if (partyMaster.PartyDocumentTypes == null || partyMaster.PartyDocumentTypes.Count == 0)
                {
                    partyMaster.PartyDocumentTypes = null;
                }

                else
                {
                    foreach (var incoming in partyMaster.PartyDocumentTypes)
                    {

                        if (incoming.Id > 0 && incoming.PartyId > 0)
                        {
                            var existingChildpartydocument = existingParty.PartyDocumentTypes
                                ?.FirstOrDefault(pt => pt.Id == incoming.Id && pt.PartyId == Id);

                            if (existingChildpartydocument != null)
                            {
                                existingChildpartydocument.DocumentId = incoming.DocumentId;
                                existingChildpartydocument.FileName = incoming.FileName;
                                // Update Log - PartyDocument
                                //await TrackChanges(existingChild, incoming, existingParty.Id, "PartyDocument");
                            }
                        }
                        else
                        {
                            if (incoming.Id == 0 && incoming.PartyId > 0 && incoming.DocumentId > 0 && !string.IsNullOrWhiteSpace(incoming.FileName))
                            {
                                existingParty.PartyDocumentTypes.Add(new PartyDocument
                                {
                                    PartyId = existingParty.Id,
                                    DocumentId = incoming.DocumentId,
                                    FileName = incoming.FileName,
                                    UploadedDate = DateTimeOffset.Now

                                });
                                // Insert Log - PartyDocument
                                await LogChange(existingParty.Id, "PartyDocument", "FileName", "", incoming.FileName, "Insert");
                            }
                        }
                    }
                }

            }

            // SalesTypes - Update if exists, else Insert
            if (partyMaster.SalesTypes != null)
            {
                foreach (var incoming in partyMaster.SalesTypes)
                {
                    if (incoming.Id > 0 && incoming.PartyId > 0)
                    {
                        var existingChildSalesType = existingParty.SalesTypes
                            .FirstOrDefault(st => st.Id == incoming.Id && st.PartyId == Id);

                        if (existingChildSalesType != null)
                        {
                            await TrackChanges(existingChildSalesType, incoming, existingParty.Id, "SalesType");
                            existingChildSalesType.SalesSegmentId = incoming.SalesSegmentId;
                            existingChildSalesType.OrderTypeId = incoming.OrderTypeId;
                            existingChildSalesType.IncotermId = incoming.IncotermId;
                            existingChildSalesType.PaymentTermsId = incoming.PaymentTermsId;
                            existingChildSalesType.ShippingConditionId = incoming.ShippingConditionId;
                            existingChildSalesType.AccountAssignmentId = incoming.AccountAssignmentId;
                            existingChildSalesType.Active = incoming.Active;
                        }
                    }
                    else
                    {
                        existingParty.SalesTypes.Add(new SalesType
                        {
                            PartyId = existingParty.Id,
                            SalesSegmentId = incoming.SalesSegmentId,
                            OrderTypeId = incoming.OrderTypeId,
                            IncotermId = incoming.IncotermId,
                            PaymentTermsId = incoming.PaymentTermsId,
                            ShippingConditionId = incoming.ShippingConditionId,
                            AccountAssignmentId = incoming.AccountAssignmentId,
                            Active = incoming.Active
                        });

                        await LogChange(existingParty.Id, "SalesType", "SalesSegmentId-OrderTypeId", "",
                            incoming.SalesSegmentId + "," + incoming.OrderTypeId, "Insert");
                    }
                }
            }

            // AgentConfigs - Update if exists, else Insert
            if (partyMaster.AgentConfigs != null)
            {
                foreach (var incoming in partyMaster.AgentConfigs)
                {
                    if (incoming.Id > 0 && incoming.PartyId > 0)
                    {
                        var existingChildAgentConfig = existingParty.AgentConfigs
                            ?.FirstOrDefault(ac => ac.Id == incoming.Id && ac.PartyId == Id);

                        if (existingChildAgentConfig != null)
                        {
                            existingChildAgentConfig.SettlementCycleId = incoming.SettlementCycleId;
                            existingChildAgentConfig.TdsApplicable = incoming.TdsApplicable;
                            existingChildAgentConfig.TdsCode = incoming.TdsCode;
                            existingChildAgentConfig.DefaultCommissionGl = incoming.DefaultCommissionGl;
                            existingChildAgentConfig.AgreementStartDate = incoming.AgreementStartDate;
                            existingChildAgentConfig.AgreementEndDate = incoming.AgreementEndDate;
                            existingChildAgentConfig.AgentPayableControlGl = incoming.AgentPayableControlGl;
                            existingChildAgentConfig.TargetAmount = incoming.TargetAmount;
                            existingChildAgentConfig.TargetPeriod = incoming.TargetPeriod;
                            existingChildAgentConfig.Status = incoming.Status;
                        }
                    }
                    else
                    {
                        existingParty.AgentConfigs ??= new List<AgentConfig>();
                        existingParty.AgentConfigs.Add(new AgentConfig
                        {
                            PartyId = existingParty.Id,
                            SettlementCycleId = incoming.SettlementCycleId,
                            TdsApplicable = incoming.TdsApplicable,
                            TdsCode = incoming.TdsCode,
                            DefaultCommissionGl = incoming.DefaultCommissionGl,
                            AgreementStartDate = incoming.AgreementStartDate,
                            AgreementEndDate = incoming.AgreementEndDate,
                            AgentPayableControlGl = incoming.AgentPayableControlGl,
                            TargetAmount = incoming.TargetAmount,
                            TargetPeriod = incoming.TargetPeriod,
                            Status = incoming.Status
                        });

                        await LogChange(existingParty.Id, "AgentConfig", "SettlementCycleId", "",
                            incoming.SettlementCycleId?.ToString() ?? "", "Insert");
                    }
                }
            }

            // TransportDetails - Update if exists, else Insert
            if (partyMaster.TransportDetails != null)
            {
                foreach (var incoming in partyMaster.TransportDetails)
                {
                    if (incoming.Id > 0 && incoming.PartyId > 0)
                    {
                        var existingChild = existingParty.TransportDetails?
                            .FirstOrDefault(td => td.Id == incoming.Id && td.PartyId == Id);

                        if (existingChild != null)
                        {
                            existingChild.TransportModeId = incoming.TransportModeId;
                            existingChild.VehicleTypeId = incoming.VehicleTypeId;
                            existingChild.DefaultFreightTypeId = incoming.DefaultFreightTypeId;
                            existingChild.DefaultFreightRate = incoming.DefaultFreightRate;
                            existingChild.LicenseNo = incoming.LicenseNo;
                            existingChild.LicenseExpiryDate = incoming.LicenseExpiryDate;
                            existingChild.VehicleNo = incoming.VehicleNo;
                            existingChild.Status = incoming.Status;
                        }
                    }
                    else
                    {
                        existingParty.TransportDetails ??= new List<TransportDetail>();
                        existingParty.TransportDetails.Add(new TransportDetail
                        {
                            PartyId = existingParty.Id,
                            TransportModeId = incoming.TransportModeId,
                            VehicleTypeId = incoming.VehicleTypeId,
                            DefaultFreightTypeId = incoming.DefaultFreightTypeId,
                            DefaultFreightRate = incoming.DefaultFreightRate,
                            LicenseNo = incoming.LicenseNo,
                            LicenseExpiryDate = incoming.LicenseExpiryDate,
                            VehicleNo = incoming.VehicleNo,
                            Status = incoming.Status
                        });

                        await LogChange(existingParty.Id, "TransportDetail", "VehicleNo", "",
                            incoming.VehicleNo ?? "", "Insert");
                    }
                }
            }

            //  _applicationDbContext.PartyMaster.Update(existingParty);

            var result = await _applicationDbContext.SaveChangesAsync();

            // success even if 0 changes (no modification detected)
            return result >= 0;
        }

        public async Task<bool> LogChange(int partyId, string tableName, string columnName, string oldValue, string newValue, string actionType)
        {
            var log = new PartyActivityLog
            {
                PartyId = partyId,
                TableName = tableName,
                ColumnName = columnName,
                OldValue = oldValue ?? "",
                NewValue = newValue ?? "",
                ActionType = actionType,
                ChangedBy = _ipAddressService.GetUserId(),
                ChangedByName = _ipAddressService.GetUserName(),
                ChangedIp = _ipAddressService.GetSystemIPAddress(),
                ChangedOn = DateTimeOffset.UtcNow
            };
            await _applicationDbContext.PartyActivityLog.AddAsync(log);

            return true;

        }
        //Track changes for PartyMaster root & child entity (all fields handled automatically)
        private async Task TrackChanges<T>(T existingEntity, T newEntity, int partyId, string tableName)
        {
            var entityType = typeof(T);
            var properties = entityType.GetProperties();

            // Fields we don't want to track (audit/system)
            var ignoreProps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "CreatedBy", "CreatedByName", "CreatedDate", "CreatedIP",
                "ModifiedBy", "ModifiedByName", "ModifiedDate", "ModifiedIP"
            };

            foreach (var prop in properties)
            {
                //  Skip navigation properties (collections / complex entities)
                if ((typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType)
                    && prop.PropertyType != typeof(string))
                    || (prop.PropertyType.IsClass && prop.PropertyType != typeof(string)))
                    continue;

                // Skip system/audit fields
                if (ignoreProps.Contains(prop.Name))
                    continue;

                var existingValue = prop.GetValue(existingEntity)?.ToString() ?? "";
                var newValue = prop.GetValue(newEntity)?.ToString() ?? "";

                if (existingValue != newValue)
                {
                    await LogChange(partyId, tableName, prop.Name, existingValue, newValue, "Update");
                }
            }
        }

        public async Task<bool> ExistsAsync(string partyname)
        {
            return await _applicationDbContext.PartyMaster
                    .AnyAsync(p => EF.Functions.Collate(p.PartyName.Trim().ToLower(), "SQL_Latin1_General_CP1_CI_AS")
                    == partyname.Trim().ToLower()
                   );
        }

        public async Task<bool> ExistsForUpdateAsync(string partyName, int id)
        {
            return await _applicationDbContext.PartyMaster
                .AnyAsync(p =>
                    EF.Functions.Collate(p.PartyName.Trim().ToLower(), "SQL_Latin1_General_CP1_CI_AS")
                        == partyName.Trim().ToLower()
                    && p.Id != id
                );
        }
        public async Task<bool> GstNumberExistsAsync(string gstNumber, int excludePartyId = 0)
        {
            return await _applicationDbContext.PartyMaster
                .AnyAsync(p => p.GSTNumber == gstNumber && p.Id != excludePartyId);
        }
        public async Task<bool> FinalizePartyStatus(PartyManagement.Domain.Entities.PartyMaster partyMaster)
        {
            var existingParty = await _applicationDbContext.PartyMaster
                .FirstOrDefaultAsync(p => p.Id == partyMaster.Id);

            if (existingParty != null)
            {
                // Update status fields
                existingParty.PartyStatus = partyMaster.PartyStatus;
                existingParty.StatusId = partyMaster.StatusId;

                // Save changes
                return await _applicationDbContext.SaveChangesAsync() > 0;
            }

            return false;
        }

        public async Task<PartyManagement.Domain.Entities.PartyMaster> GetByIdAsync(int partyId)
        {
            return await _applicationDbContext.PartyMaster
                .FirstOrDefaultAsync(p => p.Id == partyId);
        }

        public async Task<bool> RollbackStatusAsync(int id)
        {
            var partyMaster = await _applicationDbContext.PartyMaster.FindAsync(id);
            if (partyMaster == null)
                return false;

            // Get Pending StatusId dynamically
            var pendingStatusId = await (
                from m in _applicationDbContext.MiscMaster
                join mt in _applicationDbContext.MiscTypeMaster
                    on m.MiscTypeId equals mt.Id
                where mt.MiscTypeCode == "ApprovalStatus" && m.Code == "Pending"
                select m.Id
            ).FirstOrDefaultAsync();

            // Rollback
            partyMaster.StatusId = pendingStatusId;

            _applicationDbContext.PartyMaster.Update(partyMaster);
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }
        public async Task<PartyMasterWorkFlowDto> GetByIdPartyMasterWorkFlowAsync(int id)
        {
            var entity = await _applicationDbContext.PartyMaster
            .Where(x => x.Id == id)
            .Select(x => new PartyMasterWorkFlowDto
            {
                Id = x.Id,
                PartyName = x.PartyName,
                PartyCode = x.PartyCode,
                StatusId = x.StatusId,
                PartyStatus = x.PartyStatus,

                UnitId = x.UnitId,
                // add other fields from entity → dto mapping
            })
            .FirstOrDefaultAsync();


            return entity!;
        }

        public async Task<PartyManagement.Domain.Entities.PartyMaster> GetByIdWithContactsAsync(int partyId)
        {
            return await _applicationDbContext.PartyMaster
                .Include(p => p.PartyContactTypes)
                .FirstOrDefaultAsync(p => p.Id == partyId);
        }

        public async Task<PartyContact> GetPrimaryContactAsync(int partyId)
        {
            return await _applicationDbContext.Set<PartyContact>()
                .Where(c => c.PartyId == partyId)
                .OrderByDescending(c => c.ContactBy != null &&
                                        c.ContactBy.Trim().Equals("Primary", StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(c => !string.IsNullOrWhiteSpace(c.EmailID) ||
                                    !string.IsNullOrWhiteSpace(c.MobileNo))
                .FirstOrDefaultAsync();
        }



        public async Task<bool> EmailExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return await (
                from c in _applicationDbContext.PartyContact
                join p in _applicationDbContext.PartyMaster
                    on c.PartyId equals p.Id
                where p.IsActive == PartyManagement.Domain.Common.BaseEntity.Status.Active   // ✅ compare with enum
                    && c.EmailID != null
                    && EF.Functions.Collate(
                            c.EmailID.Trim(),
                            "SQL_Latin1_General_CP1_CI_AS"
                        ) == EF.Functions.Collate(
                            email.Trim(),
                            "SQL_Latin1_General_CP1_CI_AS"
                        )
                select c.Id
            ).AnyAsync();
        }
        public async Task<bool> MobileExistsAsync(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile))
                return false;

            return await (
                from c in _applicationDbContext.PartyContact
                join p in _applicationDbContext.PartyMaster
                    on c.PartyId equals p.Id
                where p.IsActive == PartyManagement.Domain.Common.BaseEntity.Status.Active      // ✅ Only Active Parties
                    && c.MobileNo != null
                    && EF.Functions.Collate(
                            c.MobileNo.Trim(),
                            "SQL_Latin1_General_CP1_CI_AS"
                        ) == EF.Functions.Collate(
                            mobile.Trim(),
                            "SQL_Latin1_General_CP1_CI_AS"
                        )
                select c.Id
            ).AnyAsync();
        }

        public async Task<bool> EmailExistsUpdateAsync(string email, int excludePartyId)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return await (
                from c in _applicationDbContext.PartyContact
                join p in _applicationDbContext.PartyMaster
                    on c.PartyId equals p.Id
                where p.IsActive == PartyManagement.Domain.Common.BaseEntity.Status.Active
                    && p.Id != excludePartyId                               // ✅ Exclude current record
                    && c.EmailID != null
                    && EF.Functions.Collate(c.EmailID.Trim(), "SQL_Latin1_General_CP1_CI_AS") ==
                        EF.Functions.Collate(email.Trim(), "SQL_Latin1_General_CP1_CI_AS")
                select c.Id
            ).AnyAsync();
        }

        public async Task<bool> MobileExistsUpdateAsync(string mobile, int excludePartyId)
        {
            if (string.IsNullOrWhiteSpace(mobile))
                return false;

            return await (
                from c in _applicationDbContext.PartyContact
                join p in _applicationDbContext.PartyMaster
                    on c.PartyId equals p.Id
                where p.IsActive == PartyManagement.Domain.Common.BaseEntity.Status.Active
                    && p.Id != excludePartyId                               // ✅ Exclude current record
                    && c.MobileNo != null
                    && EF.Functions.Collate(c.MobileNo.Trim(), "SQL_Latin1_General_CP1_CI_AS") ==
                        EF.Functions.Collate(mobile.Trim(), "SQL_Latin1_General_CP1_CI_AS")
                select c.Id
            ).AnyAsync();
        }

        // in PartyMasterCommandRepository
        public async Task<List<string>> GetPartyTypeCodesAsync(int partyId)
        {
            // Assuming PartyType.PartyTypeId -> MiscMaster.Id and MiscTypeMaster.MiscTypeCode = 'PartyType'
            var q =
                from pt in _applicationDbContext.PartyType
                join mm in _applicationDbContext.MiscMaster on pt.PartyTypeId equals mm.Id
                join mt in _applicationDbContext.MiscTypeMaster on mm.MiscTypeId equals mt.Id
                where pt.PartyId == partyId && mt.MiscTypeCode == "PartyType"
                select mm.Code;

            return await q.Distinct().ToListAsync();
        }

       
         

    }
}
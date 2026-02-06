using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PartyManagement.Application.PartyMaster.Command.CreatePartyMaster;
using PartyManagement.Application.PartyMaster.Command.DeletePartyMaster;
using PartyManagement.Application.PartyMaster.Command.UpdatePartyMaster;
using PartyManagement.Application.PartyMaster.Queries.GetPartyMasterById;
using PartyManagement.Domain.Entities;
using static PartyManagement.Application.PartyMaster.Command.CreatePartyMaster.CreatePartyMasterDto;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.Application.Common.Mappings
{
    public class PartyMasterProfile : Profile
    {
        public PartyMasterProfile()
        {
            //Create DTO
            CreateMap<CreatePartyMasterDto, PartyManagement.Domain.Entities.PartyMaster>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
             .ForMember(dest => dest.PartyUnitCompanyMappings,
              opt => opt.MapFrom(src => src.PartyUnitCompanies))
            .ForMember(dest => dest.PartyAddressTypes, opt => opt.MapFrom(src => src.PartyAddresses))
            .ForMember(dest => dest.PartyBankTypes, opt => opt.MapFrom(src => src.PartyBanks))
            .ForMember(dest => dest.PartyContactTypes, opt => opt.MapFrom(src => src.PartyContacts))
            .ForMember(dest => dest.PartyDocumentTypes, opt =>
                {
                    opt.PreCondition(src => src.PartyDocuments != null && src.PartyDocuments.Any());
                    opt.MapFrom(src => src.PartyDocuments);
                })
            .ForMember(dest => dest.IsMsmeCompliant, opt => opt.MapFrom(src => src.IsMsmeCompliant == 1 ? true : false))
            .ForMember(dest => dest.IsTDSApplicable, opt => opt.MapFrom(src => src.IsTDSApplicable == 1 ? true : false))
            .ForMember(dest => dest.IsTCSApplicable, opt => opt.MapFrom(src => src.IsTCSApplicable == 1 ? true : false))
            .ForMember(dest => dest.IsGstReverseCharge, opt => opt.MapFrom(src => src.IsGstReverseCharge == 1 ? true : false))
            .ForMember(dest => dest.Is206AB206CCAApplicable, opt => opt.MapFrom(src => src.Is206AB206CCAApplicable == 1 ? true : false))
            .ForMember(dest => dest.IsInternalSupplier, opt => opt.MapFrom(src => src.IsInternalSupplier == 1 ? true : false))
            .ForMember(dest => dest.IsInternalCustomer, opt => opt.MapFrom(src => src.IsInternalCustomer == 1 ? true : false))
            .ForMember(dest => dest.IsStopPayment, opt => opt.MapFrom(src => src.IsStopPayment == 1 ? true : false))
            .ForMember(dest => dest.IsGroup, opt => opt.MapFrom(src => src.IsGroup == 1 ? true : false))
            .ForMember(dest => dest.IsSubsidiary, opt => opt.MapFrom(src => src.IsSubsidiary == 1 ? true : false))
            .ForMember(dest => dest.PartyStatus, opt => opt.MapFrom(src => "Pending"))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
            .ForMember(dest => dest.IsPortalAccessEnabled, opt => opt.MapFrom(src => src.IsPortalAccessEnabled == 1 ? true : false))
           // .ForMember(dest => dest.IsPortalAccessEnabled, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.IsUpdate, opt => opt.MapFrom(src => 0));
            CreateMap<PartyContactDto, PartyManagement.Domain.Entities.PartyContact>();
            CreateMap<PartyAddressDto, PartyManagement.Domain.Entities.PartyAddress>();
            CreateMap<PartyBankDto, PartyManagement.Domain.Entities.PartyBank>();
            CreateMap<PartyTypeDto, PartyManagement.Domain.Entities.PartyType>();
            CreateMap<PartyDocumentDto, PartyManagement.Domain.Entities.PartyDocument>();
            CreateMap<PartyUnitCompanyDto, PartyManagement.Domain.Entities.PartyUnitCompanyMapping>();

            //Update DTO
            CreateMap<UpdatePartyMasterDto, PartyManagement.Domain.Entities.PartyMaster>()
            .ForMember(dest => dest.PartyTypes, opt => opt.MapFrom(src => src.PartyTypesUpdate))
            .ForMember(dest => dest.PartyUnitCompanyMappings, opt => opt.MapFrom(src => src.PartyUnitCompaniesUpdate))
            .ForMember(dest => dest.PartyContactTypes, opt => opt.MapFrom(src => src.PartyContactsUpdate))
            .ForMember(dest => dest.PartyAddressTypes, opt => opt.MapFrom(src => src.PartyAddressesUpdate))
            .ForMember(dest => dest.PartyBankTypes, opt => opt.MapFrom(src => src.PartyBanksUpdate))
             .ForMember(dest => dest.PartyDocumentTypes, opt =>
                {
                    opt.PreCondition(src => src.PartyDocumentsUpdate != null && src.PartyDocumentsUpdate.Any());
                    opt.MapFrom(src => src.PartyDocumentsUpdate);
                })
            //.ForMember(dest => dest.PartyDocumentTypes, opt => opt.MapFrom(src => src.PartyDocumentsUpdate))
            .ForMember(dest => dest.IsTDSApplicable, opt => opt.MapFrom(src => src.IsTDSApplicable == 1 ? true : false))
            .ForMember(dest => dest.IsTCSApplicable, opt => opt.MapFrom(src => src.IsTCSApplicable == 1 ? true : false))
            .ForMember(dest => dest.IsGstReverseCharge, opt => opt.MapFrom(src => src.IsGstReverseCharge == 1 ? true : false))
            .ForMember(dest => dest.Is206AB206CCAApplicable, opt => opt.MapFrom(src => src.Is206AB206CCAApplicable == 1 ? true : false))
            .ForMember(dest => dest.IsInternalSupplier, opt => opt.MapFrom(src => src.IsInternalSupplier == 1 ? true : false))
            .ForMember(dest => dest.IsInternalCustomer, opt => opt.MapFrom(src => src.IsInternalCustomer == 1 ? true : false))
            .ForMember(dest => dest.IsStopPayment, opt => opt.MapFrom(src => src.IsStopPayment == 1 ? true : false))
             .ForMember(dest => dest.IsGroup, opt => opt.MapFrom(src => src.IsGroup == 1 ? true : false))
            .ForMember(dest => dest.IsSubsidiary, opt => opt.MapFrom(src => src.IsSubsidiary == 1 ? true : false))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive))
            .ForMember(dest => dest.IsPortalAccessEnabled, opt => opt.MapFrom(src => src.IsPortalAccessEnabled == 1 ? true : false))
            .ForMember(dest => dest.IsUpdate, opt => opt.MapFrom(src => 1));
            CreateMap<UpdatePartyMasterDto.UpdatePartyTypeDto, PartyType>();
            CreateMap<UpdatePartyMasterDto.UpdatePartyContactDto, PartyContact>();
            CreateMap<UpdatePartyMasterDto.UpdatePartyAddressDto, PartyAddress>();
            CreateMap<UpdatePartyMasterDto.UpdatePartyBankDto, PartyBank>();
            CreateMap<UpdatePartyMasterDto.UpdatePartyDocumentDto, PartyDocument>();
            CreateMap<UpdatePartyMasterDto.UpdatePartyUniCompanyDto, PartyUnitCompanyMapping>();

            //Delete 

            CreateMap<DeletePartyMasterCommand, PartyManagement.Domain.Entities.PartyMaster>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

            //For Workflow Approval
            // CreateMap<PartyMasterWorkFlowDto, CreatePartyMasterReverseDto>()
            //     .ForMember(dest => dest.Header, opt => opt.MapFrom(src => src))
            //     .ForMember(dest => dest.Lines, opt => opt.MapFrom(src => new List<PartyMasterWorkFlowDto> { src }));
        
        // For Workflow Approval
                CreateMap<PartyMasterWorkFlowDto, CreatePartyMasterReverseDto>()
                    .ForMember(dest => dest.Header, opt => opt.MapFrom(src => src))
                    .ForMember(dest => dest.Lines, opt => opt.MapFrom(src => new List<PartyMasterWorkFlowDto>())); 
                }
    }
}
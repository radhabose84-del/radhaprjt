using AutoMapper;
// using Contracts.Events.Notifications;
using UserManagement.Application.Units.Commands.CreateUnit;
using UserManagement.Application.Units.Commands.DeleteUnit;
using UserManagement.Application.Units.Queries.GetUnits;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Common.Mappings
{
    public class UnitsProfile :Profile
    {
        public UnitsProfile()
        {
            CreateMap<UnitsDto, Unit>()
              .ForMember(dest => dest.Id, opt => opt.Ignore())
             .ForMember(dest => dest.UnitAddress, opt => opt.MapFrom(src => src.UnitAddressDto))
             .ForMember(dest => dest.UnitContacts, opt => opt.MapFrom(src => src.UnitContactsDto))
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
             .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<CreateUnitCommand, Unit>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UnitAddress, opt => opt.MapFrom(src => src.UnitAddressDto))
            .ForMember(dest => dest.UnitContacts, opt => opt.MapFrom(src => src.UnitContactsDto))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));


            CreateMap<UnitAddressDto, UnitAddress>()
           .ForMember(dest => dest.Id, opt => opt.Ignore())
           .ForMember(dest => dest.AddressLine1, opt => opt.MapFrom(src => src.AddressLine1))
           .ForMember(dest => dest.AddressLine2, opt => opt.MapFrom(src => src.AddressLine2))
           .ForMember(dest => dest.PinCode, opt => opt.MapFrom(src => src.PinCode))
           .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.CountryId))
           .ForMember(dest => dest.StateId, opt => opt.MapFrom(src => src.StateId))
           .ForMember(dest => dest.CityId, opt => opt.MapFrom(src => src.CityId))
           .ForMember(dest => dest.ContactNumber, opt => opt.MapFrom(src => src.ContactNumber))
           .ForMember(dest => dest.AlternateNumber, opt => opt.MapFrom(src => src.AlternateNumber))
           .ReverseMap();

            CreateMap<UnitContactsDto, UnitContacts>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Designation, opt => opt.MapFrom(src => src.Designation))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNo, opt => opt.MapFrom(src => src.PhoneNo))
            .ForMember(dest => dest.Remarks, opt => opt.MapFrom(src => src.Remarks))
            .ReverseMap();

            CreateMap<UnitAddress, UnitAddressDto>()
           .ForMember(dest => dest.AddressLine1, opt => opt.MapFrom(src => src.AddressLine1))
           .ForMember(dest => dest.AddressLine2, opt => opt.MapFrom(src => src.AddressLine2))
           .ForMember(dest => dest.PinCode, opt => opt.MapFrom(src => src.PinCode))
           .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.CountryId))
           .ForMember(dest => dest.StateId, opt => opt.MapFrom(src => src.StateId))
           .ForMember(dest => dest.CityId, opt => opt.MapFrom(src => src.CityId))
           .ForMember(dest => dest.ContactNumber, opt => opt.MapFrom(src => src.ContactNumber))
           .ForMember(dest => dest.AlternateNumber, opt => opt.MapFrom(src => src.AlternateNumber));

            CreateMap<UnitContacts, UnitContactsDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Designation, opt => opt.MapFrom(src => src.Designation))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNo, opt => opt.MapFrom(src => src.PhoneNo))
            .ForMember(dest => dest.Remarks, opt => opt.MapFrom(src => src.Remarks));

            CreateMap<Unit, UnitsDto>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.UnitName));

            CreateMap<UpdateUnitsDto, Unit>()
                 .ForMember(dest => dest.UnitAddress, opt => opt.MapFrom(src => src.UnitAddressDto))
                 .ForMember(dest => dest.UnitContacts, opt => opt.MapFrom(src => src.UnitContactsDto))
                 .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<Unit, GetUnitsDTO>();
            // .ForMember(dest => dest.UnitAddressDto, opt => opt.MapFrom(src => src.UnitAddress))
            // .ForMember(dest => dest.UnitContactsDto, opt => opt.MapFrom(src => src.UnitContacts));


            CreateMap<DeleteUnitCommand, Unit>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UnitId))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

            CreateMap<Unit, UnitAutoCompleteDTO>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.UnitName));


            CreateMap<Unit, GetUnitsByIdDto>()
            .ForMember(dest => dest.UnitAddressDto, opt => opt.MapFrom(src => src.UnitAddress))
            .ForMember(dest => dest.UnitContactsDto, opt => opt.MapFrom(src => src.UnitContacts));


                 
        }
    }
}
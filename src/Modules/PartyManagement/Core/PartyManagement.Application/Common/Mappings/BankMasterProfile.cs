
using AutoMapper;
using PartyManagement.Domain.Common;

namespace PartyManagement.Application.BankMaster.Mapping;

public class BankMasterProfile : Profile
{
    public BankMasterProfile()
    {
        CreateMap<PartyManagement.Domain.Entities.BankMaster, BankMasterDto>()
            .ForMember(d => d.IsActive,  m => m.MapFrom(s => (int)s.IsActive))
            .ForMember(d => d.IsDeleted, m => m.MapFrom(s => (int)s.IsDeleted));

        CreateMap<CreateBankMasterDto, PartyManagement.Domain.Entities.BankMaster>()
            .ForMember(d => d.IsDeleted, m => m.MapFrom(_ => BaseEntity.IsDelete.NotDeleted))
            .ForMember(d => d.IsActive,  m => m.MapFrom(_ => BaseEntity.Status.Active))
            .ForMember(d => d.CreatedDate, m => m.MapFrom(_ => DateTimeOffset.UtcNow));

        CreateMap<UpdateBankMasterDto, PartyManagement.Domain.Entities.BankMaster>()
            .ForMember(d => d.ModifiedDate, m => m.MapFrom(_ => DateTimeOffset.UtcNow));
    }
}

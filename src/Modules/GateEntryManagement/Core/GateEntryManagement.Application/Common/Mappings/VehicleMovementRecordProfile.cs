using AutoMapper;
using GateEntryManagement.Application.VehicleMovementRecord.Commands.CreateVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Commands.UpdateVehicleMovementRecord;
using static GateEntryManagement.Domain.Common.BaseEntity;

namespace GateEntryManagement.Application.Common.Mappings
{
    public class VehicleMovementRecordProfile : Profile
    {
        public VehicleMovementRecordProfile()
        {
            CreateMap<CreateVehicleMovementRecordCommand, Domain.Entities.VehicleMovementRecord>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateVehicleMovementRecordCommand, Domain.Entities.VehicleMovementRecord>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}

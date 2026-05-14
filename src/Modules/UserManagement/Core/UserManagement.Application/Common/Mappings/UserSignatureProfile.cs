using AutoMapper;
using UserManagement.Application.UserSignature.Queries.GetAllUserSignature;
using UserManagement.Application.UserSignature.Queries.GetUserSignatureById;

namespace UserManagement.Application.Common.Mappings
{
    public class UserSignatureProfile : Profile
    {
        public UserSignatureProfile()
        {
            CreateMap<UserManagement.Domain.Entities.UserSignature, GetAllUserSignatureDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                    src.User == null
                        ? string.Empty
                        : ((src.User.FirstName ?? string.Empty) + " " + (src.User.LastName ?? string.Empty)).Trim()))
                .ForMember(dest => dest.EmailId, opt => opt.MapFrom(src => src.User == null ? null : src.User.EmailId));

            CreateMap<UserManagement.Domain.Entities.UserSignature, UserSignatureByIdDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                    src.User == null
                        ? string.Empty
                        : ((src.User.FirstName ?? string.Empty) + " " + (src.User.LastName ?? string.Empty)).Trim()))
                .ForMember(dest => dest.EmailId, opt => opt.MapFrom(src => src.User == null ? null : src.User.EmailId))
                .ForMember(dest => dest.ImageBase64, opt => opt.Ignore()); // populated by query handler from disk
        }
    }
}

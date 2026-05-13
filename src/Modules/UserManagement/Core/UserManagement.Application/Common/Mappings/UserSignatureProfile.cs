using AutoMapper;
using UserManagement.Application.UserSignature.Command.CreateUserSignature;
using UserManagement.Application.UserSignature.Command.DeleteUserSignature;
using UserManagement.Application.UserSignature.Command.UpdateUserSignature;
using UserManagement.Application.UserSignature.Queries.GetAllUserSignature;
using UserManagement.Application.UserSignature.Queries.GetUserSignatureById;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Common.Mappings
{
    public class UserSignatureProfile : Profile
    {
        public UserSignatureProfile()
        {
            CreateMap<CreateUserSignatureCommand, UserManagement.Domain.Entities.UserSignature>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => IsDelete.NotDeleted));

            CreateMap<UpdateUserSignatureCommand, UserManagement.Domain.Entities.UserSignature>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            CreateMap<DeleteUserSignatureCommand, UserManagement.Domain.Entities.UserSignature>()
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => IsDelete.Deleted));

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
                .ForMember(dest => dest.SignatureBase64, opt => opt.MapFrom(src =>
                    src.SignatureImage == null ? null : Convert.ToBase64String(src.SignatureImage)));
        }
    }
}

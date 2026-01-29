using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces;
using Core.Application.Common.Interfaces.IUser;
using Core.Application.Users.Commands.CreateFirstTimeUserPassword;
using Core.Domain.Entities;
using MediatR;

namespace Core.Application.Users.Commands.ChangeUserPassword
{
    public class ChangeUserPasswordCommandHandler : IRequestHandler<ChangeUserPasswordCommand, ApiResponseDTO<string>>
    {
        private readonly IMapper _imapper;
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly IChangePassword _ichangePassword;
        public ChangeUserPasswordCommandHandler(IMapper imapper, IUserQueryRepository userQueryRepository, IChangePassword ichangePassword)
        {
            _imapper = imapper;
            _userQueryRepository = userQueryRepository;
            _ichangePassword = ichangePassword;
        }

        public async Task<ApiResponseDTO<string>> Handle(ChangeUserPasswordCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.NewPassword))
                return new ApiResponseDTO<string>
                {
                    IsSuccess = false,
                    Message = "Invalid input parameters."
                };

            var passwordLog = _imapper.Map<PasswordLog>(request);

            passwordLog.PasswordHash = await _ichangePassword.PasswordEncode(request.NewPassword);
            var changedPassword = await _ichangePassword.ChangePassword(request.UserId, request.NewPassword, passwordLog);

            if (!changedPassword)
            {
                return new ApiResponseDTO<string> { IsSuccess = false, Message = "Try a different Password" };
            }
            return new ApiResponseDTO<string> { IsSuccess = true, Message = "Password changed successfully." };

        }
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces;
using Core.Application.Common.Interfaces.IUser;
using Core.Application.Common.Utilities;
using Core.Application.Users.Queries.GetUsers;
using Core.Domain.Entities;
using MediatR;

namespace Core.Application.Users.Commands.ResetUserPassword
{
    public class ResetUserPasswordCommandHandler : IRequestHandler<ResetUserPasswordCommand, ApiResponseDTO<string>>
    {
        private readonly IMapper _mapper;
        private readonly IChangePassword _changePassword;
        private readonly IUserQueryRepository _userQueryRepository;
        private readonly ITimeZoneService _timeZoneService;

        public ResetUserPasswordCommandHandler(
            IMapper mapper,
            IChangePassword changePassword,
            IUserQueryRepository userRepository, ITimeZoneService timeZoneService)
        {
            _mapper = mapper;
            _changePassword = changePassword;
            _userQueryRepository = userRepository;
            _timeZoneService = timeZoneService;
        }

        public async Task<ApiResponseDTO<string>> Handle(ResetUserPasswordCommand request, CancellationToken cancellationToken)
        {
             var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
             var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId); 
            
            var user = await _userQueryRepository.GetByUsernameAsync(request.UserName);
            

            var passwordLog = new PasswordLogDTO
            {
                UserId = user.UserId,
                UserName = user.UserName,
                PasswordHash = await _changePassword.PasswordEncode(request.Password),
                CreatedAt = currentTime
            };


            var passwordLogMap = _mapper.Map<PasswordLog>(passwordLog);
            var result = await _changePassword.ResetUserPassword(user.UserId,passwordLogMap);
            bool log = await _changePassword.PasswordLog(passwordLogMap);
           
                // Remove the verification code after successful password reset
                ForgotPasswordCache.CodeStorage.Remove(request.UserName);

                return new ApiResponseDTO<string> { IsSuccess = true, Message = result};
            
        }
    }
}

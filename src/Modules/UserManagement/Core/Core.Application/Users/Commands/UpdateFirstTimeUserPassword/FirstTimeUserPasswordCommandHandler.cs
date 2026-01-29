using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces;
using Core.Application.Common.Interfaces.IUser;
using Core.Domain.Entities;
using MediatR;

namespace Core.Application.Users.Commands.UpdateFirstTimeUserPassword
{
    public class FirstTimeUserPasswordCommandHandler : IRequestHandler<FirstTimeUserPasswordCommand, ApiResponseDTO<string>>
    {
        private readonly IMapper _imapper;
        private readonly IChangePassword _ichangePassword;
        private readonly IUserQueryRepository _userQueryRepository;
        public FirstTimeUserPasswordCommandHandler(IMapper imapper, IChangePassword ichangePassword, IUserQueryRepository userRepository)
        {
            _imapper = imapper;
            _ichangePassword = ichangePassword;
            _userQueryRepository = userRepository;
            
        }
        public async Task<ApiResponseDTO<string>> Handle(FirstTimeUserPasswordCommand request, CancellationToken cancellationToken)
        {
           
                
                var user = await _userQueryRepository.GetByIdAsync(request.UserId);
                
                 if ( !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                 {
                       
                      var passwordLog = _imapper.Map<PasswordLog>(request);
                      
                      passwordLog.PasswordHash = await _ichangePassword.PasswordEncode(request.Password);
                      
                      var changedPasswordLog = await _ichangePassword.FirstTimeUserChangePassword(request.UserId,passwordLog);

                      if (changedPasswordLog)
                      {
                          return new ApiResponseDTO<string> { IsSuccess = true, Message = "Password changed successfully."};
                      }

                     return new ApiResponseDTO<string> { IsSuccess = false, Message = "Password change failed."}; 
                 }
                 
                 return new ApiResponseDTO<string> { IsSuccess = false, Message = "Your input password should not match the default password. Please try a different password."};    
           
        }
    }
}
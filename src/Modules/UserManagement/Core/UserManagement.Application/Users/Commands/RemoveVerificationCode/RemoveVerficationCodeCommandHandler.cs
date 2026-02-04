using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Common.Interfaces.IUser;
using MediatR;

namespace UserManagement.Application.Users.Commands.RemoveVerificationCode
{
    public class RemoveVerficationCodeCommandHandler : IRequestHandler<RemoveVerficationCodeCommand, ApiResponseDTO<bool>>
    {
        private readonly IUserCommandRepository _userCommandRepository;
        public RemoveVerficationCodeCommandHandler(IUserCommandRepository userCommandRepository)
        {
            _userCommandRepository = userCommandRepository;
        }
        public async Task<ApiResponseDTO<bool>> Handle(RemoveVerficationCodeCommand request, CancellationToken cancellationToken)
        {
            var result = _userCommandRepository.RemoveVerficationCode(request.UserName);
            return new ApiResponseDTO<bool> { IsSuccess = true, Data = true };
        }
    }
}
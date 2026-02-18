#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
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
        #pragma warning disable CS1998
        }
        #pragma warning restore CS1998
        #pragma warning disable CS1998
        public async Task<ApiResponseDTO<bool>> Handle(RemoveVerficationCodeCommand request, CancellationToken cancellationToken)
        #pragma warning restore CS1998
        {
            var result = _userCommandRepository.RemoveVerficationCode(request.UserName);
            return new ApiResponseDTO<bool> { IsSuccess = true, Data = true };
        }
    }
}
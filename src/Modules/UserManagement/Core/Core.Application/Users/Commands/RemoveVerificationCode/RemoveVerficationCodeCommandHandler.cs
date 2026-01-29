using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.IUser;
using MediatR;

namespace Core.Application.Users.Commands.RemoveVerificationCode
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
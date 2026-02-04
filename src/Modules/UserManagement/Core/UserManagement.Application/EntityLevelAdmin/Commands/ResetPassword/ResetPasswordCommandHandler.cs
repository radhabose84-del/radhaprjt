using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Application.Common.Utilities;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.EntityLevelAdmin.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
    {
         private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IUserCommandRepository _userRepository;
        private readonly IUserQueryRepository _userQueryRepository;
        public ResetPasswordCommandHandler(IMediator mediator, IMapper mapper, IUserCommandRepository userRepository, IUserQueryRepository userQueryRepository)
        {
            _mediator = mediator;
            _mapper = mapper;
            _userRepository = userRepository;
            _userQueryRepository = userQueryRepository;
        }
        public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
             
              var existingUser = await _userQueryRepository.GetByUsernameAsync(request.Email);
            if (existingUser == null)
            {
                throw new ValidationException("User not found.");
             
            }
            _mapper.Map(request, existingUser);

             var RowsUpdated = await _userRepository.SetAdminPassword(request.UserId, existingUser);
             if(RowsUpdated > 0)
             {
                 return RowsUpdated > 0;
             }
             throw new Exception("Password update failed.");
          
        }
    }
}
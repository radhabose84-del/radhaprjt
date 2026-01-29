using Core.Application.Users.Queries.GetUsers;
using Core.Domain.Entities;
using AutoMapper;
using MediatR;
using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.IUser;
using Core.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Core.Application.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery,UserByIdDTO>
    {
        private readonly IUserQueryRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<GetUserByIdQueryHandler> _logger;


        public GetUserByIdQueryHandler(IUserQueryRepository userRepository, IMapper mapper, IMediator mediator,ILogger<GetUserByIdQueryHandler> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        public async Task<UserByIdDTO> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching user details for UserId: {UserId}", request.UserId);
            
            // Fetch the user by ID
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User with UserId: {UserId} not found.", request.UserId);
                return null; // Or throw an exception if preferred
            }
            _logger.LogInformation("User details for UserId: {UserId} successfully fetched.", request.UserId);

           // Publish a domain event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: user.UserName,        
                    actionName: user.FirstName + " " + user.LastName,                
                    details: $"Fetched details for User '{user.UserName}'. Full Name: {user.FirstName} {user.LastName}",
                    module:"User"
                );

                await _mediator.Publish(domainEvent, cancellationToken);
                return _mapper.Map<UserByIdDTO>(user);

        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.IMenu;
using Core.Domain.Events;
using MediatR;

namespace Core.Application.Menu.Queries.GetMenu
{
    public class GetMenuQueryHandler : IRequestHandler<GetMenuQuery, ApiResponseDTO<List<MenuDto>>>
    {
        private readonly IMenuQuery _menuQuery;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetMenuQueryHandler(IMenuQuery menuQuery, IMapper mapper, IMediator mediator)
        {
            _menuQuery = menuQuery;
            _mapper = mapper;
            _mediator = mediator;
        }
        public async Task<ApiResponseDTO<List<MenuDto>>> Handle(GetMenuQuery request, CancellationToken cancellationToken)
        {
            var (Menus, totalCount) = await _menuQuery.GetAllMenuAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            var MenuList = _mapper.Map<List<MenuDto>>(Menus);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetMenus",
                actionCode: "",
                actionName: "",
                details: $"Menu details was fetched.",
                module: "Menu"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return new ApiResponseDTO<List<MenuDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = MenuList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        

    }
}
#nullable disable
using AutoMapper;
using UserManagement.Application.Common.Interfaces.IMenu;
using MediatR;

namespace UserManagement.Application.Menu.Queries.GetParentMenu
{
    public class GetParentMenuQueryHandler : IRequestHandler<GetParentMenuQuery, List<ParentMenuDto>>
    {
        private readonly IMenuQuery _menuQuery;
        private readonly IMapper _mapper;
        
        public GetParentMenuQueryHandler(IMenuQuery menuQuery, IMapper mapper)
        {
            _menuQuery = menuQuery;
            _mapper = mapper;
        }
        public async Task<List<ParentMenuDto>> Handle(GetParentMenuQuery request, CancellationToken cancellationToken)
        {
            
             
            var result = await _menuQuery.GetParentMenuAutoComplete(request.SearchPattern);
            var MenuList = _mapper.Map<List<ParentMenuDto>>(result);
             
            return MenuList;  
        }
    }
}
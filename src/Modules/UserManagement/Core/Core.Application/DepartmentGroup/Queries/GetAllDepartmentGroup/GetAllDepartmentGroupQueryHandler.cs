using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.IDepartmentGroup;
using Core.Domain.Events;
using MediatR;

namespace Core.Application.DepartmentGroup.Queries.GetAllDepartmentGroup
{
    public class GetAllDepartmentGroupQueryHandler : IRequestHandler<GetAllDepartmentGroupQuery, ApiResponseDTO<List<GetAllDepartmentGroupDto>>>
    {
        private readonly IDepartmentGroupQueryRepository _departmentGroupRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllDepartmentGroupQueryHandler(IDepartmentGroupQueryRepository departmentGroupRepository, IMapper mapper, IMediator mediator)
        {
            _mapper = mapper;
            _departmentGroupRepository = departmentGroupRepository;
            _mediator = mediator;


        }
        public async Task<ApiResponseDTO<List<GetAllDepartmentGroupDto>>> Handle(GetAllDepartmentGroupQuery request, CancellationToken cancellationToken)
        {
          

            var (departmentGroups, totalCount) = await _departmentGroupRepository
                .GetAllDepartmentGroupAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            if (departmentGroups == null || !departmentGroups.Any())
            {
               

                return new ApiResponseDTO<List<GetAllDepartmentGroupDto>>
                {
                    IsSuccess = false,
                    Message = "No Record Found"
                };
            }

            var departmentGroupList = _mapper.Map<List<GetAllDepartmentGroupDto>>(departmentGroups);

            

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",
                actionName: "",
                details: "Department group details were fetched.",
                module: "DepartmentGroup"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

         

            return new ApiResponseDTO<List<GetAllDepartmentGroupDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = departmentGroupList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }



    }
}
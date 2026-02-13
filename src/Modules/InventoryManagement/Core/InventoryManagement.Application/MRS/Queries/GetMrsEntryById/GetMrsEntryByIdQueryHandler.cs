using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using InventoryManagement.Application.Common.Interfaces.IMRS;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.MRS.Queries.GetMrsEntryById
{
    public class GetMrsEntryByIdQueryHandler : IRequestHandler<GetMrsEntryByIdQuery, GetMrsEntryByIdDto>
    {
        private readonly IMrsEntryQueryRepository _iMrsEntryQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IDepartmentLookup _departmentLookup;
        public GetMrsEntryByIdQueryHandler(IMrsEntryQueryRepository iMrsEntryQueryRepository, IMapper mapper, IMediator mediator, IDepartmentLookup departmentLookup)
        {
            _iMrsEntryQueryRepository = iMrsEntryQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _departmentLookup = departmentLookup;

        }

        public async Task<GetMrsEntryByIdDto> Handle(GetMrsEntryByIdQuery request, CancellationToken cancellationToken)
        {
             // 1️⃣ Fetch Header + Details
            var dto = await _iMrsEntryQueryRepository.GetMrsDetailsById(request.Id);
            if (dto == null)
                throw new KeyNotFoundException("Mrs not found");

            // 2️⃣ Collect IDs
            var departmentIds = new List<int> { dto.DepartmentId, dto.SubDepartmentId }.Distinct().ToList();
            var uomIds = dto.MrsDetails.Select(x => x.UomId).Distinct().ToList();
            var itemIds = dto.MrsDetails.Select(x => x.ItemId).Distinct().ToList();

            // 3️⃣ Fire parallel lookup calls
            var departmentTask = _departmentLookup.GetAllDepartmentAsync();

            // 4️⃣ Await all together
            await Task.WhenAll(departmentTask);

            var departments = await departmentTask;
        
            // 5️⃣ Prepare lookup dictionaries
            var departmentById = departments.ToDictionary(d => d.DepartmentId, d => d.DepartmentName);


            // 6️⃣ Enrich Header
            dto.DepartmentName = departmentById.GetValueOrDefault(dto.DepartmentId, "NA");
            dto.SubDepartmentName = departmentById.GetValueOrDefault(dto.SubDepartmentId, "NA");

           

            // 8️⃣ Audit log
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetMrsEntryByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Mrs details {dto.Id} fetched.",
                module: "MrsEntry"
            );

            await _mediator.Publish(domainEvent, cancellationToken);
            return dto;
        }
    }
}

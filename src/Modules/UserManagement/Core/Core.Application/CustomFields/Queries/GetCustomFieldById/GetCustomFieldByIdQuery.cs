using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.CustomFields.Queries.GetCustomFieldById
{
    public class GetCustomFieldByIdQuery : IRequest<CustomFieldByIdDTO>
    {
        public int Id { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Language.Queries.GetLanguages;
using MediatR;

namespace UserManagement.Application.Language.Queries.GetLanguageById
{
    public class GetLanguageByIdQuery : IRequest<LanguageDTO>
    {
        public int Id { get; set; }
    }
}
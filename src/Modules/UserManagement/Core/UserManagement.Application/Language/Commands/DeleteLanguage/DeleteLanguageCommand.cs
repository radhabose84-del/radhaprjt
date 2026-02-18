using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Language.Commands.DeleteLanguage
{
    public class DeleteLanguageCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Language.Commands.UpdateLanguage
{
    public class UpdateLanguageCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public byte IsActive { get; set; }
    }
}
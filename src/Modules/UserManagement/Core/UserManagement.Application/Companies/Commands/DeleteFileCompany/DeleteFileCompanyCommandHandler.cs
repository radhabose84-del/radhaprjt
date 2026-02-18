#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.ICompany;
using MediatR;

namespace UserManagement.Application.Companies.Commands.DeleteFileCompany
{
    public class DeleteFileCompanyCommandHandler : IRequestHandler<DeleteFileCompanyCommand, bool>
    {
        private readonly IFileUploadService _ifileUploadService;
        public DeleteFileCompanyCommandHandler(IFileUploadService ifileUploadService)
        {
            _ifileUploadService = ifileUploadService;
        }

        public async Task<bool> Handle(DeleteFileCompanyCommand request, CancellationToken cancellationToken)
        {
            
           var result = await _ifileUploadService.DeleteFileAsync(request.Logo);
           return result;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces;
using Core.Application.Companies.Queries.GetCompanies;
using FluentValidation;
using MediatR;

namespace Core.Application.Companies.Commands.UploadFileCompany
{
    public class UploadFileCompanyCommandHandler : IRequestHandler<UploadFileCompanyCommand, GetCompanyDTO>
    {
         private readonly IFileUploadService _ifileUploadService;
         private readonly IMediator _mediator;
         private readonly IMapper _imapper;

        public UploadFileCompanyCommandHandler(IFileUploadService ifileUploadService, IMediator mediator, IMapper imapper)
        {
            _ifileUploadService = ifileUploadService;
            _mediator = mediator;
            _imapper = imapper;
        }

        public async Task<GetCompanyDTO> Handle(UploadFileCompanyCommand request, CancellationToken cancellationToken)
        {
            // var existingFile = await _ifileUploadService.GetFileSession();
            // if(existingFile != "Not Found")
            // {
            //     await _ifileUploadService.DeleteFileAsync(existingFile);
            // }
              string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources/AllFiles");
              if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
             var uploadResult = await _ifileUploadService.UploadFileAsync(request.File,  uploadPath);
             if (!uploadResult.IsSuccess)
             {
                throw new ValidationException("File not uploaded");
                 
             }

                var response = new GetCompanyDTO
                 {
                     Logo = uploadResult.FilePath,
                     LogoBase64 = uploadResult.logoBase64
                 };
            //   await _ifileUploadService.SetFileSession( uploadResult.FilePath);
             return response;
        }
    }
}
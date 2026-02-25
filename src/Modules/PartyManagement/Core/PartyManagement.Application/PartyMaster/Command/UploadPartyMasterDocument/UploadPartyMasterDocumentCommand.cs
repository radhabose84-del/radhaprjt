using MediatR;
using Microsoft.AspNetCore.Http;

namespace PartyManagement.Application.PartyMaster.Command.UploadPartyMasterDocument
{
    public class UploadPartyMasterDocumentCommand : IRequest<PartyDocumetDto>
    {
        public IFormFile? File { get; set; }  
    }
}
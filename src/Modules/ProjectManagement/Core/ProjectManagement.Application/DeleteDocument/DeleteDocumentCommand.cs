using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace ProjectManagement.Application.DeleteDocument
{
    public class DeleteDocumentCommand   : IRequest<bool>
    {
        public string? ProjectDocumentPath { get; set; }        
        public int Id { get; set; }     

        public int ProjectId { get; set; }   
        public string? FileName { get; set; }  
    }
}
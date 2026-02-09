using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ProjectManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using ProjectManagement.Application.ProjectMaster.Command.CreateProjectMaster;
using ProjectManagement.Application.ProjectMaster.Command.UpdateProjectMaster;
using ProjectManagement.Application.ProjectMaster.Queries.Dtos;
using ProjectManagement.Application.ProjectMaster.Queries.GetProjectMaster;
using ProjectManagement.Application.ProjectMaster.Queries.ProjectMasterAutoComplete;
using ProjectManagement.Domain.Entities;

namespace ProjectManagement.Application.Common.Mappings
{
    public class ProjectMasterProfile : Profile
    {
        public ProjectMasterProfile()
        {
            // ProjectMaster → ProjectMasterDto (includes documents)
            CreateMap<ProjectManagement.Domain.Entities.ProjectMaster, ProjectMasterDto>()
                .ForMember(dest => dest.Documents, opt => opt.MapFrom(src => src.ProjectDocuments));

            // Update DTO → ProjectMaster
            CreateMap<UpdateProjectMasterDto, ProjectManagement.Domain.Entities.ProjectMaster>()
                .ForMember(d => d.ProjectDocuments, opt => opt.MapFrom(src => src.Documents))
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.ProjectCode, opt => opt.Ignore())
                .ForMember(d => d.CreatedBy, opt => opt.Ignore())
                .ForMember(d => d.CreatedByName, opt => opt.Ignore())
                .ForMember(d => d.CreatedDate, opt => opt.Ignore())
                .ForMember(d => d.CreatedIP, opt => opt.Ignore());

            // ProjectDocument → ProjectDocumentDto
            CreateMap<ProjectDocument, ProjectDocumentDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DocumentId, opt => opt.MapFrom(src => src.DocumentId))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(dest => dest.UploadedDate, opt => opt.MapFrom(src => src.UploadedDate));

            // ProjectDocumentDto → ProjectDocument
            CreateMap<ProjectDocumentDto, ProjectDocument>();

            // Dapper GetProjectMaster → ProjectMasterDto
            CreateMap<GetProjectMasterDto, ProjectMasterDto>();
            CreateMap<GetProjectDocumentDto, ProjectDocumentDto>(); // <--- This fixes the nested Documents mapping

            // CreateProjectMaster → Entity
            CreateMap<CreateProjectMasterDto, ProjectManagement.Domain.Entities.ProjectMaster>()
                .ForMember(dest => dest.ProjectCode, opt => opt.Ignore());

            // AutoComplete DTO
            CreateMap<ProjectManagement.Domain.Entities.ProjectMaster, ProjectMasterAutoCompleteDto>();

            // Workflow DTO
            CreateMap<ProjectManagement.Domain.Entities.ProjectMaster, ProjectMasterWorkFlowDto>();
        }
    }
}
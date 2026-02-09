using AutoMapper;
using BudgetManagement.Application.BudgetRequest;
using BudgetManagement.Application.BudgetRequest.Commands.Create;
using BudgetManagement.Application.BudgetRequest.Commands.Update;

namespace BudgetManagement.Application.Common.Mappings;

public class BudgetRequestProfile : Profile
{
    public BudgetRequestProfile()
    {
        CreateMap<CreateBudgetRequestCommand, Domain.Entities.BudgetRequest>();
        CreateMap<UpdateBudgetRequestCommand,  Domain.Entities.BudgetRequest>();
        CreateMap<Domain.Entities.BudgetRequest, BudgetRequestDto>();
    }
}

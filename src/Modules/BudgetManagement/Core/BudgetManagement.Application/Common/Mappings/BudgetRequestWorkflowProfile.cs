using AutoMapper;
using BudgetManagement.Application.BudgetRequest.Commands.Create;


public class BudgetRequestWorkflowProfile : Profile
{
    public BudgetRequestWorkflowProfile()
    {
        CreateMap<BudgetRequestWorkFlowDto, CreateBudgetRequestReverseDto>()
            .ConvertUsing(src => new CreateBudgetRequestReverseDto
            {
                Header = src,
                Lines  = Array.Empty<BudgetRequestWorkFlowDto>() 
            });
    }
}

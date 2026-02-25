namespace UserManagement.Application.State.Queries.GetStates
{
    public class StateAutoCompleteDTO 
    {
        public int Id { get; set; }
        public string? StateCode { get; set; } 
        public string? StateName { get; set; }       
    }
}
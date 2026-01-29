using Core.Domain.Common;

namespace Core.Domain.Entities
{

    public class Division : BaseEntity
    {
       
        public int Id { get; set; }
        public string? ShortName { get; set; }
        public string? Name { get; set; }
        public int CompanyId { get; set; }
        public Company? Company { get; set; }
        public IList<UserDivision>? UserDivisions { get; set; }
        public IList<Unit>? Units { get; set; }

        
    }
}
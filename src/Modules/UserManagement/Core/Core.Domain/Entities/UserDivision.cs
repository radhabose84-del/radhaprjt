using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Domain.Entities
{
    public class UserDivision
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int DivisionId { get; set; }
        public Division? Division { get; set; }
        public byte IsActive { get; set; }
    }
}
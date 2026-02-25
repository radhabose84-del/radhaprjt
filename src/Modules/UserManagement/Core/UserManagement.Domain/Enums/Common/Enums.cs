namespace UserManagement.Domain.Enums.Common
{
    public class Enums
    {
        public enum Status
        {
            Inactive = 0,
            Active  = 1
        }
        public enum IsDelete
        {
            NotDeleted = 0,
            Deleted = 1
        }
        public enum FirstTimeUserStatus
        {
            No = 0, 
            Yes = 1  
        }
    }
}
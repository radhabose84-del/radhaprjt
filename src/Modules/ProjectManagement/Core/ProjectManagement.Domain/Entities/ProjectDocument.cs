namespace ProjectManagement.Domain.Entities
{
    public class ProjectDocument 
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }       
        public int DocumentId { get; set; }      
        public string FileName { get; set; } = default!;
        public DateTimeOffset UploadedDate { get; set; }
        

        // Navigation
        public ProjectMaster Project { get; set; } = default!;
        
    }
}
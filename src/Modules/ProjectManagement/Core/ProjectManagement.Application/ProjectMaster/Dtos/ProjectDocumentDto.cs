namespace ProjectManagement.Application.ProjectMaster.Queries.Dtos
{
    public class ProjectDocumentDto
    {
        

        public int Id { get; set; }
        public int DocumentId { get; set; }
        public string FileName { get; set; } = default!;
       public string? UploadedPath { get; set; }
        public DateTimeOffset UploadedDate { get; set; }


    }
}
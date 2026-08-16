namespace AssignmentSystem.Api.DTOs.Subjects;

public class SubjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}

public class UpsertSubjectDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
}

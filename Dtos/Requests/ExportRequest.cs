namespace Basic_Report.Dtos.Requests;

public class ExportRequest
{
    public string Format { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public required List<DatasetRequest> Datasets { get; set; }

}

namespace Basic_Report.Endpoints;

using System.Text.Json;

using Basic_Report.Dtos.Requests;
using Basic_Report.Extensions;
using Basic_Report.Services;

using BoldReports.Web;
using BoldReports.Writer;

using Carter;
using Microsoft.AspNetCore.Mvc;

public class ReportEndpoint : ICarterModule {
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/upload", UploadReport).DisableAntiforgery();
        app.MapPost("/export/{filename}", Export);
    }
    private static async Task<IResult> UploadReport([FromForm]IFormFile file,
    IFileProcessor fileProcessor,
    CancellationToken cancellationToken) 
    {
        var obj = await fileProcessor.SaveFileAsync(file, cancellationToken);
        return obj is null
            ? Response.Send("Unable to process Request", StatusCodes.Status422UnprocessableEntity)
            : Response.Send(obj, StatusCodes.Status201Created);
    }

    private static async Task<IResult> Export(string filename, [FromBody]ExportRequest request, IWebHostEnvironment webHost)
    {
        if (string.IsNullOrEmpty(request.FileName))
        {
            request.FileName += "report";
        }
        FileStream inputStream = new(webHost.WebRootPath + @$"\reports\{filename}", FileMode.Open, FileAccess.Read);
        MemoryStream reportStream = new();
        await inputStream.CopyToAsync(reportStream);
        reportStream.Position = 0;
        inputStream.Close();
        // Initialize the Report Writer instance
        ReportWriter writer = new()
        {
            ReportProcessingMode = ProcessingMode.Local
        };
        //Pass the dataset collection for report

        writer.DataSources.Clear();
        foreach (var set in request.Datasets)
        {
            if (set.Value is null)
            {
                continue;
            }
            var deserializeObj = JsonSerializer.Deserialize<object>(set.Value.ToString()!);
            var serializedObj = JsonSerializer.Serialize(deserializeObj);
            JsonDocument objectDocument = JsonDocument.Parse(serializedObj);
            if (DynamicDictionaryExtension.IsJsonArray(serializedObj))
            {
                // Convert list of objects to list of dictionaries
                var dictionaries = DynamicDictionaryExtension.ConvertToListOfDictionaries(objectDocument.RootElement);
                writer.DataSources.Add(new ReportDataSource { Name = set.Name, Value = dictionaries });
            }
            else
            {
                // Convert single object to dictionary
                var dictionary = DynamicDictionaryExtension.ConvertToDictionary(objectDocument.RootElement);
                writer.DataSources.Add(new ReportDataSource { Name = set.Name, Value = dictionary });
            }
        }


        WriterFormat writerFormat;
        string? mimeType;
        if (request.Format.Equals("pdf", StringComparison.CurrentCultureIgnoreCase))
        {
            request.FileName += ".pdf";
            mimeType = "application/pdf";
            writerFormat = WriterFormat.PDF;
        }
        else if (request.Format.Equals("html", StringComparison.CurrentCultureIgnoreCase))
        {
            request.FileName += ".html";
            mimeType = "text/html";
            writerFormat = WriterFormat.HTML;
        }
        else if (request.Format.Equals("word", StringComparison.CurrentCultureIgnoreCase))
        {
            request.FileName += ".docx";
            mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            writerFormat = WriterFormat.Word;
        }
        else if (request.Format.Equals("csv", StringComparison.CurrentCultureIgnoreCase))
        {
            request.FileName += ".csv";
            mimeType = "text/csv";
            writerFormat = WriterFormat.CSV;
        }
        else if (request.Format.Equals("excel", StringComparison.CurrentCultureIgnoreCase))
        {
            request.FileName += ".xlsx";
            mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            writerFormat = WriterFormat.Excel;
        }
        else
        {
            request.FileName += ".html";
            mimeType = "text/html";
            writerFormat = WriterFormat.HTML;
        }

        writer.LoadReport(reportStream);

        MemoryStream memoryStream = new();
        writer.Save(memoryStream, writerFormat);

        // Download the generated export document to the client side.
        memoryStream.Position = 0;
        return Results.File(memoryStream, mimeType, request.FileName);
    }
}
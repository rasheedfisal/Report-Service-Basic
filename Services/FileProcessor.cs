namespace Basic_Report.Services;

public class FileProcessor : IFileProcessor
{
    private const string WWWroot = "wwwroot";
    private const string DirReportName = "reports";
    private const string FileExtensionName = ".rdlc";
    private readonly IWebHostEnvironment _hostEnvironment;
    public FileProcessor(IWebHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }
    public async Task<bool> RemoveFileAsync(string fileKey)
    {
        if (!fileKey.Contains(FileExtensionName))
        {
            fileKey += FileExtensionName;
        }
        string fileToDeletePath = Path.Combine(_hostEnvironment.ContentRootPath, WWWroot, DirReportName, fileKey);

        if (File.Exists(fileToDeletePath))
        {
            await Task.Run(() => File.Delete(fileToDeletePath));
            return true;
        }
        return false;
    }

    public async Task<FileObj?> SaveFileAsync(IFormFile? file, CancellationToken cancellationToken = default)
    {
       
        if (file is null)
        {
            return null;
        }
        
        if (file.Length == 0)
        {
            return null;
        }

        var folderName = Path.Combine(WWWroot, DirReportName);
        var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

        var fileNameWithExtension = file.FileName;
        var fileExt = Path.GetExtension(fileNameWithExtension);

        if(fileExt != ".rdlc")
        {
            return null;
        } 

        var fileName = Path.GetFileNameWithoutExtension(fileNameWithExtension);
        var fullPath = Path.Combine(pathToSave, fileNameWithExtension);
        var fileSize = file.Length;

        using FileStream? outputStream = new (fullPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 4096, true);
        await file.CopyToAsync(outputStream, cancellationToken);

        return new FileObj
        {
            FileName = fileName,
            FileExtension = fileExt,
            FileSize = fileSize,
        };
    }
}

public class FileObj {
    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public long FileSize { get; set; }
}

public interface IFileProcessor
{
    Task<FileObj?> SaveFileAsync(IFormFile? file, CancellationToken cancellationToken);
    Task<bool> RemoveFileAsync(string fileKey);
}

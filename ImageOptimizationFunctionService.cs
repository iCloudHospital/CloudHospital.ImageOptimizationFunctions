using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace CloudHospital.ImageOptimizationFunctions;

public class ImageOptimizationFunctionService
{
    private readonly ImageOptimizationService _imageOptimizationService;
    private readonly ILogger _logger;

    public ImageOptimizationFunctionService(
        ImageOptimizationService imageOptimizationService,
        ILoggerFactory loggerFactory)
    {
        _imageOptimizationService = imageOptimizationService;
        _logger = loggerFactory.CreateLogger<ImageOptimizationFunctionService>();
    }

    public async Task Run(byte[] triggerItem,
        string containerName,
        string fileName,
        string extension)
    {
        _logger.LogInformation($"Trigger item in {containerName} container");

        if (triggerItem == null || triggerItem.Length == 0)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(containerName))
        {
            throw new ArgumentNullException(nameof(containerName));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentNullException(nameof(fileName));
        }

        var step = 1;
        var metadataKey = "Optimized";
        var metadataValue = "True";
        // Image optimization width basis
        var width = 0;
        // Image optimization height basis
        var height = 0;

        var widthString = Environment.GetEnvironmentVariable("Width");
        var heightString = Environment.GetEnvironmentVariable("Height");

        if (!int.TryParse(widthString, out width))
        {
            width = 1240;
        }

        if (!int.TryParse(heightString, out height))
        {
            height = 940;
        }

        // BLOB Storage connection string
        var connectionString = Environment.GetEnvironmentVariable("BlobStorage");

        //_logger.LogInformation($"C# Blob trigger function Processed blob\n Name: {triggerItem} \n input: {input}");
        //_logger.LogInformation($"Trigger item: {triggerItem.Length}");
        //_logger.LogInformation($"Name: {fileName}");
        //_logger.LogInformation($"Extension: {extension}");
        //_logger.LogInformation($"Connection string: {connectionString}");

        var originalFileSize = triggerItem.Length;
        var optimizedFileSize = 0L;

        var blobName = $"{fileName}.{extension}";
        var client = new BlobServiceClient(connectionString);

        var containerClient = client.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var properties = await blobClient.GetPropertiesAsync();

        //var blobContentType = properties.Value.ContentType; // application/octet-stream

        //_logger.LogDebug($"{blobName} is {blobContentType}");

        //if (!blobContentType.StartsWith("image/"))
        //{
        //    _logger.LogInformation($"[OK] {blobName} is {blobContentType}. Do nothing");
        //    return;
        //}

        _logger.LogInformation($"[Step {step++}] Check metadata has {metadataKey} is {metadataValue}");
        var tag = string.Empty;
        if (properties.Value.Metadata.TryGetValue(metadataKey, out tag))
        {
            if (tag == metadataValue)
            {
                _logger.LogInformation($"[OK] {blobName} has {metadataKey}:{metadataValue} metadata. Do nothing");
                return;
            }
        }

        _logger.LogInformation($"[Step {step++}] Check extension");
        if (string.IsNullOrWhiteSpace(extension))
        {
            _logger.LogInformation("[OK] Could not recognize file extension. Do nothing");
            return;
        }

        var contentType = "unknown";
        try
        {
            contentType = _imageOptimizationService.GetContentType(extension);
        }
        catch
        {
            _logger.LogWarning($"[OK] It is unsupported file extension. (Current:{extension})");
            contentType = "unknown";
        }

        _logger.LogInformation($"[Step {step++}] Check content type");
        if (!contentType.StartsWith("image/"))
        {
            _logger.LogInformation($"[OK] {blobName} may not be an image file or may be an unsupported file. Do nothing");
            return;
        }

        _logger.LogInformation($"[Step {step++}] Do optimize");
        using (var inputStream = new MemoryStream(triggerItem))
        {
            inputStream.Position = 0;

            using (var outputStream = new MemoryStream())
            {
                try
                {
                    await _imageOptimizationService.OptimizeAsync(inputStream, outputStream, contentType, width, height);

                    // Delete current item when Optimization succeed
                    await blobClient.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots);

                    _logger.LogInformation($"[Step {step++}] Origin file has been deleted");

                    optimizedFileSize = outputStream.Length;
                    outputStream.Position = 0;

                    await containerClient.UploadBlobAsync(blobName, outputStream);
                    _logger.LogInformation($"[Step {step++}] Optimized file has been uploaded.");

                    var optimizedBlobClient = containerClient.GetBlobClient(blobName);
                    var exists = await optimizedBlobClient.ExistsAsync();

                    if (exists)
                    {
                        // Set optimized flag
                        var meataData = new Dictionary<string, string>
                        {
                            { metadataKey, metadataValue },
                        };

                        optimizedBlobClient.SetMetadata(meataData);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[FAIL] Could not optimize image file");
                }
                finally
                {
                    outputStream.Close();
                }
            }

            inputStream.Close();
        }

        _logger.LogInformation($"[Step {step++}] Diff:\nOrigin: {originalFileSize:n0} bytes\nOptimized: {optimizedFileSize:n0} bytes");
        _logger.LogInformation($"[OK] {blobName} has been optimized.");
    }
}
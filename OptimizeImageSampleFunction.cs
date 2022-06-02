using System;
using System.IO;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CloudHospital.ImageOptimizationFunctions
{
    public class OptimizeImageSampleFunction
    {
        /// <summary>
        /// 대상 컨테이너 이름
        /// </summary>
        private const string ContainerName = "sample";

        private readonly ImageOptimizationFunctionService _imageOptimizationFunctionService;
        private readonly ILogger _logger;

        public OptimizeImageSampleFunction(ImageOptimizationFunctionService imageOptimizationFunctionService, ILoggerFactory loggerFactory)
        {
            _imageOptimizationFunctionService = imageOptimizationFunctionService;
            _logger = loggerFactory.CreateLogger<OptimizeImageSampleFunction>();
        }

        [Function("OptimizeImageSampleFunction")]
        public Task Run(
            [BlobTrigger($"{ContainerName}/{{fileName}}.{{extension}}", Connection = "BlobStorage")] byte[] triggerItem,
            string fileName,
            string extension)
        {
            return _imageOptimizationFunctionService.Run(triggerItem, ContainerName, fileName, extension);
        }
    }
}

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using CloudHospital.ImageOptimizationFunctions;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddTransient<ImageOptimizationService>();
        services.AddTransient<ImageOptimizationFunctionService>();
    })
    .Build();


host.Run();
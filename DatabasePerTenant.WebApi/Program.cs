using DatabasePerTenant.WebApi.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DatabasePerTenant.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddAzureConfigAndKeyvault(hostingContext);

                if (hostingContext.HostingEnvironment.EnvironmentName == Environments.Development
                    || hostingContext.HostingEnvironment.EnvironmentName == "Test")
                {
                    config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json");
                }
            })
            .UseStartup<Startup>();
    }
}

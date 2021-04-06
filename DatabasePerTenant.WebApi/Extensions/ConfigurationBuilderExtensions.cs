using System;
using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace DatabasePerTenant.WebApi.Extensions
{
    public static class IWebHostBuilderExtensions
    {
        public static void AddAzureConfigAndKeyvault(this IConfigurationBuilder config, WebHostBuilderContext hostingContext)
        {
            var settings = config.Build();

            var credentials = new DefaultAzureCredential();

            config.AddAzureAppConfiguration(options =>
            {
                try
                {
                    options
                    .Connect(new Uri(settings["AppConfig:Endpoint"]), credentials)
                    .Select(KeyFilter.Any, LabelFilter.Null)
                    .Select(KeyFilter.Any, hostingContext.HostingEnvironment.EnvironmentName)
                    .ConfigureKeyVault(kv =>
                    {
                        kv.SetCredential(credentials);
                    });
                }
                catch (AuthenticationFailedException e)
                {
                    Console.WriteLine($"Authentication Failed. {e.Message}");
                }
            });
        }
    }
}

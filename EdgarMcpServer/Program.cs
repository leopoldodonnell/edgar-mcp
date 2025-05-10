using EdgarMcpServer.Services;
using EdgarMcpServer.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace EdgarMcpServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Create the host builder
            var builder = Host.CreateApplicationBuilder(args);

            // Configure basic console logging
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole(consoleLogOptions =>
            {
                consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Information;
            });

            // Configure services
            ConfigureServices(builder.Services);

            // Add MCP server with stdio transport
            builder.Services
                .AddMcpServer()
                .WithStdioServerTransport()
                .WithToolsFromAssembly();

            // Build and run the host
            var host = builder.Build();
            await host.RunAsync();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Add HTTP client
            services.AddHttpClient<EdgarService>();

            // Register the EdgarService as a singleton
            services.AddSingleton<EdgarService>(sp => {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient(nameof(EdgarService));
                var logger = sp.GetRequiredService<ILogger<EdgarService>>();
                return new EdgarService(httpClient, logger);
            });

            // Register the EdgarTools as a singleton
            services.AddSingleton<EdgarTools>();
        }
    }
}

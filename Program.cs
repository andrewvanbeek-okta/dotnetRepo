using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Filters;

namespace ExampleIdentityProvider
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddSerilog(new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .WriteTo.RollingFile("Logs/saml-{Date}.log")
                        .Filter.ByIncludingOnly(Matching.FromSource("ComponentSpace.Saml2"))
                        .CreateLogger());
                })
                .UseStartup<Startup>()
                .Build();
    }
}

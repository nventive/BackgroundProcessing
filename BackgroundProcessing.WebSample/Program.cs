using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Sample Code.")]
[assembly: SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Sample Code.")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Sample Code.")]

namespace BackgroundProcessing.WebSample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseApplicationInsights()
                .UseStartup<Startup>();
    }
}

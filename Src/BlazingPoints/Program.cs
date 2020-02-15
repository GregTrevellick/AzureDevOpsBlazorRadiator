using DevExpress.Blazor;
using Microsoft.AspNetCore.Blazor.Hosting;
using System.Threading.Tasks;

namespace BlazingPoints
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            
            builder.Services.AddDevExpressBlazor();
            
            builder.RootComponents.Add<App>("appy");

            await builder.Build().RunAsync();
        }
    }
}
      
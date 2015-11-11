using Microsoft.AspNet.Builder;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Web.Services;

namespace Web
{
    public class Startup
    {
        private static IConfigurationRoot _configurationRoot;

        public Startup(IApplicationEnvironment appEnv)
        {
            _configurationRoot = new ConfigurationBuilder()
                .SetBasePath(appEnv.ApplicationBasePath)
                .AddJsonFile("config.json")
                .Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSingleton(serviceProvider => _configurationRoot);

            services.AddTransient<AzureSearchService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }
    }
}

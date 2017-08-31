using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using shared.Services;
using srvkestrel;
using System;
using System.IO;
using System.Linq;
using websrv1;

namespace KestrelDemo
{
    public class Startup
    {
        LightweightMiddleware staticmiddleware;
        public Startup(IHostingEnvironment env,ILogger<Startup> logger)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();


            string file = Configuration["server:dbdatafile"];
            logger.LogInformation($"datafile: {file}");

            long started = DateTime.Now.Ticks;
            var data = FileDbLoader.Load(file);
            Console.WriteLine($"data loaded in {(DateTime.Now.Ticks - started) / 10000} ms");

            var db = new InmemoryDatabase(data);

            staticmiddleware = new LightweightMiddleware(new LightweightRouter(db));
        }


        public static IConfigurationRoot Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfigurationRoot>(Configuration);
            //services.AddSingleton<IDatabase>(db);
        }


       
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.Run(staticmiddleware.process);
        }
    }
}
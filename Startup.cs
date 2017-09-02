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


            string fileName = Configuration["server:dbdatafile"];
            logger.LogInformation($"datafile: {fileName}");

            long started = DateTime.Now.Ticks;
            var data = FileDbLoader.Load(fileName);
            Console.WriteLine($"data loaded in {(DateTime.Now.Ticks - started) / 10000} ms");

            if (data.currentTime == null)
            {
                try
                {
                    using (var file = File.OpenText(fileName.Replace("/data.zip", "/options.txt")))
                        data.currentTime = file.ReadLine().ParseInt();
                }
                catch
                {
                    Console.WriteLine("no timestamp found neither in data.zip nor out of zip");
                }
            }

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
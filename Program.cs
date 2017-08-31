using KestrelDemo;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace websrv1
{
    class Program
    {
        public static void Main(string[] args = null)
        {
            try
            {
                Console.WriteLine($"{DateTime.Now.ToString("hh:mm:ss")} START v2017-08-30 18:00 vKest BigData");

                var host = WebHost.CreateDefaultBuilder(args)

                    .ConfigureLogging((hostingContext, logging) =>
                    {
                        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                        logging.AddConsole();
                        //logging.AddDebug();
                    })
                    .UseStartup<Startup>()
                    .UseKestrel(options =>
                    {
                        options.AllowSynchronousIO = true;
                        //options.ApplicationSchedulingMode = SchedulingMode.ThreadPool; =default
                    })
                    /*
                    .UseLibuv(opts =>
                        {
                            opts.ThreadCount = 2;
                        }
                    )
                    */
                    //.UseUrls("http://127.0.0.1:80")
                    .Build();

                Console.WriteLine($"{DateTime.Now.ToString("hh:mm:ss")} Begin processing");
                Task.Run(async () => { await tester(); });
                host.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now.ToString("hh:mm:ss")}FATAL: " + e.Message);
            }
        }

        private static async Task tester()
        {
            await warmup();
            long lastcount=0;
            while (true)
            {
                int sec = 10;
                await Task.Delay(1000*sec);
                long max = Interlocked.Exchange(ref Stats.maxval,0);
                if (Stats.totalCnt > 0)
                {
                    Console.WriteLine($" {Stats.totalTicksBefWrite / 10000} / {Stats.totalTicksALL / 10000} ms for {Stats.totalCnt} req, glob-avg={Stats.totalTicksALL / 10 / Stats.totalCnt}, loc-max={max / 10} mks/req @ {(Stats.totalCnt - lastcount) / sec} rps ");
                    //Console.WriteLine($"ports: cnt={Startup.portStat.Count}, single={Startup.portStat.Where(s=>s.Value.requests==1).Count()} non-single-avg={Startup.portStat.Where(s => s.Value.requests > 1).Select(s=>(int?)s.Value.requests).Average()} ");
                    //Startup.portStat.Clear();
                }
                lastcount = Stats.totalCnt;
                
            }

        }

        private static async Task warmup()
        {
            await Task.Delay(3000);
            try
            {
                DateTime started = DateTime.Now;
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("http://localhost:80/users/1");
                HttpWebResponse resp = (HttpWebResponse)(await req.GetResponseAsync());
                resp.Close();
                Console.WriteLine($"warmup test done:\t{(DateTime.Now.Ticks - started.Ticks) / 10000} ms");
            }
            catch (Exception e)
            {
                Console.WriteLine("warmup error:" + e.Message);
            }
            Stats.totalCnt = 0;
            //Stats.totalTicksAftRead = 0;
            Stats.totalTicksALL = 0;
            Stats.totalTicksBefWrite = 0;
        }
    }
}

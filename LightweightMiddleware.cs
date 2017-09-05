using Microsoft.AspNetCore.Http;
using shared;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using websrv1;

namespace srvkestrel
{
    class LightweightMiddleware
    {
        LightweightRouter router;

        public LightweightMiddleware(LightweightRouter router)
        {
            this.router = router;
        }


        private byte[] ReadExact(Stream inputStream, int len)
        {
            byte[] res = new byte[len];
            int done = 0;
            while (len > done)
                done += inputStream.Read(res, done, len - done);
            return res;
        }


        static void disableGC()
        {
            try
            {
                bool ok = GC.TryStartNoGCRegion(256000000);
                Console.WriteLine("NoGCRegion:" + ok.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("NoGCRegion: err " + e.Message);
            }
        }

        static volatile string LAST_METHOD = "GET";
        Timer gctimer = new Timer(callGC, null, 5000, 2000);

        static DateTime? LASTPOSTTIME = null;
        static void callGC(object obj)
        {
            if (LASTPOSTTIME.HasValue && (DateTime.Now - LASTPOSTTIME.Value).TotalSeconds > 2.5)
            {
                LASTPOSTTIME = null;
                Stopwatch sw = Stopwatch.StartNew();
                Console.Write("GC:");
                GC.Collect();
                Console.WriteLine(sw.ElapsedMilliseconds + " ms");

            }
        }


        private Resp processInternal(HttpRequest requ)
        {
            try
            {
                if (requ.Method == "POST") LASTPOSTTIME = DateTime.Now;

                if (LAST_METHOD == "POST" && requ.Method == "GET")
                {
                    LAST_METHOD = requ.Method;
                    Task.Run(() => { disableGC(); });
                }
                LAST_METHOD = requ.Method;


                switch (requ.Method)
                {
                    case "GET":
                        return router.processGetRequest(requ.Path, requ.Query);
                    case "POST":
                        var len = requ.ContentLength;
                        if (len > 0)
                        {
                            byte[] data = ReadExact(requ.Body, checked((int)len));
                            return router.processPostRequest(requ.Path, Encoding.UTF8.GetString(data));
                        }
                        else
                            return new Resp { code = 400 };
                    default:
                        return new Resp { code = 404 };
                }
            }
            catch (HttpError e)
            {
                return new Resp { code = e.code };
            }
            catch (Exception)
            {
                return new Resp { code = 500 };
            }
        }

        //public class PortStat {public int requests = 0; };
        //public static ConcurrentDictionary<int, PortStat> portStat = new ConcurrentDictionary<int, PortStat>();
        static readonly byte[] empty = new byte[0];

        public async Task process(HttpContext client)
        {
#if DEBUG
            long started = DateTime.Now.Ticks;


            /*
                        int port = client.Connection.RemotePort;
                        PortStat stat;

                        if (portStat.TryGetValue(port, out stat)) {
                            Interlocked.Increment(ref stat.requests);
                        }
                        else
                            if (!portStat.TryAdd(port, new PortStat { requests = 1 }))
                                Interlocked.Increment(ref portStat[port].requests);
            */

#endif
            Resp resp = processInternal(client.Request);
            //object must be either null,string or byte[]


            client.Response.StatusCode = resp.code;
            client.Response.Headers["Connection"] = "keep-alive";
            //client.Response.Headers["Keep-Alive"] = "timeout=3000";

#if DEBUG
            Interlocked.Add(ref Stats.totalTicksBefWrite, DateTime.Now.Ticks - started);
#endif
            if (resp.code == 200)
            {
                if (resp.bodyStr != null) {
                    int len = Encoding.UTF8.GetByteCount(resp.bodyStr);
                    client.Response.ContentLength = len;

                    using(var wr =new StreamWriter(client.Response.Body)) wr.Write(resp.bodyStr);
                }
                else {
                    byte[] respdata = resp.body ?? empty;
                    client.Response.ContentLength = respdata.Length;
                    client.Response.ContentType = "application/json; charset=utf-8";
                    client.Response.Body.Write(respdata, 0, respdata.Length);
                }
            }
            else
            {
                client.Response.ContentLength = 0;
                //await client.Response.Body.WriteAsync(empty, 0, 0).ConfigureAwait(false);
            }

#if DEBUG            
            long ticks = DateTime.Now.Ticks - started;
            Interlocked.Add(ref Stats.totalTicksALL, ticks);
            Interlocked.Increment(ref Stats.totalCnt);

            Interlocked.Exchange(ref Stats.maxval, Math.Max(ticks,Stats.maxval));
#endif            
        }
    }
}

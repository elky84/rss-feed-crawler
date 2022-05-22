using AutoMapper;
using EzAspDotNet.Exception;
using EzAspDotNet.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using Server.Services;
using System;
using System.Text;

namespace Server
{

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .WriteTo.Console()
                .CreateLogger();

            Configuration = configuration;

            using (var log = new LoggerConfiguration().WriteTo.Console().CreateLogger())
            {
                log.Information($"Local TimeZone:{TimeZoneInfo.Local}");
            }
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            EzAspDotNet.Models.MapperUtil.Initialize(
                new MapperConfiguration(cfg =>
                {
                    cfg.AllowNullDestinationValues = true;

                    cfg.CreateMap<EzAspDotNet.Notification.Models.Notification, Protocols.Common.Notification>(MemberList.None);
                    cfg.CreateMap<Protocols.Common.Notification, EzAspDotNet.Notification.Models.Notification>(MemberList.None);

                    cfg.CreateMap<FeedCrawler.Models.Rss, Protocols.Common.Rss>(MemberList.None)
                        .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

                    cfg.CreateMap<Protocols.Common.Rss, FeedCrawler.Models.Rss>(MemberList.None)
                        .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

                    cfg.CreateMap<FeedCrawler.Models.FeedData, Protocols.Common.FeedData>(MemberList.None);
                    cfg.CreateMap<Protocols.Common.FeedData, FeedCrawler.Models.FeedData>(MemberList.None);
                })
            );

            services.AddHttpClient();

            services.AddControllers().AddNewtonsoftJson();

            services.AddCors(options => options.AddPolicy("AllowSpecificOrigin",
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            });

            services.ConfigureSwaggerGen(options =>
            {
                options.CustomSchemaIds(x => x.FullName);
            });

            services.AddTransient<HttpClientService>();

            services.AddTransient<MongoDbService>();

            services.AddSingleton<IHostedService, WebHookLoopingService>();
            services.AddSingleton<WebHookService>();

            services.AddSingleton<IHostedService, CrawlingLoopingService>();
            services.AddSingleton<CrawlingService>();

            services.AddSingleton<NotificationService>();

            services.AddSingleton<RssService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseHttpsRedirection();
            }

            app.UseCors("AllowSpecificOrigin");

            app.ConfigureExceptionHandler();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger(c =>
            {
                c.SerializeAsV2 = true;
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "My API V1");
            });
        }
    }
}

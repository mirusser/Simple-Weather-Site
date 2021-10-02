using System.Reflection;
using EmailService.Listeners;
using EmailService.Middlewares;
using EmailService.Services;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Models.Settings;

namespace EmailService
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ExceptionHandler>();
            services.Configure<MailSettings>(Configuration.GetSection(nameof(MailSettings)));

            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddMassTransit(config =>
            {
                RabbitMQSettings rabbitMQSettings = new();
                Configuration.GetSection(nameof(RabbitMQSettings)).Bind(rabbitMQSettings);

                config.AddConsumer<SendEmailListener>();
                config.SetKebabCaseEndpointNameFormatter();

                config.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(rabbitMQSettings.Host);
                    cfg.ConfigureEndpoints(ctx);
                });
            });
            services.AddMassTransitHostedService(waitUntilStarted: true);

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "EmailService", Version = "v1" });
            });

            services.AddTransient<IMailService, MailService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ExceptionHandler>();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EmailService v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/ping", ctx => ctx.Response.WriteAsync("pong"));
                endpoints.MapGet("", ctx => ctx.Response.WriteAsync($"EmailService in {env.EnvironmentName} mode"));
            });
        }
    }
}
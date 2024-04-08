using Common.Presentation;
using EmailService.Application;
using EmailService.Domain.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Host.UseSerilog();
    builder.Services.AddCommonPresentationLayer(builder.Configuration);
    builder.Services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));

    builder.Services.AddApplication(builder.Configuration);

    builder.Services.AddControllers();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "EmailService", Version = "v1" });
    });
}

var app = builder.Build();
{
    if (builder.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseServiceExceptionHandler();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EmailService v1"));

    //app.UseHttpsRedirection();

    app.UseRouting();

    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapGet("/ping", ctx => ctx.Response.WriteAsync("pong"));
        endpoints.MapGet("", ctx => ctx.Response.WriteAsync($"EmailService in {builder.Environment.EnvironmentName} mode"));
    });

    WebApplicationStartup.Run(app);
}
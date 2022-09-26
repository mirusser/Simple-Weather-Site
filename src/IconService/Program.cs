using Common.Presentation;
using Common.Presentation.Exceptions;
using Common.Presentation.Exceptions.Handlers;
using IconService;
using IconService.Application;
using IconService.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
{
    builder.Host.UseSerilog();

    builder.Services
        .AddPresentation()
        .AddApplication()
        .AddInfrastructure(builder.Configuration);
}

var app = builder.Build();
{
    if (builder.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseServiceExceptionHandler();

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "IconService v1"));

    //app.UseHttpsRedirection();

    app.UseRouting();

    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });

    WebApplicationStartup.Run(app);
}
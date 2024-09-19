using Common.Application.HealthChecks;
using Common.Presentation;
using Common.Presentation.Http;
using Common.Presentation.Settings;
using GrpcCitiesClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WeatherSite.Clients;
using WeatherSite.Logic.Clients;
using WeatherSite.Settings;

var builder = WebApplication.CreateBuilder(args);
{
	builder.Host.UseSerilog();
	builder.Services.AddCommonPresentationLayer(builder.Configuration);

	builder.Services.Configure<ApiEndpoints>(builder.Configuration.GetSection(nameof(ApiEndpoints)));
	builder.Services.Configure<ApiConsumerAuthSettings>(builder.Configuration.GetSection(nameof(ApiConsumerAuthSettings)));

	builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
	builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

	builder.Services.AddTransient<BearerTokenHandler>();

	builder.Services.AddHttpClient<WeatherForecastClient>();
	builder.Services.AddHttpClient<WeatherHistoryClient>();
	builder.Services.AddHttpClient<IconClient>();

	builder.Services.AddHttpClient<CityClient>()
		.AddHttpMessageHandler<BearerTokenHandler>();

	builder.Services.AddGrpcCitiesClient(builder.Configuration);
	builder.Services.AddCommonHealthChecks(builder.Configuration);
}

var app = builder.Build();
{
	if (builder.Environment.IsDevelopment())
	{
		app.UseDeveloperExceptionPage();
	}
	else
	{
		app.UseExceptionHandler("/Home/Error");
		// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
		app.UseHsts();
	}

	app.UseDefaultExceptionHandler();
	//app.UseHttpsRedirection();
	app.UseStaticFiles();

	app
	.UseRouting()
	.UseCommonHealthChecks();

	app.UseAuthorization();

	app.UseEndpoints(endpoints =>
	{
		endpoints.MapControllerRoute(
			name: "default",
			pattern: "{controller=WeatherPrediction}/{action=Index}/{id?}");
	});
}

await app.RunWithLoggerAsync();
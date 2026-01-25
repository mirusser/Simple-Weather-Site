using Common.Application.HealthChecks;
using Common.Infrastructure;
using Common.Presentation;
using Common.Presentation.Http;
using Common.Presentation.Settings;
using GrpcCitiesClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeatherSite.Logic.Clients;
using WeatherSite.Logic.Settings;

var builder = WebApplication.CreateBuilder(args);
{
	builder.Services.AddCommonInfrastructure(builder.Configuration);
	builder.AddCommonPresentationLayer();

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
	builder.Services.AddCommonHealthChecks();
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

	app.UseRouting();

	//app.UseAuthorization();

	app.MapControllerRoute(
		name: "default",
		pattern: "{controller=WeatherPrediction}/{action=Index}/{id?}");
	
	app.MapCommonHealthChecks();
}

await app.RunWithLoggerAsync();
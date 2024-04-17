using System.Text;
using JwtAuth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
{
	builder.Services.AddControllers();

	builder.Services.AddEndpointsApiExplorer();
	builder.Services.AddSwaggerGen();

	const string key = "veryStrongKey1234$$$$$$$$$$$$$$$^&%*^&%*%";

	builder.Services
		.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		})
		.AddJwtBearer(options =>
		{
			options.RequireHttpsMetadata = false;
			options.SaveToken = true;
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
				ValidateIssuer = false,
				ValidateAudience = false,
			};
		});

	builder.Services.AddSingleton(new AuthenticationManager(key));
}

var app = builder.Build();
{
	app.UseSwagger();
	app.UseSwaggerUI();

	app.UseHttpsRedirection();
	app.UseAuthentication();
	app.UseAuthorization();

	app.MapControllers();
}

app.Run();
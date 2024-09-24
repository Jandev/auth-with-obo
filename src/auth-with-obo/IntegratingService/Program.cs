using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
		.EnableTokenAcquisitionToCallDownstreamApi()
			.AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
			.AddDownstreamApi("BackendService", o => 
			{
				o.RequestAppToken = false;
				o.BaseUrl = builder.Configuration.GetValue<string>("BackendService:BaseUrl");
				o.Scopes = builder.Configuration.GetValue<string>("BackendService:Scopes")?.Split(",");
			})
			.AddInMemoryTokenCaches();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

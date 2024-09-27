using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Microsoft.Graph;

namespace BackendService.Controllers
{
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	public class WeatherForecastController : ControllerBase
	{
		private readonly GraphServiceClient _graphServiceClient;
		private static readonly string[] Summaries = new[]
		{
			"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
		};

		private readonly ILogger<WeatherForecastController> _logger;

		public WeatherForecastController(ILogger<WeatherForecastController> logger, GraphServiceClient graphServiceClient)
		{
			_logger = logger;
			_graphServiceClient = graphServiceClient;
		}

		///////////////////////////////
		// Authorization via Application Scopes
		///////////////////////////////

		[HttpGet("Default", Name = "GetWeatherForecast")]
		[RequiredScope("BackendDefault")]
		public async Task<IEnumerable<WeatherForecast>> Get()
		{
			string user = await GetUsername();
			return Enumerable.Range(1, 5).Select(index => new WeatherForecast
			{
				Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
				TemperatureC = Random.Shared.Next(-20, 55),
				Summary = user + " is " + Summaries[Random.Shared.Next(Summaries.Length)]
			})
			.ToArray();
		}

		[HttpGet("WithUserScope", Name = "GetWithUserScope")]
		[RequiredScope("WeatherUser")]
		public async Task<IEnumerable<WeatherForecast>> GetWeatherUser()
		{
			string user = await GetUsername();
			return Enumerable.Range(1, 5).Select(index => new WeatherForecast
			{
				Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
				TemperatureC = Random.Shared.Next(-20, 55),
				Summary = user + " is " + Summaries[Random.Shared.Next(Summaries.Length)]
			})
			.ToArray();
		}

		[HttpGet("WithAdminScope", Name = "GetWithAdminScope")]
		[RequiredScope("WeatherAdmin")]
		public async Task<IEnumerable<WeatherForecast>> GetWeatherAdmin()
		{
			string user = await GetUsername();
			return Enumerable.Range(1, 5).Select(index => new WeatherForecast
			{
				Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
				TemperatureC = Random.Shared.Next(-20, 55),
				Summary = user + " is " + Summaries[Random.Shared.Next(Summaries.Length)]
			})
			.ToArray();
		}

		[HttpGet("WithAccessAsUserScope", Name = "GetWithAccessAsUserScope")]
		[RequiredScope("access_as_user")]
		public async Task<IEnumerable<WeatherForecast>> GetWeatherAsUser()
		{
			string user = await GetUsername();
			return Enumerable.Range(1, 5).Select(index => new WeatherForecast
			{
				Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
				TemperatureC = Random.Shared.Next(-20, 55),
				Summary = user + " is " + Summaries[Random.Shared.Next(Summaries.Length)]
			})
			.ToArray();
		}

		[HttpGet("WithAccessAsUserScopeAndAdminRole", Name = "GetWithAccessAsUserScopeAndAdminRole")]
		[RequiredScope("access_as_user")]
		[Authorize(Roles = "Admin")]
		public async Task<IEnumerable<WeatherForecast>> GetWeatherAsUserScopeAndAdminRole()
		{
			string user = await GetUsername();
			return Enumerable.Range(1, 5).Select(index => new WeatherForecast
			{
				Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
				TemperatureC = Random.Shared.Next(-20, 55),
				Summary = user + " is " + Summaries[Random.Shared.Next(Summaries.Length)]
			})
			.ToArray();
		}

		///////////////////////////////
		// Authorization via Application Roles
		///////////////////////////////

		[HttpGet("WithAdminRole", Name = "GetWithAdminRole")]
		[Authorize(Roles = "Admin")]
		public async Task<IEnumerable<WeatherForecast>> GetAdminRole()
		{
			string user = await GetUsername();
			return Enumerable.Range(1, 5).Select(index => new WeatherForecast
			{
				Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
				TemperatureC = Random.Shared.Next(-20, 55),
				Summary = user + " is " + Summaries[Random.Shared.Next(Summaries.Length)]
			})
			.ToArray();
		}

		[HttpGet("WithUserRole", Name = "GetWithUserRole")]
		[Authorize(Roles = "User")]
		public async Task<IEnumerable<WeatherForecast>> GetUserRole()
		{
			string user = await GetUsername();
			return Enumerable.Range(1, 5).Select(index => new WeatherForecast
			{
				Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
				TemperatureC = Random.Shared.Next(-20, 55),
				Summary = user + " is " + Summaries[Random.Shared.Next(Summaries.Length)]
			})
			.ToArray();
		}

		private async Task<string> GetUsername()
		{
			try
			{
				var user = await _graphServiceClient.Me.Request().GetAsync();
				return user.DisplayName;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error fetching user from Graph API.");
				return "Unknown";
			}
			
		}
	}
}

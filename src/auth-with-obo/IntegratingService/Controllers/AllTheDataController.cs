using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Microsoft.Graph;
using System.Net.Http.Headers;
using Azure.Identity;
using Azure.Core;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Identity.Client;
using Microsoft.Identity.Abstractions;

namespace IntegratingService.Controllers
{
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
	public class AllTheDataController : ControllerBase
	{
		const string WeatherAdminScope = "WeatherAdmin";
		const string WeatherUserScope = "WeatherUser";

		private readonly GraphServiceClient _graphServiceClient;
		private readonly IHttpClientFactory clientFactory;
		private readonly IConfiguration configuration;

		private readonly ILogger<AllTheDataController> _logger;
		private readonly IDownstreamApi _downstreamApi;
		private readonly IAuthorizationHeaderProvider _authorizationHeaderProvider;

		public AllTheDataController(
			GraphServiceClient graphServiceClient,
			IHttpClientFactory clientFactory,
			IConfiguration configuration,
			ILogger<AllTheDataController> logger,
			IDownstreamApi downstreamApi,
			IAuthorizationHeaderProvider authorizationHeaderProvider
			)
		{
			_logger = logger;
			this._downstreamApi = downstreamApi;
			this._authorizationHeaderProvider = authorizationHeaderProvider;
			_graphServiceClient = graphServiceClient;
			this.clientFactory = clientFactory;
			this.configuration = configuration;
		}

		[HttpGet("WeatherForecast", Name = "GetWeatherForecast")]
		public async Task<ApiResponse> Get()
		{
			var username = await GetUserName();

			var accessToken = await GenerateAccessToken();
			var backendDetails = await GetBackendDetails(accessToken, "/WeatherForecast/Default/");

			var integrationServiceDetails = new IntegrationServiceCallDetails(
				HttpContext.Request.Headers.Authorization.First()!,
				username);

			return new ApiResponse(integrationServiceDetails, backendDetails);
		}

		[HttpGet("WeatherForecastWithUserRole", Name = "GetWeatherForecastWithUserRole")]
		public async Task<ApiResponse> GetWithUserRole()
		{
			var username = await GetUserName();

			var accessToken = await GenerateAccessToken();
			var backendDetails = await GetBackendDetails(accessToken, "/WeatherForecast/WithUserRole/");

			var integrationServiceDetails = new IntegrationServiceCallDetails(
				HttpContext.Request.Headers.Authorization.First()!,
				username);

			return new ApiResponse(integrationServiceDetails, backendDetails);
		}

		[HttpGet("WeatherForecastWithAdminRole", Name = "GetWeatherForecastWithAdminRole")]
		public async Task<ApiResponse> GetWithAdminRole()
		{
			var username = await GetUserName();

			var accessToken = await GenerateAccessToken();
			var backendDetails = await GetBackendDetails(accessToken, "/WeatherForecast/WithAdminRole/");

			var integrationServiceDetails = new IntegrationServiceCallDetails(
				HttpContext.Request.Headers.Authorization.First()!,
				username);

			return new ApiResponse(integrationServiceDetails, backendDetails);
		}

		[HttpGet("GetWeatherForecastWithAdminRoleAccessAsUserManagedIdentity", Name = "GetWeatherForecastWithAdminRoleAccessAsUserManagedIdentity")]
		public async Task<ApiResponse> GetWeatherForecastWithAdminRoleAccessAsUserManagedIdentity()
		{
			var username = await GetUserName();

			var accessToken = await GenerateAccessToken("access_as_user");
			accessToken = accessToken.Replace("Bearer ", "");
			var backendDetails = await GetBackendDetails(accessToken, "/WeatherForecast/WithAdminRole/");

			var integrationServiceDetails = new IntegrationServiceCallDetails(
				HttpContext.Request.Headers.Authorization.First()!,
				username);

			return new ApiResponse(integrationServiceDetails, backendDetails);
		}

		[HttpGet("GetWeatherForecastWithAdminRoleAccessAsUser", Name = "GetWeatherForecastWithAdminRoleAccessAsUser")]
		public async Task<ApiResponse> GetWeatherForecastWithAdminRoleAccessAsUser()
		{
			var username = await GetUserName();

			var accessToken = await this._authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(new[] { "api://8ebbea06-f01e-4f94-8254-32da2e94c240/access_as_user" });
			accessToken = accessToken.Replace("Bearer ", "");
			var backendDetails = await GetBackendDetails(accessToken, "/WeatherForecast/WithAccessAsUserScope/");

			var integrationServiceDetails = new IntegrationServiceCallDetails(
				HttpContext.Request.Headers.Authorization.First()!,
				username);

			return new ApiResponse(integrationServiceDetails, backendDetails);
		}

		[HttpGet("WeatherForecastWithWeatherUserScope", Name = "GetWeatherForecastWithWeatherUserScope")]
		public async Task<ApiResponse> GetWithUserScope()
		{
			var username = await GetUserName();

			//var accessToken = await GenerateAccessToken(WeatherUserScope);
			var accessToken = await this._authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(new[] { "api://8ebbea06-f01e-4f94-8254-32da2e94c240/" + WeatherUserScope, "api://8ebbea06-f01e-4f94-8254-32da2e94c240/user_impersonation" });
			accessToken = accessToken.Replace("Bearer ", "");
			var backendDetails = await GetBackendDetails(accessToken, "/WeatherForecast/WithUserScope/");
			//var backendDetails = await GetBackendDetailsWithScopes(WeatherUserScope, "/WeatherForecast/WithUserScope/");

			var integrationServiceDetails = new IntegrationServiceCallDetails(
				HttpContext.Request.Headers.Authorization.First()!,
				username);

			return new ApiResponse(integrationServiceDetails, backendDetails);
		}

		[HttpGet("WeatherForecastWithWeatherAdminScope", Name = "GetWeatherForecastWithWeatherAdminScope/")]
		public async Task<ApiResponse> GetWithAdminScope()
		{
			var username = await GetUserName();

			// var accessToken = await GenerateAccessToken(WeatherAdminScope);
			var accessToken = await this._authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(new []{ "api://8ebbea06-f01e-4f94-8254-32da2e94c240/" + WeatherAdminScope, "api://8ebbea06-f01e-4f94-8254-32da2e94c240/user_impersonation" });
			accessToken = accessToken.Replace("Bearer ", "");
			var backendDetails = await GetBackendDetails(accessToken, "/WeatherForecast/WithAdminScope/");
			//var backendDetails = await GetBackendDetailsWithScopes(WeatherAdminScope, "/WeatherForecast/WithAdminScope/");

			var integrationServiceDetails = new IntegrationServiceCallDetails(
				HttpContext.Request.Headers.Authorization.First()!,
				username);

			return new ApiResponse(integrationServiceDetails, backendDetails);
		}

		private async Task<string> GetUserName()
		{
			try
			{
				var user = await _graphServiceClient.Me.Request().GetAsync();

				_logger.LogInformation("User `{User}` is fetched from Graph API.", user.DisplayName);
				return user.DisplayName;
			}
			catch(Exception ex)
			{
				this._logger.LogError(ex, "Failed to get user details from Graph API.");
				throw;
			}
			
		}

		private async Task<BackendApiCallDetails> GetBackendDetails(string accessToken, string slug)
		{
			string backendApiBaseUrl = this.configuration["BackendService:BaseUrl"] ?? throw new InvalidOperationException("BackendService:BaseUrl is not configured.");
			this._logger.LogInformation("Invoking backend API at `{BackendApiBaseUrl}`.", backendApiBaseUrl);

			var httpClient = this.clientFactory.CreateClient();
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			this._logger.LogInformation("Access token `{AccessToken}` is set in the request header.", accessToken);

			var response = await httpClient.GetAsync(backendApiBaseUrl + slug);
			var body = await response.Content.ReadAsStringAsync();

			try
			{
				var callDetails = new BackendApiCallDetails(
								accessToken,
								body,
								(int)response.StatusCode,
								response.ReasonPhrase ?? "No reason provided");

				return callDetails;
			}
			catch (Exception ex)
			{
				this._logger.LogError(ex, "Failed to get response from backend API.");
				throw;
			}
		}

		private async Task<BackendApiCallDetails> GetBackendDetailsWithScopes(string scope, string slug)
		{
			string backendApiBaseUrl = this.configuration["BackendService:BaseUrl"] ?? throw new InvalidOperationException("BackendService:BaseUrl is not configured.");
			this._logger.LogInformation("Invoking backend API at `{BackendApiBaseUrl}`.", backendApiBaseUrl);

			try
			{
				var response = await this._downstreamApi.CallApiForUserAsync<BackendApiCallDetails>("BackendService", o =>
				{
					o.RelativePath = slug;
					o.HttpMethod = HttpMethod.Get.ToString();
					o.Scopes = new List<string>() { scope };
				});

				this._logger.LogInformation("Found response `{response}`", response);
				return response ?? new BackendApiCallDetails("n/a", "n/a", 500, "n/a");
			}
			catch(Exception ex)
			{
				this._logger.LogError(ex, "Failed to get response from backend API.");
				throw;
			}
		}

		private async Task<string> GenerateAccessToken()
		{
			return await GenerateAccessToken(string.Empty);
		}

		private async Task<string> GenerateAccessToken(string requestedScope)
		{
			string applicationIdUri = this.configuration["BackendService:ApplicationIdUri"] ?? throw new InvalidOperationException("BackendService:ApplicationIdUri is not configured.");
			var tenantId = this.configuration["AzureAd:TenantId"];

			string userAssignedClientId = this.configuration["AssignedManagedIdentity"];
			try
			{
				_logger.LogInformation("Found `{UserManagedIdentityClientId}` as user assigned managed identity in the configuration.", userAssignedClientId);
				bool isRunningLocal = string.IsNullOrEmpty(userAssignedClientId);

				string scopeToRequest = string.IsNullOrEmpty(requestedScope) ? applicationIdUri + "/.default" : applicationIdUri + requestedScope;
				if (isRunningLocal)
				{
					_logger.LogInformation("Running locally. Using DefaultAzureCredential to get access token.");
					var tokenCredential = new DefaultAzureCredential();
					var accessToken = await tokenCredential.GetTokenAsync(
						new TokenRequestContext(scopes: new string[] { scopeToRequest }) { }
					);

					return accessToken.Token;
				}
				else
				{
					_logger.LogInformation("Running in Azure. Using ManagedIdentityCredential with Managed Identity `{ManagedIdentityClientId}` to get access token.", userAssignedClientId);

					var tokenCredential = new ManagedIdentityCredential(userAssignedClientId);

					_logger.LogInformation("Requesting access token for scope `{Scope}`.", requestedScope);
					var accessToken = await tokenCredential.GetTokenAsync(
						new TokenRequestContext(scopes: new string[] { scopeToRequest }) { }
					);

					
					return accessToken.Token;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to get access token for ApplicationId `{ApplicationId}`, Scope `{Scope}`, TenantId `{TenantId}` for identity `{Identity}`.", applicationIdUri, requestedScope, tenantId, userAssignedClientId);
				throw;
			}
		}

		public record BackendApiCallDetails(string AccessToken, string Body, int StatusCode, string Reason);

		public record IntegrationServiceCallDetails(string AccessToken, string Username);

		public record ApiResponse(IntegrationServiceCallDetails integrationServiceDetails, BackendApiCallDetails BackendDetails);
	}
}

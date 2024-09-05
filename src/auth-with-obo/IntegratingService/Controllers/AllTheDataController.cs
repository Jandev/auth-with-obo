using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Microsoft.Graph;
using System.Net.Http.Headers;
using Azure.Identity;
using Azure.Core;

namespace IntegratingService.Controllers
{
	[Authorize]
	[ApiController]
	[Route("[controller]")]
	[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
	public class AllTheDataController : ControllerBase
	{
		private readonly GraphServiceClient _graphServiceClient;
		private readonly IHttpClientFactory clientFactory;
		private readonly IConfiguration configuration;
		
		private readonly ILogger<AllTheDataController> _logger;

		public AllTheDataController(
			GraphServiceClient graphServiceClient,
			IHttpClientFactory clientFactory,
			IConfiguration configuration,
			ILogger<AllTheDataController> logger
			)
		{
			_logger = logger;
			_graphServiceClient = graphServiceClient;
			this.clientFactory = clientFactory;
			this.configuration = configuration;
		}

		[HttpGet(Name = "GetWeatherForecast")]
		public async Task<ApiResponse> Get()
		{
			var username = await GetUserName();
			var backendDetails = await GetBackendDetails();

			var integrationServiceDetails = new IntegrationServiceCallDetails(
				HttpContext.Request.Headers.Authorization.First()!,
				username);

			return new ApiResponse(integrationServiceDetails, backendDetails);
		}

		private async Task<string> GetUserName()
		{
			var user = await _graphServiceClient.Me.Request().GetAsync();
			return user.DisplayName;
		}

		private async Task<BackendApiCallDetails> GetBackendDetails()
		{
			string backendApiBaseUrl = this.configuration["BackendService:BaseUrl"];

			var accessToken = await GenerateAccessToken();

			var httpClient = this.clientFactory.CreateClient();
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			var response = await httpClient.GetAsync(backendApiBaseUrl + "/weatherforecast/");
			var body = await response.Content.ReadAsStringAsync();

			var callDetails = new BackendApiCallDetails(
				accessToken,
				body,
				(int)response.StatusCode,
				response.ReasonPhrase);

			return callDetails;
		}

		private async Task<string> GenerateAccessToken()
		{
			string applicationIdUri = this.configuration["BackendService:ApplicationIdUri"];
			var tenantId = this.configuration["AzureAd:TenantId"];

			var tokenCredential = new DefaultAzureCredential();
			var accessToken = await tokenCredential.GetTokenAsync(
				new TokenRequestContext(scopes: new string[] { applicationIdUri + "/.default" }) { }
			);

			return accessToken.Token;
		}

		public record BackendApiCallDetails(string AccessToken, string Body, int StatusCode, string Reason);

		public record IntegrationServiceCallDetails(string AccessToken, string Username);

		public record ApiResponse(IntegrationServiceCallDetails integrationServiceDetails, BackendApiCallDetails BackendDetails);
	}
}

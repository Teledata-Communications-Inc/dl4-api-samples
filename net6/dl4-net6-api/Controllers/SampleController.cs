using System.Text.Json;
using dl4_net6_api.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace dl4_net6_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SampleController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        public SampleController(IHttpClientFactory clientFactory)
        {
            _httpClient = clientFactory.CreateClient(HttpClients.TciDecisionLenderClient);
        }

        /// <summary>
        /// Returns a list of Dealers.
        /// </summary>
        /// <returns></returns>
        [HttpGet("/dealers")]
        public async Task<IActionResult> Dealers()
        {
            var response = await _httpClient.GetAsync("dealers");
            var content = await response.Content.ReadAsStringAsync();
            return Ok(JsonSerializer.Deserialize<dynamic>(content));
        }

        /// <summary>
        /// Returns a list of Roles.
        /// </summary>
        /// <returns></returns>
        [HttpGet("/roles")]
        public async Task<IActionResult> Roles()
        {
            var response = await _httpClient.GetAsync("roles");
            var content = await response.Content.ReadAsStringAsync();
            return Ok(JsonSerializer.Deserialize<dynamic>(content));
        }
    }
}

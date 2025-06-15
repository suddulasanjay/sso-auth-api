using Microsoft.AspNetCore.Mvc;
using SSOAuthAPI.Interfaces;
using SSOAuthAPI.Models.Application;

namespace SSOAuthAPI.Controllers.Application
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet]
        [ProducesResponseType<ClientDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(string clientId)
        {
            var res = await _clientService.GetClientAsync(clientId);
            return Ok(res);
        }

        [HttpPost]
        [ProducesResponseType<ClientAppResponse>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] ClientDto client)
        {
            var clientDetails = await _clientService.RegisterClientAsync(client);
            return Ok(clientDetails);
        }

    }
}

using Microsoft.AspNetCore.Mvc;
using TheraHub.Application.Abstractions.Mediator;
using TheraHub.Application.Test;

namespace TheraHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ISender _sender;

    public HealthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    [HttpPost("test")]
    public async Task<IActionResult> Test([FromBody] TestRequest request)
    {
        var command = new TestCommand(request.Name);
        var result = await _sender.SendCommandAsync(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = result.Value });
    }
}

public record TestRequest(string Name);

using System.Threading.Tasks;
using BackgroundProcessing.Core;
using BackgroundProcessing.Core.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundProcessing.WebSample
{
    // See http://restalk-patterns.org/long-running-operation-polling.html
    [ApiController]
    public class SampleController : ControllerBase
    {
        [HttpPost("api/v1/commands")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> DispatchCommand(
            [FromServices] IBackgroundDispatcher dispatcher)
        {
            var command = new SampleCommand();
            await dispatcher.DispatchAsync(command);
            return AcceptedAtRoute(nameof(GetCommandStatus), new { id = command.Id });
        }

        [HttpGet("api/v1/commands/{id}", Name = nameof(GetCommandStatus))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status303SeeOther)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetCommandStatus(
            [FromRoute] string id,
            [FromServices] IBackgroundCommandEventRepository repository)
        {
            var latestEvent = await repository.GetLatestEventForCommandId(id);
            if (latestEvent is null)
            {
                return NotFound();
            }

            switch (latestEvent.Status)
            {
                case BackgroundCommandEventStatus.Dispatched:
                case BackgroundCommandEventStatus.Processing:
                    // Indicate the desired polling frequency, in seconds.
                    Response.Headers.Add("Retry-After", "10");
                    return NoContent();
                case BackgroundCommandEventStatus.Processed:
                    // Once executed, redirect to the output.
                    return this.SeeOther(Url.RouteUrl(nameof(GetCommandOutput), new { id }));
                default:
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("api/v1/commands/{id}/output", Name = nameof(GetCommandOutput))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> GetCommandOutput([FromRoute] string id)
        {
            // This part is up to you / your business logic.
            if (id is null)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}

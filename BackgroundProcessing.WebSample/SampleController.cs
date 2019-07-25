using System;
using System.Threading.Tasks;
using BackgroundProcessing.Core;
using BackgroundProcessing.Core.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundProcessing.WebSample
{
    // See http://restalk-patterns.org/long-running-operation-polling.html
    public class SampleController : ControllerBase
    {
        [HttpPost("api/v1/commands")]
        public async Task<IActionResult> DispatchCommand(
            [FromServices] IBackgroundDispatcher dispatcher)
        {
            var command = new SampleCommand();
            await dispatcher.DispatchAsync(command);
            return AcceptedAtAction(nameof(GetCommandStatus), new { id = command.Id });
        }

        [HttpGet("api/v1/commands/{id}")]
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
                case BackgroundCommandEventStatus.Dispatching:
                case BackgroundCommandEventStatus.Processing:
                    // Indicate the desired polling frequency, in seconds.
                    Response.Headers.Add("Retry-After", "10");
                    return Ok();
                case BackgroundCommandEventStatus.Processed:
                    // Once executed, redirect to the output.
                    return this.SeeOther(Url.Action(nameof(GetCommandOutput), new { id }));
                default:
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("api/v1/commands/{id}/output")]
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

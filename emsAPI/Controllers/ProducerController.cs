using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProducerController : ControllerBase
{
    private readonly IProducerService _s;

    public ProducerController(IProducerService s)
    {
        _s = s;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var producers = await _s.ReadAll();
        return Ok(producers);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOne(int id)
    {
        var producer = await _s.ReadOne(id);
        if (producer == null)
            return NotFound();

        return Ok(producer);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Producer producer)
    {
        var ok = await _s.Create(producer);
        if (!ok)
            return BadRequest("Could not create producer");

        return CreatedAtAction(nameof(GetOne), new { id = producer.Id }, producer);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody] Producer producer)
    {
        var ok = await _s.Update(id, producer);
        if (!ok)
            return BadRequest("Could not update producer");

        return Ok("Updated");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _s.Delete(id);
        if (!ok)
            return NotFound("Could not delete producer");

        return Ok("Deleted");
    }
}
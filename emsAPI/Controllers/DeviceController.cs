using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeviceController : ControllerBase
{
    private readonly IDeviceService _s;

    public DeviceController(IDeviceService s)
    {
        _s = s;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var devices = await _s.ReadAll();
        return Ok(devices);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOne(int id)
    {
        var device = await _s.ReadOne(id);
        if (device == null)
            return NotFound();

        return Ok(device);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Device device)
    {
        var ok = await _s.Create(device);
        if (!ok)
            return BadRequest("Could not create device");

        return CreatedAtAction(nameof(GetOne), new { id = device.Id }, device);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody] Device device)
    {
        var ok = await _s.Update(id, device);
        if (!ok)
            return BadRequest("Could not update device");

        return Ok("Updated");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _s.Delete(id);
        if (!ok)
            return NotFound("Could not delete device");

        return Ok("Deleted");
    }
}
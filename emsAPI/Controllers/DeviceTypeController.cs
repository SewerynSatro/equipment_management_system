using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeviceTypeController : ControllerBase
{
    private readonly IDeviceTypeService _s;

    public DeviceTypeController(IDeviceTypeService s)
    {
        _s = s;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var deviceTypes = await _s.ReadAll();
        return Ok(deviceTypes);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOne(int id)
    {
        var deviceType = await _s.ReadOne(id);
        if (deviceType == null)
            return NotFound();

        return Ok(deviceType);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] DeviceType deviceType)
    {
        var ok = await _s.Create(deviceType);
        if (!ok)
            return BadRequest("Device type already exists or name is invalid.");

        return CreatedAtAction(nameof(GetOne), new { id = deviceType.Id }, deviceType);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody] DeviceType deviceType)
    {
        var ok = await _s.Update(id, deviceType);
        if (!ok)
            return BadRequest("Could not update device type");

        return Ok("Updated");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _s.Delete(id);
        if (!ok)
            return NotFound("Could not delete device type");

        return Ok("Deleted");
    }
}
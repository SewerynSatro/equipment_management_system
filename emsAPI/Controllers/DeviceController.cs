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
        if (string.IsNullOrWhiteSpace(device.SerialNumber))
            return BadRequest("Serial number is required.");

        var ok = await _s.Create(device);
        if (!ok)
            return Conflict("Device with this serial number already exists.");

        return CreatedAtAction(nameof(GetOne), new { id = device.Id }, device);
    }


    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody] Device device)
    {
        if (string.IsNullOrWhiteSpace(device.SerialNumber))
            return BadRequest("Serial number is required.");

        var ok = await _s.Update(id, device);
        if (!ok)
            return Conflict("Device with this serial number already exists.");

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
    
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable()
    {
        var devices = await _s.ReadAvailable();
        return Ok(devices);
    }
    
    [HttpGet("producer/{producerId:int}")]
    public async Task<IActionResult> GetByProducer(int producerId)
    {
        var devices = await _s.ReadByProducer(producerId);
        return Ok(devices);
    }

    [HttpGet("type/{typeId:int}")]
    public async Task<IActionResult> GetByType(int typeId)
    {
        var devices = await _s.ReadByType(typeId);
        return Ok(devices);
    }
}
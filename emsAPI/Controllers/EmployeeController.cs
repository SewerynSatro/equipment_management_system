using DTOs;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _service;

    public EmployeeController(IEmployeeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var employees = await _service.ReadAll();
        return Ok(employees);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOne(int id)
    {
        var employee = await _service.ReadOne(id);
        if (employee == null)
            return NotFound();

        return Ok(employee);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] EmployeeCreateDto dto)
    {
        var employee = await _service.Create(dto);
        if (employee == null) return BadRequest("Could not create employee");
        return CreatedAtAction(nameof(GetOne), new { id = dto }, employee);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] EmployeeUpdateDto dto)
    {
        var ok = await _service.Update(id, dto);
        if (!ok) return BadRequest("Could not update employee");
        return Ok("Updated");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.Delete(id);
        if (!ok)
            return NotFound("Could not delete employee");

        return Ok("Deleted");
    }
}
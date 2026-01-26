using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class BranchController : ControllerBase
{
    private readonly IBranchService _service;

    public BranchController(IBranchService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var branches = await _service.ReadAll();
        return Ok(branches);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOne(int id)
    {
        var branch = await _service.ReadOne(id);
        if (branch == null) 
            return NotFound();

        return Ok(branch);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Branch branch)
    {
        var ok = await _service.Create(branch);
        if (!ok) 
            return BadRequest("Could not create branch");

        return CreatedAtAction(nameof(GetOne), new { id = branch.Id }, branch);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] Branch branch)
    {
        var ok = await _service.Update(id, branch);
        if (!ok) 
            return BadRequest("Could not update branch");

        return Ok("Updated");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.Delete(id);
        if (!ok) 
            return NotFound("Could not delete branch");

        return Ok("Deleted");
    }
}
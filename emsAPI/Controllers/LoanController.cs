using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoanController : ControllerBase
{
    private readonly ILoanService _service;

    public LoanController(ILoanService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var loans = await _service.ReadAll();
        return Ok(loans);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOne(int id)
    {
        var loan = await _service.ReadOne(id);
        if (loan == null)
            return NotFound();

        return Ok(loan);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Loan loan)
    {
        var ok = await _service.Create(loan);
        if (!ok)
            return BadRequest("Could not create loan");

        return CreatedAtAction(nameof(GetOne), new { id = loan.Id }, loan);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] Loan loan)
    {
        var ok = await _service.Update(id, loan);
        if (!ok)
            return BadRequest("Could not update loan");

        return Ok("Updated");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.Delete(id);
        if (!ok)
            return NotFound("Could not delete loan");

        return Ok("Deleted");
    }
}
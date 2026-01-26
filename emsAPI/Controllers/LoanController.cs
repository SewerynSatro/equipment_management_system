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
    
    [HttpGet("user/{userId}/active")]
    public async Task<IActionResult> GetUserActiveLoans(int userId)
    {
        var loans = await _service.ShowUserActiveLoans(userId);
        if (loans == null)
            return NotFound($"User with id {userId} does not exist");

        return Ok(loans);
    }

    [HttpGet("user/{userId}/history")]
    public async Task<IActionResult> GetUserLoanHistory(int userId)
    {
        var loans = await _service.ShowUserHistory(userId);
        if (loans == null)
            return NotFound($"User with id {userId} does not exist");

        return Ok(loans);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveLoans()
    {
        var loans = await _service.ShowActiveLoans();
        return Ok(loans);
    }
    
    [HttpPost("{id}/return")]
    public async Task<IActionResult> Return(int id)
    {
        var ok = await _service.Return(id);
        if (!ok)
            return BadRequest("Could not return loan");

        return Ok("Returned");
    }
}

using Microsoft.AspNetCore.Mvc;
using ToDoApi.Models;
using ToDoApi.Services;

namespace ToDoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly ITodoService _svc;
    public TodosController(ITodoService svc) => _svc = svc;

    [HttpGet]
    public ActionResult<IEnumerable<TodoItem>> Get() => Ok(_svc.GetAll());

    public record AddTodoDto(string Title, int Priority = 2);

    [HttpPost]
    public ActionResult<TodoItem> Post([FromBody] AddTodoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title)) return BadRequest("Title required");
        var pr = Math.Clamp(dto.Priority, 1, 3);
        var added = _svc.Add(dto.Title.Trim(), pr); 
        return CreatedAtAction(nameof(Get), new { id = added.Id }, added);
    }

    [HttpPost("{id:int}/toggle")]
    public IActionResult Toggle(int id)
    {
        var ok = _svc.Toggle(id);
        return ok ? NoContent() : NotFound();
    }
}

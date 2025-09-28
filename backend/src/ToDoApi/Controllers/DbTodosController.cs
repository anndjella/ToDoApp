
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoApi.Data;
using ToDoApi.Models;

namespace ToDoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DbTodosController : ControllerBase
{
    private readonly AppDbContext _db;
    public DbTodosController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> Get()
    {
        var items = await _db.Todos.AsNoTracking()
            .OrderBy(t => t.Done)           
            .ThenByDescending(t => t.Priority)
            .ThenBy(t => t.Id)
            .ToListAsync();
        return Ok(items);
    }

    public record AddTodoDto(string Title, int Priority = 2);

    [HttpPost]
    public async Task<ActionResult<TodoItem>> Post([FromBody] AddTodoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title)) return BadRequest("Title required");
        var pr = Math.Clamp(dto.Priority, 1, 3);
        var item = new TodoItem { Title = dto.Title.Trim(), Done = false, Priority = (Priority)pr };
        _db.Todos.Add(item);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
    }

    [HttpPost("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var item = await _db.Todos.FirstOrDefaultAsync(t => t.Id == id);
        if (item is null) return NotFound();
        item.Done = !item.Done;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

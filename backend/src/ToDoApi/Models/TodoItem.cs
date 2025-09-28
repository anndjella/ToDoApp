
namespace ToDoApi.Models;

public enum Priority { Low = 3, Medium = 2, High = 1 }
public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool Done { get; set; }

    public Priority Priority { get; set; } = Priority.Medium;
}

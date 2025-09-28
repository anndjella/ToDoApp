
using ToDoApi.Models;

namespace ToDoApi.Services;

public interface ITodoService
{
    IReadOnlyList<TodoItem> GetAll();
    TodoItem Add(string title, int priority = 2);
    bool Toggle(int id);
}

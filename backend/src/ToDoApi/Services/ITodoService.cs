
using ToDoApi.Models;

namespace ToDoApi.Services;

public interface ITodoService
{
    IReadOnlyList<TodoItem> GetAll();
    TodoItem Add(string title);
    bool Toggle(int id);
}

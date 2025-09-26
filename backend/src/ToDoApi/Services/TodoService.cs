
using ToDoApi.Models;

namespace ToDoApi.Services;

public class TodoService : ITodoService
{
    private readonly List<TodoItem> _items = new();
    private int _id = 1;

    public IReadOnlyList<TodoItem> GetAll() => _items;

    public TodoItem Add(string title)
    {
        var item = new TodoItem { Id = _id++, Title = title, Done = false };
        _items.Add(item);
        return item;
    }

    public bool Toggle(int id)
    {
        var i = _items.FindIndex(x => x.Id == id);
        if (i < 0) return false;
        _items[i].Done = !_items[i].Done;
        return true;
    }
}

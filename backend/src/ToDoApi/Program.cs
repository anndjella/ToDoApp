
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ToDoApi.Data;
using ToDoApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<ITodoService, TodoService>();

var cs = builder.Configuration.GetConnectionString("SqlServer")
         ?? "Server=localhost,11433;Database=ToDoDb;User Id=sa;Password=Str0ngPassword!;TrustServerCertificate=True";

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(cs, sql => sql.EnableRetryOnFailure()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapControllers();

app.Run();

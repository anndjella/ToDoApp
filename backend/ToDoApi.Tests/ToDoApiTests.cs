using FluentAssertions;
using ToDoApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToDoApi.Data;
using System.Net.Http.Json;
using System.Net;

namespace ToDoApi.Tests
{
    public class ToDoApiTests { 

        private static WebApplicationFactory<Program> Factory()
        => new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b =>
            {
                b.UseEnvironment("Testing");
                b.ConfigureServices(services =>
                {
                    services.RemoveAll<DbContextOptions<AppDbContext>>();
                    services.RemoveAll<AppDbContext>();

                    services.AddDbContext<AppDbContext>(o =>
                        o.UseInMemoryDatabase($"testdb-{Guid.NewGuid()}"));
                });
            });

        [Fact]
        public async Task Create_Then_Toggle_Then_Get_Works()
        {
            using var factory = Factory();
            var client = factory.CreateClient();

            var created = await client.PostAsJsonAsync("/api/todos", new { title = "X", priority = 1 });
            created.StatusCode.Should().Be(HttpStatusCode.Created);

            var item = await created.Content.ReadFromJsonAsync<TodoItem>();
            item!.Priority.Should().Be(Priority.High);
            item.Done.Should().BeFalse();

            var toggle = await client.PostAsync($"/api/todos/{item.Id}/toggle", null);
            toggle.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var list = await client.GetFromJsonAsync<TodoItem[]>("/api/todos");
            list!.Single(i => i.Id == item.Id).Done.Should().BeTrue();
        }

        [Fact]
        public async Task Create_BadRequest_When_TitleMissing()
        {
            using var factory = Factory();
            var client = factory.CreateClient();

            var res = await client.PostAsJsonAsync("/api/todos", new { title = "   ", priority = 2 });
            res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_ReturnsOk_And_Reflects_State()
        {
            using var factory = Factory();
            var client = factory.CreateClient();

            await client.PostAsJsonAsync("/api/todos", new { title = "A", priority = 2 });
            await client.PostAsJsonAsync("/api/todos", new { title = "B", priority = 3 });

            var res = await client.GetAsync("/api/todos");
            res.StatusCode.Should().Be(HttpStatusCode.OK);

            var items = await res.Content.ReadFromJsonAsync<TodoItem[]>();
            items!.Select(i => i.Title).Should().BeEquivalentTo(new[] { "A", "B" });
        }
    }


}
using ToDoApi.Services;
using FluentAssertions;
using System.Linq;
using Xunit;
using ToDoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ToDoApi.Controllers;
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
        public void Add_AddsItemWithIncrementingId_And_NotDone()
        {
            var svc = new TodoService();

            var a = svc.Add("first");
            var b = svc.Add("second");

            a.Id.Should().Be(1);
            b.Id.Should().Be(2);
            a.Done.Should().BeFalse();
            b.Done.Should().BeFalse();
        }

        [Fact]
        public void Toggle_TogglesDone_WhenExists()
        {
            var svc = new TodoService();
            var a = svc.Add("x");
            var ok = svc.Toggle(a.Id);
            ok.Should().BeTrue();
            svc.GetAll().Single(i => i.Id == a.Id).Done.Should().BeTrue();
        }

        [Fact]
        public void Toggle_ReturnsFalse_WhenNotFound()
        {
            var svc = new TodoService();
            svc.Toggle(123).Should().BeFalse();
        }

        [Fact]
        public void Add_DefaultsToMedium_WhenPriorityOmitted()
        {
            var svc = new TodoService();

            var item = svc.Add("task-with-default");

            item.Priority.Should().Be(Priority.Medium);
        }

        [Fact]
        public void Add_SetsPriority_WhenProvided()
        {
            var svc = new TodoService();

            var high = svc.Add("h", 1);
            var med = svc.Add("m", 2);
            var low = svc.Add("l", 3);

            high.Priority.Should().Be(Priority.High);
            med.Priority.Should().Be(Priority.Medium);
            low.Priority.Should().Be(Priority.Low);
        }

        [Theory]
        [InlineData(0, Priority.High)]
        [InlineData(-5, Priority.High)]
        [InlineData(4, Priority.Low)]
        [InlineData(99, Priority.Low)]
        public void Add_Clamps_OutOfRange_ToNearestValid(int input, Priority expected)
        {
            var svc = new TodoService();

            var item = svc.Add("x", input);

            item.Priority.Should().Be(expected);
        }

        [Fact]
        public void Toggle_DoesNotChangePriority()
        {
            var svc = new TodoService();
            var item = svc.Add("keep-priority", 1);

            var before = item.Priority;
            svc.Toggle(item.Id);
            var after = svc.GetAll().Single(i => i.Id == item.Id).Priority;

            before.Should().Be(Priority.High);
            after.Should().Be(Priority.High);
        }

        [Fact]
        public void Get_ReturnsOk_WithItemsFromService()
        {
            var svc = new Mock<ITodoService>();
            svc.Setup(s => s.GetAll()).Returns(new List<TodoItem> {
            new() { Id = 1, Title = "a", Priority = Priority.Medium }});

            var ctrl = new TodosController(svc.Object);

            var res = ctrl.Get();

            var ok = res.Result as OkObjectResult;
            ok.Should().NotBeNull();
            var items = ok!.Value as IReadOnlyList<TodoItem>;
            items.Should().NotBeNull();
            items!.Should().HaveCount(1);
            svc.Verify(s => s.GetAll(), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Post_ReturnsBadRequest_WhenTitleMissing(string? badTitle)
        {
            var svc = new Mock<ITodoService>();
            var ctrl = new TodosController(svc.Object);

            var res = ctrl.Post(new TodosController.AddTodoDto(badTitle ?? string.Empty));

            res.Result.Should().BeOfType<BadRequestObjectResult>();
            svc.Verify(s => s.Add(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Toggle_ReturnsNoContent_WhenServiceReturnsTrue()
        {
            var svc = new Mock<ITodoService>();
            svc.Setup(s => s.Toggle(5)).Returns(true);

            var ctrl = new TodosController(svc.Object);
            var res = ctrl.Toggle(5);

            res.Should().BeOfType<NoContentResult>();
            svc.Verify(s => s.Toggle(5), Times.Once);
        }

        [Fact]
        public void Toggle_ReturnsNotFound_WhenServiceReturnsFalse()
        {
            var svc = new Mock<ITodoService>();
            svc.Setup(s => s.Toggle(123)).Returns(false);

            var ctrl = new TodosController(svc.Object);
            var res = ctrl.Toggle(123);

            res.Should().BeOfType<NotFoundResult>();
            svc.Verify(s => s.Toggle(123), Times.Once);
        }

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
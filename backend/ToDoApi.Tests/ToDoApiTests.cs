using ToDoApi.Services;
using FluentAssertions;
using System.Linq;
using Xunit;
using ToDoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ToDoApi.Controllers;

namespace ToDoApi.Tests
{
    public class ToDoApiTests
    {
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
    }
}
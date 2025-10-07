using ToDoApi.Services;
using FluentAssertions;
using ToDoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ToDoApi.Controllers;

namespace ToDoApi.Tests
{
    public class ToDoControllerTests
    {

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

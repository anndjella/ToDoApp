using ToDoApi.Services;
using FluentAssertions;
using System.Linq;
using Xunit;
using ToDoApi.Models;

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
    }
}
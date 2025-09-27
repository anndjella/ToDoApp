using ToDoApi.Services;
using FluentAssertions;
using System.Linq;
using Xunit;

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
    }
}
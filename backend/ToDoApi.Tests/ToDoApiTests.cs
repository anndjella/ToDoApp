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
    }
}
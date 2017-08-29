using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using FluentAssertions;
using UblSharp.UnqualifiedDataTypes;
using Xunit;

namespace UblSharp.Tests
{
    public class BaseDocumentTests
    {
        [Fact]
        public void CanGetSetBaseProperties()
        {
            var order = new OrderType();
            order.ID = "1";
            order.ID.Value.Should().Be("1");
        }
    }

    public class BasicTypeTests
    {
        [Fact]
        public void TimeTypeStringAssignment()
        {
            var now = DateTime.Now;

            TimeType time = "15:30:00.0Z";
            time.Value.Should().Be(new DateTime(now.Year, now.Month, now.Day, 15, 30, 0, DateTimeKind.Utc));
            time.Value.Offset.Hours.Should().Be(0);
            time.ValueString.Should().Be("15:30:00Z");

            TimeType time2 = "15:30:00.0+03:00";
            time2.Value.ToUniversalTime().Should().Be(new DateTime(now.Year, now.Month, now.Day, 12, 30, 0, DateTimeKind.Utc));
            time2.Value.Offset.Hours.Should().Be(3);
            time2.ValueString.Should().Be("15:30:00+03:00");

            TimeType time3 = "15:30:00.5";
            time3.Value.Should().Be(new DateTime(now.Year, now.Month, now.Day, 15, 30, 0, 500, DateTimeKind.Unspecified));
            time3.Value.Offset.Should().Be(DateTimeOffset.Now.Offset);
            // time3.ValueString.Should().Be("15:30:00.5");
        }

        [Fact]
        public void TimeTests()
        {
            var order = new OrderType();

            order.IssueTime = "12:00:01.000";
            var time = order.IssueTime.Value;
            time.Hour.Should().Be(12);
            time.Minute.Should().Be(0);
            time.Second.Should().Be(1);
            time.Offset.Should().Be(TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow));

            order.IssueTime = "12:00:01.000+04:00";
            time = order.IssueTime.Value;
            time.Hour.Should().Be(12);
            time.Minute.Should().Be(0);
            time.Second.Should().Be(1);
            time.Offset.Should().Be(TimeSpan.FromHours(4));
        }
    }
}
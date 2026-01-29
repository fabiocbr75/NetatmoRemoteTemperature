using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TemperatureHub.Helpers;

namespace TemperatureHub.Tests.Helpers
{
    [TestClass]
    public class DateTimeHelperTests
    {
        [TestMethod]
        public void Next_WhenTargetDayIsAfterCurrentDay_ReturnsNextOccurrence()
        {
            // Arrange - Monday, January 1, 2024
            DateTime current = new DateTime(2024, 1, 1); // Monday
            DayOfWeek targetDay = DayOfWeek.Wednesday;

            // Act
            DateTime result = current.Next(targetDay);

            // Assert
            Assert.AreEqual(new DateTime(2024, 1, 3), result); // Wednesday
            Assert.AreEqual(DayOfWeek.Wednesday, result.DayOfWeek);
        }

        [TestMethod]
        public void Next_WhenTargetDayIsBeforeCurrentDay_ReturnsNextWeekOccurrence()
        {
            // Arrange - Friday, January 5, 2024
            DateTime current = new DateTime(2024, 1, 5); // Friday
            DayOfWeek targetDay = DayOfWeek.Monday;

            // Act
            DateTime result = current.Next(targetDay);

            // Assert
            Assert.AreEqual(new DateTime(2024, 1, 8), result); // Next Monday
            Assert.AreEqual(DayOfWeek.Monday, result.DayOfWeek);
        }

        [TestMethod]
        public void Next_WhenTargetDayIsSameAsCurrentDay_ReturnsNextWeekSameDay()
        {
            // Arrange - Wednesday, January 3, 2024
            DateTime current = new DateTime(2024, 1, 3); // Wednesday
            DayOfWeek targetDay = DayOfWeek.Wednesday;

            // Act
            DateTime result = current.Next(targetDay);

            // Assert
            Assert.AreEqual(new DateTime(2024, 1, 10), result); // Next Wednesday
            Assert.AreEqual(DayOfWeek.Wednesday, result.DayOfWeek);
        }

        [TestMethod]
        public void Next_WhenCurrentDayIsSunday_ReturnsCorrectNextDay()
        {
            // Arrange - Sunday, January 7, 2024
            DateTime current = new DateTime(2024, 1, 7); // Sunday
            DayOfWeek targetDay = DayOfWeek.Tuesday;

            // Act
            DateTime result = current.Next(targetDay);

            // Assert
            Assert.AreEqual(new DateTime(2024, 1, 9), result); // Tuesday
            Assert.AreEqual(DayOfWeek.Tuesday, result.DayOfWeek);
        }

        [TestMethod]
        public void Next_WhenTargetIsSunday_FromMonday_ReturnsNextSunday()
        {
            // Arrange - Monday, January 1, 2024
            DateTime current = new DateTime(2024, 1, 1); // Monday
            DayOfWeek targetDay = DayOfWeek.Sunday;

            // Act
            DateTime result = current.Next(targetDay);

            // Assert
            Assert.AreEqual(new DateTime(2024, 1, 7), result); // Sunday
            Assert.AreEqual(DayOfWeek.Sunday, result.DayOfWeek);
        }

        [TestMethod]
        public void Next_WhenCurrentIsSunday_AndTargetIsSunday_ReturnsNextWeek()
        {
            // Arrange - Sunday, January 7, 2024
            DateTime current = new DateTime(2024, 1, 7); // Sunday
            DayOfWeek targetDay = DayOfWeek.Sunday;

            // Act
            DateTime result = current.Next(targetDay);

            // Assert
            Assert.AreEqual(new DateTime(2024, 1, 14), result); // Next Sunday
            Assert.AreEqual(DayOfWeek.Sunday, result.DayOfWeek);
        }

        [TestMethod]
        public void Next_PreservesTimeComponent()
        {
            // Arrange - Monday with specific time
            DateTime current = new DateTime(2024, 1, 1, 14, 30, 45); // Monday 2:30:45 PM
            DayOfWeek targetDay = DayOfWeek.Wednesday;

            // Act
            DateTime result = current.Next(targetDay);

            // Assert
            Assert.AreEqual(new DateTime(2024, 1, 3, 14, 30, 45), result);
            Assert.AreEqual(14, result.Hour);
            Assert.AreEqual(30, result.Minute);
            Assert.AreEqual(45, result.Second);
        }
    }
}

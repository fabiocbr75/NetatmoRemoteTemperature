using Microsoft.VisualStudio.TestTools.UnitTesting;
using TemperatureHub.Helpers;

namespace TemperatureHub.Tests.Helpers
{
    [TestClass]
    public class NumberTests
    {
        [TestMethod]
        public void HalfRound_WithPositiveWholeNumber_ReturnsCorrectValue()
        {
            // Arrange
            double value = 10.0;

            // Act
            double result = Number.HalfRound(value);

            // Assert
            Assert.AreEqual(10.0, result);
        }

        [TestMethod]
        public void HalfRound_WithPositiveValueRoundingUp_ReturnsCorrectValue()
        {
            // Arrange
            double value = 10.3;

            // Act
            double result = Number.HalfRound(value);

            // Assert
            Assert.AreEqual(10.5, result);
        }

        [TestMethod]
        public void HalfRound_WithPositiveValueRoundingDown_ReturnsCorrectValue()
        {
            // Arrange
            double value = 10.2;

            // Act
            double result = Number.HalfRound(value);

            // Assert
            Assert.AreEqual(10.0, result);
        }

        [TestMethod]
        public void HalfRound_WithExactHalfValue_ReturnsHalfValue()
        {
            // Arrange
            double value = 10.5;

            // Act
            double result = Number.HalfRound(value);

            // Assert
            Assert.AreEqual(10.5, result);
        }

        [TestMethod]
        public void HalfRound_WithNegativeValue_ReturnsCorrectValue()
        {
            // Arrange
            double value = -10.3;

            // Act
            double result = Number.HalfRound(value);

            // Assert
            Assert.AreEqual(-10.5, result);
        }

        [TestMethod]
        public void HalfRound_WithZero_ReturnsZero()
        {
            // Arrange
            double value = 0.0;

            // Act
            double result = Number.HalfRound(value);

            // Assert
            Assert.AreEqual(0.0, result);
        }

        [TestMethod]
        public void HalfRound_WithValueBetweenHalfAndWhole_RoundsToNearestHalf()
        {
            // Arrange
            double value = 10.7; // 10.7 * 2 = 21.4 -> rounds to 21 -> 21/2 = 10.5

            // Act
            double result = Number.HalfRound(value);

            // Assert
            Assert.AreEqual(10.5, result);
        }

        [TestMethod]
        public void HalfRound_WithQuarterValue_UsesBankersRounding()
        {
            // Arrange
            double value = 10.25; // 10.25 * 2 = 20.5 -> banker's rounds to 20 (even) -> 20/2 = 10

            // Act
            double result = Number.HalfRound(value);

            // Assert
            Assert.AreEqual(10.0, result);
        }

        [TestMethod]
        public void HalfRound_WithThreeQuarterValue_RoundsUp()
        {
            // Arrange
            double value = 10.75; // 10.75 * 2 = 21.5 -> banker's rounds to 22 (even) -> 22/2 = 11

            // Act
            double result = Number.HalfRound(value);

            // Assert
            Assert.AreEqual(11.0, result);
        }
    }
}

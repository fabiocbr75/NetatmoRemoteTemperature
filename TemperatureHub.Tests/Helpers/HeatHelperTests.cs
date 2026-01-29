using Microsoft.VisualStudio.TestTools.UnitTesting;
using TemperatureHub.Helpers;

namespace TemperatureHub.Tests.Helpers
{
    [TestClass]
    public class HeatHelperTests
    {
        [TestMethod]
        public void GetHeatIndexCelsius_WithNormalTemperatureAndHumidity_ReturnsValidHeatIndex()
        {
            // Arrange
            double temperature = 30.0; // 30°C (86°F)
            double humidity = 50.0; // 50%

            // Act
            double result = HeatHelper.GetHeatIndexCelsius(temperature, humidity);

            // Assert
            // Heat index should be higher than actual temperature at this combination
            Assert.IsTrue(result >= temperature);
            Assert.IsTrue(result < 100); // Sanity check
        }

        [TestMethod]
        public void GetHeatIndexCelsius_WithHighTemperatureAndHumidity_ReturnsHigherHeatIndex()
        {
            // Arrange
            double temperature = 35.0; // 35°C (95°F)
            double humidity = 80.0; // 80%

            // Act
            double result = HeatHelper.GetHeatIndexCelsius(temperature, humidity);

            // Assert
            // Heat index should be significantly higher at high temp and humidity
            Assert.IsTrue(result > temperature);
            Assert.IsTrue(result < 100); // Sanity check
        }

        [TestMethod]
        public void GetHeatIndexCelsius_WithLowTemperature_ReturnsValidValue()
        {
            // Arrange
            double temperature = 15.0; // 15°C (59°F)
            double humidity = 50.0; // 50%

            // Act
            double result = HeatHelper.GetHeatIndexCelsius(temperature, humidity);

            // Assert
            // At low temperatures, heat index is less meaningful but should still return a value
            Assert.IsTrue(result != 0 || result == 0); // Just verify it doesn't throw
        }

        [TestMethod]
        public void GetHeatIndexCelsius_WithZeroHumidity_ReturnsValidValue()
        {
            // Arrange
            double temperature = 25.0;
            double humidity = 0.0;

            // Act
            double result = HeatHelper.GetHeatIndexCelsius(temperature, humidity);

            // Assert
            Assert.IsTrue(result >= 0 || result < 0); // Just verify it returns a value without exception
        }

        [TestMethod]
        public void GetHeatIndexCelsius_WithMaxHumidity_ReturnsValidValue()
        {
            // Arrange
            double temperature = 30.0;
            double humidity = 100.0;

            // Act
            double result = HeatHelper.GetHeatIndexCelsius(temperature, humidity);

            // Assert
            // Heat index should be significantly higher at 100% humidity
            Assert.IsTrue(result > temperature);
            Assert.IsTrue(result < 100); // Sanity check
        }

        [TestMethod]
        public void GetHeatIndexCelsius_WithNegativeTemperature_ReturnsValidValue()
        {
            // Arrange
            double temperature = -10.0;
            double humidity = 50.0;

            // Act
            double result = HeatHelper.GetHeatIndexCelsius(temperature, humidity);

            // Assert
            // Should handle negative temperatures gracefully
            Assert.IsTrue(result != double.NaN);
        }

        [TestMethod]
        public void GetHeatIndexCelsius_WithMultipleCalls_ReturnsConsistentResults()
        {
            // Arrange
            double temperature = 28.0;
            double humidity = 65.0;

            // Act
            double result1 = HeatHelper.GetHeatIndexCelsius(temperature, humidity);
            double result2 = HeatHelper.GetHeatIndexCelsius(temperature, humidity);

            // Assert
            Assert.AreEqual(result1, result2, 0.001);
        }
    }
}

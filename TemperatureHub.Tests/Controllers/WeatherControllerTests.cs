using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using TemperatureHub.Controllers;
using TemperatureHub.DTOs;
using TemperatureHub.Models;
using TemperatureHub.Repository;

namespace TemperatureHub.Tests.Controllers
{
    [TestClass]
    public class WeatherControllerTests
    {
        private Mock<ISQLiteFileRepository> _mockRepository;
        private Mock<ISharedData> _mockSharedData;
        private WeatherController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<ISQLiteFileRepository>();
            _mockSharedData = new Mock<ISharedData>();
            _controller = new WeatherController(_mockRepository.Object, _mockSharedData.Object);
        }

        [TestMethod]
        public void Get_WithValidId_ReturnsWeatherDTO()
        {
            // Arrange
            string id = "MAC1";
            var mockWeatherInfo = new List<WeatherInfo>
            {
                new WeatherInfo { Day = "2024-01-15", Max = 25.0, Min = 15.0 },
                new WeatherInfo { Day = "2024-01-16", Max = 26.0, Min = 16.0 }
            };

            var mockLastData = new Dictionary<string, (double Temperature, double Humidity, System.DateTime IngestionTime, double BatteryLevel, string SenderName, double ScheduledTemperature)>
            {
                { id, (22.0, 50.0, System.DateTime.UtcNow, 90, "Sensor1", 21.0) }
            };

            _mockRepository.Setup(r => r.LoadWeatherInfo(id, 5)).Returns(mockWeatherInfo);
            _mockSharedData.Setup(s => s.LastSensorData).Returns(mockLastData);

            // Act
            var result = _controller.Get(id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(id, result.Value.SenderMAC);
            Assert.AreEqual("Sensor1", result.Value.SenderName);
            Assert.AreEqual(22.0, result.Value.Temperature);
            Assert.AreEqual(50.0, result.Value.Humidity);
            Assert.IsNotNull(result.Value.WeatherInfo);
            Assert.AreEqual(5, result.Value.WeatherInfo.Length);
        }

        [TestMethod]
        public void Get_WithPartialWeatherData_FillsRemainingDays()
        {
            // Arrange
            string id = "MAC1";
            var mockWeatherInfo = new List<WeatherInfo>
            {
                new WeatherInfo { Day = "2024-01-15", Max = 25.0, Min = 15.0 }
            };

            var mockLastData = new Dictionary<string, (double Temperature, double Humidity, System.DateTime IngestionTime, double BatteryLevel, string SenderName, double ScheduledTemperature)>
            {
                { id, (22.0, 50.0, System.DateTime.UtcNow, 90, "Sensor1", 21.0) }
            };

            _mockRepository.Setup(r => r.LoadWeatherInfo(id, 5)).Returns(mockWeatherInfo);
            _mockSharedData.Setup(s => s.LastSensorData).Returns(mockLastData);

            // Act
            var result = _controller.Get(id);

            // Assert
            Assert.AreEqual(5, result.Value.WeatherInfo.Length);
            Assert.AreEqual(25.0, result.Value.WeatherInfo[0].Max);
            Assert.IsNotNull(result.Value.WeatherInfo[1]); // Should be initialized
        }

        [TestMethod]
        public void Get_WithException_ReturnsEmptyWeatherDTO()
        {
            // Arrange
            string id = "MAC1";
            _mockRepository.Setup(r => r.LoadWeatherInfo(It.IsAny<string>(), It.IsAny<int>()))
                .Throws(new Exception("Database error"));
            
            var mockLastData = new Dictionary<string, (double Temperature, double Humidity, System.DateTime IngestionTime, double BatteryLevel, string SenderName, double ScheduledTemperature)>();
            _mockSharedData.Setup(s => s.LastSensorData).Returns(mockLastData);

            // Act
            var result = _controller.Get(id);

            // Assert - Should return empty WeatherDTO instead of throwing
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Value);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using TemperatureHub.Controllers;
using TemperatureHub.DTOs;
using TemperatureHub.Models;
using TemperatureHub.Process;
using TemperatureHub.Repository;

namespace TemperatureHub.Tests.Controllers
{
    [TestClass]
    public class SensorDataControllerTests
    {
        private Mock<IProcessData> _mockProcessData;
        private Mock<ISQLiteFileRepository> _mockRepository;
        private Mock<ISharedData> _mockSharedData;
        private SensorDataController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockProcessData = new Mock<IProcessData>();
            _mockRepository = new Mock<ISQLiteFileRepository>();
            _mockSharedData = new Mock<ISharedData>();
            _controller = new SensorDataController(_mockProcessData.Object, _mockRepository.Object, _mockSharedData.Object);
        }

        [TestMethod]
        public void Get_WithValidParameters_ReturnsListOfSensorDataDTO()
        {
            // Arrange
            string id = "TEST123";
            string from = "2024-01-01";
            string to = "2024-01-31";
            
            var mockData = new List<AggregateDataEx>
            {
                new AggregateDataEx 
                { 
                    SenderMAC = id, 
                    SenderName = "Test Sensor",
                    Temperature = 22.5,
                    Humidity = 50.0,
                    IngestionTimestamp = "2024-01-15T10:00:00Z"
                }
            };

            _mockRepository.Setup(r => r.LoadSensorDataEx(id, from, to)).Returns(mockData);

            // Act
            var result = _controller.Get(id, from, to);

            // Assert
            Assert.IsNotNull(result);
            var value = result.Value as List<SensorDataExDTO>;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Count);
            Assert.AreEqual(id, value[0].MAC);
            Assert.AreEqual("Test Sensor", value[0].Name);
            Assert.AreEqual(22.5, value[0].Temp);
        }

        [TestMethod]
        public void Get_WithEmptyData_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.LoadSensorDataEx(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<AggregateDataEx>());

            // Act
            var result = _controller.Get("TEST", "2024-01-01", "2024-01-31");

            // Assert
            var value = result.Value as List<SensorDataExDTO>;
            Assert.IsNotNull(value);
            Assert.AreEqual(0, value.Count);
        }

        [TestMethod]
        public void LastTemperature_ReturnsAllSensorsLastStatus()
        {
            // Arrange
            var mockLastData = new Dictionary<string, (double Temperature, double Humidity, System.DateTime IngestionTime, double BatteryLevel, string SenderName, double ScheduledTemperature)>
            {
                { "MAC1", (20.0, 50.0, System.DateTime.UtcNow, 90, "Sensor1", 21.0) },
                { "MAC2", (22.0, 55.0, System.DateTime.UtcNow, 85, "Sensor2", 23.0) }
            };

            _mockSharedData.Setup(s => s.LastSensorData).Returns(mockLastData);

            // Act
            var result = _controller.LastTemperature();

            // Assert
            var value = result.Value as List<LastStatusDTO>;
            Assert.IsNotNull(value);
            Assert.AreEqual(2, value.Count);
            Assert.IsTrue(value.Any(x => x.MAC == "MAC1"));
            Assert.IsTrue(value.Any(x => x.MAC == "MAC2"));
        }

        [TestMethod]
        public void LastTemperature_WithId_ReturnsSpecificSensorStatus()
        {
            // Arrange
            string id = "MAC1";
            var mockLastData = new Dictionary<string, (double Temperature, double Humidity, System.DateTime IngestionTime, double BatteryLevel, string SenderName, double ScheduledTemperature)>
            {
                { id, (20.0, 50.0, System.DateTime.UtcNow, 90, "Sensor1", 21.0) }
            };

            _mockSharedData.Setup(s => s.LastSensorData).Returns(mockLastData);

            // Act
            var result = _controller.LastTemperature(id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(id, result.Value.MAC);
            Assert.AreEqual(20.0, result.Value.Temp);
            Assert.AreEqual(90, result.Value.BatteryLevel);
        }

        [TestMethod]
        public void LastTemperature_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            var mockLastData = new Dictionary<string, (double Temperature, double Humidity, System.DateTime IngestionTime, double BatteryLevel, string SenderName, double ScheduledTemperature)>
            {
                { "MAC1", (20.0, 50.0, System.DateTime.UtcNow, 90, "Sensor1", 21.0) }
            };

            _mockSharedData.Setup(s => s.LastSensorData).Returns(mockLastData);

            // Act
            var result = _controller.LastTemperature("NONEXISTENT");

            // Assert
            Assert.IsNull(result.Value);
        }

        [TestMethod]
        public void Post_WithValidData_CallsProcessDataAdd()
        {
            // Arrange
            var sensorDataDTO = new SensorDataDTO
            {
                MAC = "TEST123",
                Temp = 22.5,
                Humidity = 50.0,
                BatteryLevel = 95
            };

            // Act
            _controller.Post(sensorDataDTO);

            // Assert
            _mockProcessData.Verify(p => p.Add(It.Is<SensorData>(sd => 
                sd.SenderMAC == sensorDataDTO.MAC && 
                sd.Temperature == sensorDataDTO.Temp &&
                sd.Humidity == sensorDataDTO.Humidity &&
                sd.BatteryLevel == sensorDataDTO.BatteryLevel)), Times.Once);
        }
    }
}

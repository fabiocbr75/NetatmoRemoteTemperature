using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using TemperatureHub.Controllers;
using TemperatureHub.DTOs;
using TemperatureHub.Models;
using TemperatureHub.Repository;

namespace TemperatureHub.Tests.Controllers
{
    [TestClass]
    public class SensorMasterDataControllerTests
    {
        private Mock<ISQLiteFileRepository> _mockRepository;
        private SensorMasterDataController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<ISQLiteFileRepository>();
            _controller = new SensorMasterDataController(_mockRepository.Object);
        }

        [TestMethod]
        public void Get_ReturnsListOfSensorMasterData()
        {
            // Arrange
            var mockData = new List<SensorMasterData>
            {
                new SensorMasterData
                {
                    SenderMAC = "MAC1",
                    SenderName = "Sensor1",
                    RoomId = "Room1",
                    NetatmoSetTemp = true,
                    ExternalSensor = true
                },
                new SensorMasterData
                {
                    SenderMAC = "MAC2",
                    SenderName = "Sensor2",
                    RoomId = "Room2",
                    NetatmoSetTemp = false,
                    ExternalSensor = false
                }
            };

            _mockRepository.Setup(r => r.LoadSensorMasterData()).Returns(mockData);

            // Act
            var result = _controller.Get();

            // Assert
            Assert.IsNotNull(result);
            var value = result.Value as List<SensorMasterDataDTO>;
            Assert.IsNotNull(value);
            Assert.AreEqual(2, value.Count);
            Assert.AreEqual("MAC1", value[0].SenderMAC);
            Assert.AreEqual("Sensor1", value[0].SenderName);
        }

        [TestMethod]
        public void Get_WithEmptyData_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.LoadSensorMasterData()).Returns(new List<SensorMasterData>());

            // Act
            var result = _controller.Get();

            // Assert
            var value = result.Value as List<SensorMasterDataDTO>;
            Assert.IsNotNull(value);
            Assert.AreEqual(0, value.Count);
        }

        [TestMethod]
        public void SwitchPower_WithValidId_ReturnsSensorMasterData()
        {
            // Arrange
            string id = "MAC1";
            bool power = true;
            var mockData = new SensorMasterData
            {
                SenderMAC = id,
                SenderName = "Sensor1",
                RoomId = "Room1",
                NetatmoSetTemp = true,
                ExternalSensor = power
            };

            _mockRepository.Setup(r => r.SwitchPower(id, power)).Returns(mockData);

            // Act
            var result = _controller.SwitchPower(id, power);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(id, result.Value.SenderMAC);
            Assert.AreEqual(power, result.Value.ExternalSensor);
            _mockRepository.Verify(r => r.SwitchPower(id, power), Times.Once);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using TemperatureHub.Controllers;
using TemperatureHub.DTOs;
using TemperatureHub.Models;
using TemperatureHub.Process;
using TemperatureHub.Repository;

namespace TemperatureHub.Tests.Controllers
{
    [TestClass]
    public class MinMaxData4DayControllerTests
    {
        private Mock<IProcessData> _mockProcessData;
        private Mock<ISQLiteFileRepository> _mockRepository;
        private MinMaxData4DayController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockProcessData = new Mock<IProcessData>();
            _mockRepository = new Mock<ISQLiteFileRepository>();
            _controller = new MinMaxData4DayController(_mockProcessData.Object, _mockRepository.Object);
        }

        [TestMethod]
        public void Get_WithValidParameters_ReturnsListOfMinMaxData()
        {
            // Arrange
            string id = "TEST123";
            string from = "2024-01-01";
            string to = "2024-01-31";
            
            var mockData = new List<MinMaxData4Day>
            {
                new MinMaxData4Day
                {
                    SenderMAC = id,
                    Day = "2024-01-15",
                    MaxT = 25.789,
                    MinT = 15.123,
                    MaxTime = "14:00",
                    MinTime = "06:00"
                }
            };

            _mockRepository.Setup(r => r.LoadMinMaxData4Day(id, from, to)).Returns(mockData);

            // Act
            var result = _controller.Get(id, from, to);

            // Assert
            Assert.IsNotNull(result);
            var value = result.Value as List<MinMaxData4DayDTO>;
            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Count);
            Assert.AreEqual(id, value[0].MAC);
            Assert.AreEqual("2024-01-15", value[0].Day);
            Assert.AreEqual(25.8, value[0].Max); // Rounded to 1 decimal
            Assert.AreEqual(15.1, value[0].Min); // Rounded to 1 decimal
        }

        [TestMethod]
        public void Get_WithEmptyData_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.LoadMinMaxData4Day(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<MinMaxData4Day>());

            // Act
            var result = _controller.Get("TEST", "2024-01-01", "2024-01-31");

            // Assert
            var value = result.Value as List<MinMaxData4DayDTO>;
            Assert.IsNotNull(value);
            Assert.AreEqual(0, value.Count);
        }

        [TestMethod]
        public void Get_RoundsTemperaturesToOneDecimal()
        {
            // Arrange
            var mockData = new List<MinMaxData4Day>
            {
                new MinMaxData4Day
                {
                    SenderMAC = "TEST",
                    Day = "2024-01-15",
                    MaxT = 25.456,
                    MinT = 15.444,
                    MaxTime = "14:00",
                    MinTime = "06:00"
                }
            };

            _mockRepository.Setup(r => r.LoadMinMaxData4Day(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mockData);

            // Act
            var result = _controller.Get("TEST", "2024-01-01", "2024-01-31");

            // Assert
            var value = result.Value as List<MinMaxData4DayDTO>;
            Assert.AreEqual(25.5, value[0].Max);
            Assert.AreEqual(15.4, value[0].Min);
        }
    }
}

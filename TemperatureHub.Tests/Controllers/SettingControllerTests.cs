using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using TemperatureHub.Controllers;
using TemperatureHub.Models;

namespace TemperatureHub.Tests.Controllers
{
    [TestClass]
    public class SettingControllerTests
    {
        private Mock<IMemoryCache> _mockCache;
        private Mock<ISharedData> _mockSharedData;
        private SettingController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockCache = new Mock<IMemoryCache>();
            _mockSharedData = new Mock<ISharedData>();
            _controller = new SettingController(_mockCache.Object, _mockSharedData.Object);
        }

        [TestMethod]
        public void ClearCache_RemovesAllCacheEntries()
        {
            // Arrange
            var cacheKeys = new HashSet<string> { "key1", "key2", "key3" };
            _mockSharedData.Setup(s => s.CacheKey).Returns(cacheKeys);

            // Act
            var result = _controller.ClearCache();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
            _mockCache.Verify(c => c.Remove("key1"), Times.Once);
            _mockCache.Verify(c => c.Remove("key2"), Times.Once);
            _mockCache.Verify(c => c.Remove("key3"), Times.Once);
        }

        [TestMethod]
        public void ClearCache_WithEmptyCacheKeys_ReturnsOk()
        {
            // Arrange
            _mockSharedData.Setup(s => s.CacheKey).Returns(new HashSet<string>());

            // Act
            var result = _controller.ClearCache();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        public void ClearCache_WithExceptionOnRemove_ContinuesProcessing()
        {
            // Arrange
            var cacheKeys = new HashSet<string> { "key1", "key2" };
            _mockSharedData.Setup(s => s.CacheKey).Returns(cacheKeys);
            _mockCache.Setup(c => c.Remove("key1")).Throws(new Exception("Test exception"));

            // Act
            var result = _controller.ClearCache();

            // Assert - Should still return Ok even with exception
            Assert.IsInstanceOfType(result, typeof(OkResult));
            _mockCache.Verify(c => c.Remove("key2"), Times.Once);
        }
    }
}

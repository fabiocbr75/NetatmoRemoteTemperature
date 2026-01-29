using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using TemperatureHub.NetatmoData;

namespace TemperatureHub.Tests.NetatmoData
{
    [TestClass]
    public class NetatmoTokenTests
    {
        [TestMethod]
        public void Serialize_WithValidToken_ProducesCorrectJson()
        {
            // Arrange
            var token = new NetatmoToken
            {
                Access_token = "test_access_token",
                Refresh_token = "test_refresh_token",
                Scope = new[] { "read_thermostat", "write_thermostat" },
                Expires_in = 10800,
                Expire_in = 10800
            };

            // Act
            var json = JsonSerializer.Serialize(token);
            var deserialized = JsonSerializer.Deserialize<NetatmoToken>(json);

            // Assert
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(token.Access_token, deserialized.Access_token);
            Assert.AreEqual(token.Refresh_token, deserialized.Refresh_token);
            Assert.AreEqual(token.Scope.Length, deserialized.Scope.Length);
            Assert.AreEqual(token.Expires_in, deserialized.Expires_in);
            Assert.AreEqual(token.Expire_in, deserialized.Expire_in);
        }

        [TestMethod]
        public void Deserialize_WithSnakeCaseJson_CreatesValidToken()
        {
            // Arrange - JSON with snake_case properties as returned by Netatmo API
            var json = @"{
                ""access_token"": ""test_access_123"",
                ""refresh_token"": ""test_refresh_456"",
                ""scope"": [""read_thermostat"", ""write_thermostat""],
                ""expires_in"": 10800,
                ""expire_in"": 10800
            }";

            // Act
            var token = JsonSerializer.Deserialize<NetatmoToken>(json);

            // Assert
            Assert.IsNotNull(token);
            Assert.AreEqual("test_access_123", token.Access_token);
            Assert.AreEqual("test_refresh_456", token.Refresh_token);
            Assert.AreEqual(2, token.Scope.Length);
            Assert.AreEqual("read_thermostat", token.Scope[0]);
            Assert.AreEqual("write_thermostat", token.Scope[1]);
            Assert.AreEqual(10800, token.Expires_in);
            Assert.AreEqual(10800, token.Expire_in);
        }

        [TestMethod]
        public void Serialize_ProducesSnakeCasePropertyNames()
        {
            // Arrange
            var token = new NetatmoToken
            {
                Access_token = "test_token",
                Refresh_token = "refresh_token",
                Scope = new[] { "read_thermostat" },
                Expires_in = 3600,
                Expire_in = 3600
            };

            // Act
            var json = JsonSerializer.Serialize(token);

            // Assert
            Assert.IsTrue(json.Contains("\"access_token\""));
            Assert.IsTrue(json.Contains("\"refresh_token\""));
            Assert.IsTrue(json.Contains("\"scope\""));
            Assert.IsTrue(json.Contains("\"expires_in\""));
            Assert.IsTrue(json.Contains("\"expire_in\""));
        }

        [TestMethod]
        public void Deserialize_WithEmptyJson_CreatesTokenWithDefaults()
        {
            // Arrange
            var json = "{}";

            // Act
            var token = JsonSerializer.Deserialize<NetatmoToken>(json);

            // Assert
            Assert.IsNotNull(token);
            Assert.AreEqual(string.Empty, token.Access_token);
            Assert.AreEqual(string.Empty, token.Refresh_token);
            Assert.IsNotNull(token.Scope);
            Assert.AreEqual(0, token.Scope.Length);
            Assert.AreEqual(0, token.Expires_in);
            Assert.AreEqual(0, token.Expire_in);
        }

        [TestMethod]
        public void Deserialize_WithMissingProperties_UsesDefaults()
        {
            // Arrange
            var json = @"{
                ""access_token"": ""test_access""
            }";

            // Act
            var token = JsonSerializer.Deserialize<NetatmoToken>(json);

            // Assert
            Assert.IsNotNull(token);
            Assert.AreEqual("test_access", token.Access_token);
            Assert.AreEqual(string.Empty, token.Refresh_token);
            Assert.IsNotNull(token.Scope);
            Assert.AreEqual(0, token.Scope.Length);
            Assert.AreEqual(0, token.Expires_in);
            Assert.AreEqual(0, token.Expire_in);
        }

        [TestMethod]
        public void Deserialize_WithNullScope_CreatesEmptyArray()
        {
            // Arrange
            var json = @"{
                ""access_token"": ""test_access"",
                ""refresh_token"": ""test_refresh"",
                ""scope"": null,
                ""expires_in"": 10800,
                ""expire_in"": 10800
            }";

            // Act
            var token = JsonSerializer.Deserialize<NetatmoToken>(json);

            // Assert
            Assert.IsNotNull(token);
            Assert.IsNull(token.Scope); // System.Text.Json will keep it as null
        }

        [TestMethod]
        public void RoundTrip_SerializeAndDeserialize_PreservesData()
        {
            // Arrange
            var original = new NetatmoToken
            {
                Access_token = "access_123",
                Refresh_token = "refresh_456",
                Scope = new[] { "read_thermostat", "write_thermostat", "read_station" },
                Expires_in = 7200,
                Expire_in = 7200
            };

            // Act
            var json = JsonSerializer.Serialize(original);
            var roundTrip = JsonSerializer.Deserialize<NetatmoToken>(json);

            // Assert
            Assert.IsNotNull(roundTrip);
            Assert.AreEqual(original.Access_token, roundTrip.Access_token);
            Assert.AreEqual(original.Refresh_token, roundTrip.Refresh_token);
            Assert.AreEqual(original.Scope.Length, roundTrip.Scope.Length);
            for (int i = 0; i < original.Scope.Length; i++)
            {
                Assert.AreEqual(original.Scope[i], roundTrip.Scope[i]);
            }
            Assert.AreEqual(original.Expires_in, roundTrip.Expires_in);
            Assert.AreEqual(original.Expire_in, roundTrip.Expire_in);
        }
    }
}

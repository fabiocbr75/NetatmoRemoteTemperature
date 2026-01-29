using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using TemperatureHub.Repository;

namespace TemperatureHub.Tests.Repository
{
    [TestClass]
    public class ExecutorTests
    {
        [TestMethod]
        public void Run_WithValidAction_ExecutesAction()
        {
            // Arrange
            var executor = new Executor();
            executor.StartExecutionLoop();
            bool actionExecuted = false;

            // Act
            executor.Run(() => { actionExecuted = true; });

            // Wait a bit for async execution
            Thread.Sleep(100);

            // Assert
            Assert.IsTrue(actionExecuted);

            // Cleanup
            executor.Dispose();
        }

        [TestMethod]
        public void Run_WithMultipleActions_ExecutesAllActions()
        {
            // Arrange
            var executor = new Executor();
            executor.StartExecutionLoop();
            int counter = 0;

            // Act
            executor.Run(() => { counter++; });
            executor.Run(() => { counter++; });
            executor.Run(() => { counter++; });

            // Wait for all actions to complete
            Thread.Sleep(300);

            // Assert
            Assert.AreEqual(3, counter);

            // Cleanup
            executor.Dispose();
        }

        [TestMethod]
        public void StartExecutionLoop_StartsExecutionThread()
        {
            // Arrange
            var executor = new Executor();

            // Act
            executor.StartExecutionLoop();

            // Allow thread to start
            Thread.Sleep(50);
            bool actionExecuted = false;
            executor.Run(() => { actionExecuted = true; });
            Thread.Sleep(100);

            // Assert
            Assert.IsTrue(actionExecuted);

            // Cleanup
            executor.Dispose();
        }

        [TestMethod]
        public void Dispose_CleansUpResources()
        {
            // Arrange
            var executor = new Executor();
            executor.StartExecutionLoop();
            executor.Run(() => { Thread.Sleep(10); });

            // Act
            executor.Dispose();

            // Assert - Should not throw
            Assert.IsTrue(true); // Dispose completed successfully
        }

        [TestMethod]
        public void Run_WithExceptionInAction_ContinuesExecution()
        {
            // Arrange
            var executor = new Executor();
            executor.StartExecutionLoop();
            bool secondActionExecuted = false;

            // Act
            executor.Run(() => { throw new Exception("Test exception"); });
            executor.Run(() => { secondActionExecuted = true; });

            // Wait for actions to complete
            Thread.Sleep(200);

            // Assert - Second action should still execute despite first one throwing
            Assert.IsTrue(secondActionExecuted);

            // Cleanup
            executor.Dispose();
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeamCarving.Classes;
using SeamCarving.Interfaces;
using Moq;

namespace UnitTests
{
    [TestClass]
    public class TestOf_SeamCalculator
    {
        [TestMethod]
        public void basicSuccess()
        {
            int x = 3;
            int y = 3;

            var mock = new Mock<ISeamUtilities>();
            mock.Setup(m => m.findMinIndex(It.IsAny<int[]>(), It.IsAny<SeamCarvingContext>())).Returns(0);

            var testContext = new SeamCarvingContext() { energy = new int[x * y], dirtyArray = new int[x * y] };

            for (int i = 0; i < x; i++)
            {
                testContext.energy[i] = 0;
            }
            for (int i = x; i < testContext.energy.Length; i++)
            {
                testContext.energy[i] = Int32.MaxValue;
            }

            var testSeamCalculator = new SeamCalculator(mock.Object);

            testSeamCalculator.calculateSeam(testContext);

            for (int i = 0; i < x; i++)
            {
                Assert.AreEqual(testContext.dirtyArray[i], 1);
            }
            for (int i = x; i < testContext.energy.Length; i++)
            {
                Assert.AreEqual(testContext.dirtyArray[i], 0);
            }
            
        }
    }
}

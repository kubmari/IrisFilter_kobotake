using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using IrisFilter_kobotake;

namespace ProcessingTests
{
    [TestClass]
    public class calculateAngleBetweenInterestAndPixelTests
    {
        [TestMethod]
        public void AcuteAngleTest()
        {
            //arrange
            Processing.CovergenceImageFilter testFilter = new Processing.CovergenceImageFilter();
            int startPixelX = 0;
            int startPixelY = 0;

            int endPixelX = 1;
            int endPixelY = 1;

            int gradientX = 0;
            int gradientY = 0;

            //act
            testFilter.

            //assert
        }

        [TestMethod]
        public void RightAngleTest()
        {
        }

        [TestMethod]
        public void StraightAngleTest()
        {
        }

        [TestMethod]
        public void ObtuseAngleTest()
        {
        }

        [TestMethod]
        public void ReflexAngleTest()
        {
        }

    }
}

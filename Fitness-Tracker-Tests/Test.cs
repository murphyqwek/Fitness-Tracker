using System;
using System.Collections.Generic;
using System.Text;

namespace Fitness_Tracker_Tests
{
    public class Test
    {
        [Fact]
        public void Test1()
        {
            // Arrange
            int expected = 5;
            // Act
            int actual = 2 + 3;
            // Assert
            Assert.Equal(expected, actual);
        }
    }
}

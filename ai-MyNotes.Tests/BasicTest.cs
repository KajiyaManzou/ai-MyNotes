using Xunit;

namespace ai_MyNotes.Tests
{
    public class BasicTest
    {
        [Fact]
        public void BasicMathTest_ShouldPass()
        {
            // Arrange
            var a = 2;
            var b = 3;
            var expected = 5;

            // Act
            var result = a + b;

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void StringConcatenationTest_ShouldPass()
        {
            // Arrange
            var str1 = "Hello";
            var str2 = "World";
            var expected = "HelloWorld";

            // Act
            var result = str1 + str2;

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
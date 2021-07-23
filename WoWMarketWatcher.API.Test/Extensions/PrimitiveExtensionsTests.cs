using System;
using WoWMarketWatcher.API.Extensions;
using Xunit;

namespace WoWMarketWatcher.API.Test.Extensions
{


    /// <summary>
    /// The primitive extensions tests.
    /// </summary>
    public class PrimitiveExtensionsTests
    {
        #region ConvertToInt32FromBase64 tests

        /// <summary>
        /// ConvertToInt32FromBase64_ExpectedResponse.
        /// </summary>
        [Fact]
        public void ConvertToInt32FromBase64_ExpectedResponse()
        {
            // Arrange
            var base64Int = "AQAAAA==";

            // Act
            var result = base64Int.ConvertToInt32FromBase64Url();

            // Assert
            Assert.Equal(1, result);
        }

        /// <summary>
        /// ConvertToInt32FromBase64_ThrowsArgumentException.
        /// </summary>
        [Fact]
        public void ConvertToInt32FromBase64_ThrowsArgumentException()
        {
            // Arrange
            var base64Int = "not real";

            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => base64Int.ConvertToInt32FromBase64Url());
        }

        #endregion

        #region ConvertToInt32FromBase64 tests

        /// <summary>
        /// ConvertToInt32FromBase64_ExpectedResponse.
        /// </summary>
        [Fact]
        public void ConvertToStringFromBase64_ExpectedResponse()
        {
            // Arrange
            var base64Int = "dGVzdA==";

            // Act
            var result = base64Int.ConvertToStringFromBase64Url();

            // Assert
            Assert.Equal("test", result);
        }

        /// <summary>
        /// ConvertToStringFromBase64_ThrowsArgumentException.
        /// </summary>
        [Fact]
        public void ConvertToStringFromBase64_ThrowsArgumentException()
        {
            // Arrange
            var base64Int = "not real";

            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => base64Int.ConvertToStringFromBase64Url());
        }

        #endregion

        #region IsValidBase64EncodedInt32 tests

        /// <summary>
        /// IsValidBase64EncodedInt32_True_ExpectedResponse.
        /// </summary>
        [Fact]
        public void IsValidBase64EncodedInt32_True_ExpectedResponse()
        {
            // Arrange
            var base64Int = "AQAAAA==";

            // Act
            var result = base64Int.IsValidBase64UrlEncodedInt32();

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// IsValidBase64EncodedInt32_False_ExpectedResponse.
        /// </summary>
        [Fact]
        public void IsValidBase64EncodedInt32_False_ExpectedResponse()
        {
            // Arrange
            var base64Int = "this is not a valid base64 encoded int";

            // Act
            var result = base64Int.IsValidBase64UrlEncodedInt32();

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsValidBase64EncodedString tests

        /// <summary>
        /// IsValidBase64EncodedString_True_ExpectedResponse.
        /// </summary>
        [Fact]
        public void IsValidBase64EncodedString_True_ExpectedResponse()
        {
            // Arrange
            var base64Int = "dGVzdA==";

            // Act
            var result = base64Int.IsValidBase64UrlEncodedString();

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// IsValidBase64EncodedString_False_ExpectedResponse.
        /// </summary>
        [Fact]
        public void IsValidBase64EncodedString_False_ExpectedResponse()
        {
            // Arrange
            var base64Int = "this is not a valid base64 encoded string";

            // Act
            var result = base64Int.IsValidBase64UrlEncodedString();

            // Assert
            Assert.False(result);
        }

        #endregion

        #region ConvertToBase64 tests

        /// <summary>
        /// ConvertToBase64_String_ExpectedResponse.
        /// </summary>
        [Fact]
        public void ConvertToBase64_String_ExpectedResponse()
        {
            // Arrange
            var str = "test";

            // Act
            var result = str.ConvertToBase64Url();

            // Assert
            Assert.Equal("dGVzdA", result);
        }

        /// <summary>
        /// ConvertToStringFromBase64_ThrowsArgumentException.
        /// </summary>
        [Fact]
        public void ConvertToBase64_Int_ExpectedResponse()
        {
            // Arrange
            var num = 1;

            // Act
            var result = num.ConvertToBase64Url();

            // Assert
            Assert.Equal("AQAAAA", result);
        }

        #endregion
    }
}
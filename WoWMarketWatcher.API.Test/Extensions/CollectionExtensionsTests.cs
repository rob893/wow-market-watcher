using System;
using System.Collections.Generic;
using System.Linq;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.QueryParameters;
using WoWMarketWatcher.API.Models.Responses.Pagination;
using Xunit;

namespace WoWMarketWatcher.API.Test.Extensions
{
    /// <summary>
    /// The collection extensions tests.
    /// </summary>
    public class CollectionExtensionsTests
    {
        private readonly Func<TestItem, int> intKeySelector = i => i.IntId;
        private readonly Func<int, string> intKeyConverter = key => key.ConvertToBase64Url();
        private readonly Func<string, int> intCursorConverter = cursor => cursor.ConvertToInt32FromBase64Url();

        /// <summary>
        /// Asserts throwing on null inputs.
        /// </summary>
        /// <param name="nullKeySelector">If key selector should be null.</param>
        /// <param name="nullKeyConverter">If cursor selector should be null.</param>
        /// <param name="nullCursorConverter">If cursor converter should be null.</param>
        /// <param name="nullParameters">If parameters should be null.</param>
        [Theory]
        [InlineData(true, true, true, true)]
        [InlineData(false, true, true, true)]
        [InlineData(true, false, true, true)]
        [InlineData(true, true, false, true)]
        [InlineData(true, true, true, false)]
        [InlineData(false, false, true, true)]
        [InlineData(false, true, false, true)]
        [InlineData(false, true, true, false)]
        [InlineData(true, false, false, true)]
        [InlineData(true, false, true, false)]
        [InlineData(true, true, false, false)]
        [InlineData(true, false, false, false)]
        [InlineData(false, true, false, false)]
        [InlineData(false, false, true, false)]
        [InlineData(false, false, false, true)]
        public void ToCursorPaginatedResponse_NullArguments_ThrowsArgumentNullException(
            bool nullKeySelector,
            bool nullKeyConverter,
            bool nullCursorConverter,
            bool nullParameters)
        {
            // Arrange
            var items = GetTestItems();
            var selector = nullKeySelector ? null : this.intKeySelector;
            var keyConverter = nullKeyConverter ? null : this.intKeyConverter;
            var cursorConverter = nullCursorConverter ? null : this.intCursorConverter;
            var queryParameters = nullParameters ? null : new CursorPaginationQueryParameters();

            // Act
            // Assert
            Assert.Throws<ArgumentNullException>(() => items.ToCursorPaginatedResponse(selector!, keyConverter!, cursorConverter!, queryParameters!));
        }

        /// <summary>
        /// Asserts that passing both first and last throws not supported.
        /// </summary>
        [Fact]
        public void ToCursorPaginatedResponse_BothFirstAndLast_ThrowsNotSupportedException()
        {
            // Arrange
            var items = GetTestItems();
            var queryParameters = new CursorPaginationQueryParameters
            {
                First = 10,
                Last = 10
            };

            // Act
            // Assert
            Assert.Throws<NotSupportedException>(() => items.ToCursorPaginatedResponse(this.intKeySelector, this.intKeyConverter, this.intCursorConverter, queryParameters));
        }

        /// <summary>
        /// Asserts passing invalid first or last throws argument exception.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="last">The last.</param>
        [Theory]
        [InlineData(-1, null)]
        [InlineData(null, -1)]
        public void ToCursorPaginatedResponse_InvalidFirstOrLast_ThrowsArgumentException(int? first, int? last)
        {
            // Arrange
            var items = GetTestItems();
            var queryParameters = new CursorPaginationQueryParameters
            {
                First = first,
                Last = last
            };

            // Act
            // Assert
            Assert.Throws<ArgumentException>(() => items.ToCursorPaginatedResponse(this.intKeySelector, this.intKeyConverter, this.intCursorConverter, queryParameters));
        }

        /// <summary>
        /// Asserts ToCursorPaginatedResponse returns an instance of CursorPaginatedResponse.
        /// </summary>
        [Fact]
        public void ToCursorPaginatedResponse_ReturnsCursorPaginatedResponse()
        {
            // Arrange
            var items = GetTestItems();
            var queryParameters = new CursorPaginationQueryParameters();

            // Act
            var result = items.ToCursorPaginatedResponse(this.intKeySelector, this.intKeyConverter, this.intCursorConverter, queryParameters);

            // Assert
            Assert.IsType<CursorPaginatedResponse<TestItem, int>>(result);
        }

        /// <summary>
        /// Asserts ToCursorPaginatedResponse returns all items when first, last, before, and after are null.
        /// </summary>
        [Fact]
        public void ToCursorPaginatedResponse_ReturnsAllItems()
        {
            // Arrange
            var items = GetTestItems();
            var queryParameters = new CursorPaginationQueryParameters
            {
                First = null,
                Last = null,
                After = null,
                Before = null
            };

            // Act
            var result = items.ToCursorPaginatedResponse(this.intKeySelector, this.intKeyConverter, this.intCursorConverter, queryParameters);
            var expectedItems = items.OrderBy(this.intKeySelector);

            // Assert
            Assert.Equal(items.Count, result.Nodes?.Count());
            Assert.Equal(items.Count, result.PageInfo.PageCount);
            Assert.Equal(expectedItems, result.Nodes);
            Assert.False(result.PageInfo.HasNextPage);
            Assert.False(result.PageInfo.HasPreviousPage);
        }

        /// <summary>
        /// Asserts usage of first.
        /// </summary>
        /// <param name="first">Number of first items.</param>
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public void ToCursorPaginatedResponse_ReturnsFirstN(int first)
        {
            // Arrange
            var items = GetTestItems(first * 2);
            var queryParameters = new CursorPaginationQueryParameters
            {
                First = first
            };

            // Act
            var result = items.ToCursorPaginatedResponse(this.intKeySelector, this.intKeyConverter, this.intCursorConverter, queryParameters);
            var expectedItems = items.OrderBy(this.intKeySelector).Take(first);

            // Assert
            Assert.Equal(first, result.Nodes?.Count());
            Assert.Equal(first, result.PageInfo.PageCount);
            Assert.Equal(expectedItems, result.Nodes);
        }

        /// <summary>
        /// Asserts usage of last.
        /// </summary>
        /// <param name="last">Number of last items.</param>
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public void ToCursorPaginatedResponse_ReturnsLasttN(int last)
        {
            // Arrange
            var items = GetTestItems(last * 2);
            var queryParameters = new CursorPaginationQueryParameters
            {
                Last = last
            };

            // Act
            var result = items.ToCursorPaginatedResponse(this.intKeySelector, this.intKeyConverter, this.intCursorConverter, queryParameters);
            var expectedItems = items.OrderBy(this.intKeySelector).TakeLast(last);

            // Assert
            Assert.Equal(last, result.Nodes?.Count());
            Assert.Equal(last, result.PageInfo.PageCount);
            Assert.Equal(expectedItems, result.Nodes);
        }

        /// <summary>
        /// Asserts usage of after.
        /// </summary>
        /// <param name="after">Id for after.</param>
        [Theory]
        [InlineData(5)]
        [InlineData(50)]
        [InlineData(500)]
        public void ToCursorPaginatedResponse_ReturnsItemsAfter(int after)
        {
            // Arrange
            var items = GetTestItems(after);
            var queryParameters = new CursorPaginationQueryParameters
            {
                After = this.intKeyConverter(after)
            };

            // Act
            var result = items.ToCursorPaginatedResponse(this.intKeySelector, this.intKeyConverter, this.intCursorConverter, queryParameters);
            var expectedItems = items.OrderBy(this.intKeySelector).Where(i => i.IntId > after);

            // Assert
            Assert.Equal(expectedItems, result.Nodes);
        }

        /// <summary>
        /// Asserts usage of before.
        /// </summary>
        /// <param name="before">Id for before.</param>
        [Theory]
        [InlineData(5)]
        [InlineData(50)]
        [InlineData(500)]
        public void ToCursorPaginatedResponse_ReturnsItemsBefore(int before)
        {
            // Arrange
            var items = GetTestItems(before);
            var queryParameters = new CursorPaginationQueryParameters
            {
                Before = this.intKeyConverter(before)
            };

            // Act
            var result = items.ToCursorPaginatedResponse(this.intKeySelector, this.intKeyConverter, this.intCursorConverter, queryParameters);
            var expectedItems = items.OrderBy(this.intKeySelector).Where(i => i.IntId < before);

            // Assert
            Assert.Equal(expectedItems, result.Nodes);
        }

        private static List<TestItem> GetTestItems(int number = 10)
        {
            return Enumerable.Range(0, number).Select(_ =>
            {
                // Random to ensure sorting logic works.
                var random = new Random().Next(0, number * 10);
                return new TestItem
                {
                    IntId = random,
                    StringId = $"{random}"
                };
            }).ToList();
        }

        private record TestItem
        {
            public int IntId { get; init; }

            public string StringId { get; init; } = default!;
        }
    }
}
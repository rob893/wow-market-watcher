using System.Collections.Generic;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Test.TestUtilities;
using Xunit;

namespace WoWMarketWatcher.API.Test.Extensions
{
    public class UtilityExtensionsTests
    {
        [Fact]
        public void ToQueryString_Dictionary_ShouldReturnString()
        {
            var obj = new Dictionary<string, string>
            {
                { "Foo", "Bar" },
                { "Baz", "Bax" }
            };

            var query = obj.ToQueryString();

            Assert.Equal("Foo=Bar&Baz=Bax", query);
        }

        [Fact]
        public void ToQueryString_DictionaryMisc_ShouldReturnString()
        {
            var obj = new Dictionary<string, object>
            {
                { "Foo", "Bar" },
                { "Baz", "Bax" },
                { "List", new List<object>
                    {
                        "L1", "L2", "L3", 3, false
                    }
                }
            };

            var query = obj.ToQueryString();

            Assert.Equal("Foo=Bar&Baz=Bax&List=L1&List=L2&List=L3&List=3&List=False", query);
        }

        [Fact]
        public void ToQueryString_DictionaryMisc_ShouldNotReturnNullProperty()
        {
            var obj = new Dictionary<string, object?>
            {
                { "Foo", "Bar" },
                { "Baz", null }
            };

            var query = obj.ToQueryString();

            Assert.Equal("Foo=Bar", query);
        }

        [Fact]
        public void ToQueryString_Class_ShouldReturnString()
        {
            var obj = new TestClass
            {
                Foo = "Bar",
                Bar = 10
            };

            var query = obj.ToQueryString();

            Assert.Equal("Foo=Bar&Bar=10", query);
        }

        [Fact]
        public void ToQueryString_AnonType_ShouldReturnString()
        {
            var obj = new
            {
                Foo = "Bar",
                Bar = 10
            };

            var query = obj.ToQueryString();

            Assert.Equal("Foo=Bar&Bar=10", query);
        }

        [Theory]
        [InlineData("test")]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData('c')]
        [InlineData(5)]
        [InlineData(5.5)]
        [InlineData(null)]
        public void ToQueryString_PrimitiveOrStringOrNull_ShouldReturnEmptyString(object obj)
        {
            var query = obj.ToQueryString();

            Assert.Equal(string.Empty, query);
        }
    }
}
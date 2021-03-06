using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Web;
using Newtonsoft.Json;
using WoWMarketWatcher.Common.Constants;

namespace WoWMarketWatcher.Common.Extensions
{
    public static class UtilityExtensions
    {
        public static bool TryGetUserId(this ClaimsPrincipal principal, [NotNullWhen(true)] out int? userId)
        {
            userId = null;

            var nameIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);

            if (nameIdClaim == null)
            {
                return false;
            }

            if (int.TryParse(nameIdClaim.Value, out var value))
            {
                userId = value;
                return true;
            }

            return false;
        }

        public static bool IsAdmin(this ClaimsPrincipal principal)
        {
            return principal.IsInRole(UserRoleName.Admin);
        }

        public static bool HasProperty(this object obj, string property)
        {
            return obj != null && obj.GetType().GetProperty(property) != null;
        }

        public static int ConvertToInt32FromBase64(this string str)
        {
            try
            {
                return BitConverter.ToInt32(Convert.FromBase64String(str), 0);
            }
            catch
            {
                throw new ArgumentException($"{str} is not a valid base 64 encoded int32.");
            }
        }

        public static string ConvertInt32ToBase64(this int i)
        {
            return Convert.ToBase64String(BitConverter.GetBytes(i));
        }

        public static IDictionary<string, object> ToDictionary(this object source)
        {
            return source.ToDictionary<object>();
        }

        public static IDictionary<string, T> ToDictionary<T>(this object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "Unable to convert object to a dictionary. The source object is null.");
            }

            var dictionary = new Dictionary<string, T>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(source))
            {
                AddPropertyToDictionary(property, source, dictionary);
            }

            return dictionary;
        }

        public static bool IsBetween(this DateTime input, DateTime startDate, DateTime endDate)
        {
            return endDate < startDate
                ? throw new ArgumentException($"{nameof(startDate)} must be less than {nameof(endDate)}.")
                : input >= startDate && input <= endDate;
        }

        public static string ToQueryString(this object obj)
        {
            if (obj == null || obj.GetType().IsPrimitive || obj is string)
            {
                return string.Empty;
            }

            var results = new List<string>();
            var asJson = obj.ToJson();
            var asDict = JsonConvert.DeserializeObject<Dictionary<string, object?>>(asJson);

            foreach (var entry in asDict)
            {
                if (entry.Key == null || entry.Value == null)
                {
                    continue;
                }

                if (!(entry.Value is string) && entry.Value is IEnumerable<object> enumberable)
                {
                    results.AddRange(enumberable.Select(e => $"{HttpUtility.UrlEncode(entry.Key)}={HttpUtility.UrlEncode(e.ToString())}"));
                }
                else
                {
                    results.Add($"{HttpUtility.UrlEncode(entry.Key)}={HttpUtility.UrlEncode(entry.Value.ToString())}");
                }
            }

            return string.Join("&", results);
        }

        public static string ToJson(this object value, Formatting formatting = Formatting.None)
        {
            if (value == null)
            {
                return "null";
            }

            try
            {
                var json = JsonConvert.SerializeObject(value, formatting);

                return json;
            }
            catch (Exception ex)
            {
                return $"Exception - {ex?.Message}";
            }
        }

        private static void AddPropertyToDictionary<T>(PropertyDescriptor property, object source, Dictionary<string, T> dictionary)
        {
            var value = property.GetValue(source);
            if (IsOfType<T>(value))
            {
                dictionary.Add(property.Name, (T)value);
            }
        }

        private static bool IsOfType<T>(object value)
        {
            return value is T;
        }
    }
}
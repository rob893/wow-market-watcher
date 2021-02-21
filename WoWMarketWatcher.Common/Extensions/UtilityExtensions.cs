using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;

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

        public static string GetSourceName(
            this object _,
            [CallerFilePath]
            string sourceFilePath = "",
            [CallerMemberName]
            string memberName = "")
        {
            var sourceName = string.Empty;
            if (!string.IsNullOrWhiteSpace(sourceFilePath))
            {
                sourceName = sourceFilePath.Contains('\\')
                    ? sourceFilePath.Split('\\').Last().Split('.').First()
                    : sourceFilePath.Split('/').Last().Split('.').First();
            }

            return $"{sourceName}.{memberName}";
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
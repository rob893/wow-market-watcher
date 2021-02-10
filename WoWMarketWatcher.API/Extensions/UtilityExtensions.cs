using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace WoWMarketWatcher.API.Extensions
{
    public static class UtilityExtensions
    {
        public static bool IsValid<T>(this JsonPatchDocument<T> patchDoc, [NotNullWhen(false)] out List<string>? errors)
            where T : class, new()
        {
            errors = null;

            try
            {
                patchDoc.ApplyTo(new T());
                return true;
            }
            catch (Exception error)
            {
                errors = new List<string> { error.Message };

                return false;
            }
        }
    }
}
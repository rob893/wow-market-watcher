using System.Linq;
using System.Runtime.CompilerServices;

namespace WoWMarketWatcher.Common.Utilities
{
    public static class UtilityFunctions
    {
        public static string GetSourceName(
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
    }
}
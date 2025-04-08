using System.Text.Json;

namespace ClassManagement.Utilities
{
    public sealed class Utils
    {
        private static readonly Lazy<Utils> instance = new(() => new Utils());
        public static Utils Instance => instance.Value;

        private Utils() { }

        public string ExportToJson<T>(List<T> data, List<string>? selectedColumns = null)
        {
            if (selectedColumns == null || selectedColumns.Count == 0)
            {
                return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            }

            var filteredData = data.Select(item =>
            {
                var dict = new Dictionary<string, object?>();
                var properties = typeof(T).GetProperties();
                foreach (var prop in properties)
                {
                    if (selectedColumns.Contains(prop.Name))
                    {
                        dict[prop.Name] = prop.GetValue(item);
                    }
                }
                return dict;
            }).ToList();

            return JsonSerializer.Serialize(filteredData, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}

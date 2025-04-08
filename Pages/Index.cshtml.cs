using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using ClassManagement.Models;
using ClassManagement.Utilities;

namespace ClassManagement.Pages
{
    public class IndexModel : PageModel
    {
        public static List<ClassInformationModel> ClassList { get; set; } = new List<ClassInformationModel>();

        [BindProperty]
        public ClassInformationModel NewClass { get; set; } = new ClassInformationModel();

        public List<ClassInformationTable> FilteredData { get; set; } = new List<ClassInformationTable>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public List<string> SelectedColumns { get; set; } = new List<string>();

        public IActionResult OnGet(string searchClassName, int? minStudents, int? maxStudents, int pageIndex = 1, int pageSize = 6, List<string> selectedColumns = null)
        {
            SelectedColumns = selectedColumns ?? new List<string>();

            var filteredList = ClassList.AsQueryable();

            if (!string.IsNullOrEmpty(searchClassName))
                filteredList = filteredList.Where(c => c.ClassName.Contains(searchClassName));

            if (minStudents.HasValue)
                filteredList = filteredList.Where(c => c.StudentCount >= minStudents.Value);

            if (maxStudents.HasValue)
                filteredList = filteredList.Where(c => c.StudentCount <= maxStudents.Value);

            var totalRecords = filteredList.Count();
            FilteredData = filteredList.Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(c => new ClassInformationTable
                {
                    Id = c.Id,
                    ClassName = c.ClassName,
                    StudentCount = c.StudentCount,
                    Description = c.Description
                }).ToList();

            CurrentPage = pageIndex;
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            return Page();
        }

        public IActionResult OnPostAdd()
        {
            if (!ModelState.IsValid)
                return Page();

            int newId = ClassList.Count > 0 ? ClassList.Max(x => x.Id) + 1 : 1;
            NewClass.Id = newId;
            ClassList.Add(NewClass);
            NewClass = new ClassInformationModel();
            return RedirectToPage(new
            {
                searchClassName = Request.Query["searchClassName"],
                minStudents = Request.Query["minStudents"],
                maxStudents = Request.Query["maxStudents"],
                pageIndex = CurrentPage
            });
        }

        public IActionResult OnPostDelete(int id)
        {
            var item = ClassList.FirstOrDefault(x => x.Id == id);
            if (item != null)
                ClassList.Remove(item);

            return RedirectToPage(new
            {
                searchClassName = Request.Query["searchClassName"],
                minStudents = Request.Query["minStudents"],
                maxStudents = Request.Query["maxStudents"],
                pageIndex = CurrentPage
            });
        }

        public IActionResult OnPostEdit(int id)
        {
            var item = ClassList.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                ClassList.Remove(item);
                NewClass = item;
            }
            return Page();
        }

        public IActionResult OnPostExportJson()
        {
            var json = Utils.Instance.ExportToJson(ClassList);
            return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", "AllClasses.json");
        }

        public IActionResult OnPostExportFilteredJson(List<string> selectedColumns)
        {
            // GET parametrelerini yeniden al
            string searchClassName = Request.Query["searchClassName"];
            int.TryParse(Request.Query["minStudents"], out int minStudents);
            int.TryParse(Request.Query["maxStudents"], out int maxStudents);

            var filteredList = ClassList.AsQueryable();

            if (!string.IsNullOrEmpty(searchClassName))
                filteredList = filteredList.Where(c => c.ClassName.Contains(searchClassName));

            if (Request.Query.ContainsKey("minStudents"))
                filteredList = filteredList.Where(c => c.StudentCount >= minStudents);

            if (Request.Query.ContainsKey("maxStudents"))
                filteredList = filteredList.Where(c => c.StudentCount <= maxStudents);

            var filteredData = filteredList.Select(c => new ClassInformationTable
            {
                Id = c.Id,
                ClassName = c.ClassName,
                StudentCount = c.StudentCount,
                Description = c.Description
            }).ToList();

            // JSON oluştur
            var filteredJsonList = new List<Dictionary<string, object>>();

            foreach (var classItem in filteredData)
            {
                var obj = new Dictionary<string, object>();

                if (selectedColumns.Contains("ClassName"))
                    obj["ClassName"] = classItem.ClassName;

                if (selectedColumns.Contains("StudentCount"))
                    obj["StudentCount"] = classItem.StudentCount;

                if (selectedColumns.Contains("Description"))
                    obj["Description"] = classItem.Description;

                filteredJsonList.Add(obj);
            }

            var json = JsonSerializer.Serialize(filteredJsonList, new JsonSerializerOptions { WriteIndented = true });
            return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", "FilteredClasses.json");
        }
        public IActionResult OnPostExportFilteredJsonOnly()
        {
            // Öncelikle filtreleme parametrelerini al
            string? searchClassName = Request.Query["searchClassName"];
            bool hasMinStudents = int.TryParse(Request.Query["minStudents"], out int minStudents);
            bool hasMaxStudents = int.TryParse(Request.Query["maxStudents"], out int maxStudents);

            // Filtreleme işlemi
            var filteredList = ClassList.AsQueryable();

            if (!string.IsNullOrEmpty(searchClassName))
                filteredList = filteredList.Where(c => c.ClassName.Contains(searchClassName));

            if (hasMinStudents)
                filteredList = filteredList.Where(c => c.StudentCount >= minStudents);

            if (hasMaxStudents)
                filteredList = filteredList.Where(c => c.StudentCount <= maxStudents);

            // **Eğer filtrelenmiş liste boşsa, boş JSON dön**
            var filteredData = filteredList.ToList();
            if (filteredData.Count == 0)
            {
                return File(System.Text.Encoding.UTF8.GetBytes("[]"), "application/json", "FilteredClassesOnly.json");
            }

            // **JSON olarak kaydetme**
            var json = JsonSerializer.Serialize(filteredData, new JsonSerializerOptions { WriteIndented = true });

            return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", "FilteredClassesOnly.json");
        }






    }
}
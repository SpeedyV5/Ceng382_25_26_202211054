using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using ClassManagement.Models;

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

        public IActionResult OnGet(string searchClassName, int? minStudents, int? maxStudents, int pageIndex = 1, int pageSize =10)
        {
            if (ClassList.Count == 0)
            {
                for (int i = 1; i <= 100; i++)
                {
                    ClassList.Add(new ClassInformationModel
                    {
                        ClassName = $"Sample Class {i}",
                        StudentCount = 10 + (i % 50),
                        Description = $"This is sample class number {i}"
                    });
                }
            }
            var filteredList = ClassList.AsQueryable();

            // Filtreleme işlemleri
            if (!string.IsNullOrEmpty(searchClassName))
            {
                filteredList = filteredList.Where(c => c.ClassName.Contains(searchClassName));
            }

            if (minStudents.HasValue)
            {
                filteredList = filteredList.Where(c => c.StudentCount >= minStudents.Value);
            }

            if (maxStudents.HasValue)
            {
                filteredList = filteredList.Where(c => c.StudentCount <= maxStudents.Value);
            }

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
            {
                return Page();
            }

            // Yeni sınıf ekle
            int newId = ClassList.Count > 0 ? ClassList.Max(x => x.Id) + 1 : 1;
            NewClass.Id = newId;

            ClassList.Add(NewClass);

            // Formu sıfırlama ve filtreyi koruma
            NewClass = new ClassInformationModel();
            return RedirectToPage(new
            {
                searchClassName = Request.Query["searchClassName"],
                minStudents = Request.Query["minStudents"],
                maxStudents = Request.Query["maxStudents"],
                pageIndex = CurrentPage // Sayfayı koru
            });
        }

        public IActionResult OnPostDelete(int id)
        {
            var item = ClassList.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                ClassList.Remove(item);
            }

            // Filtreyi koruma
            return RedirectToPage(new
            {
                searchClassName = Request.Query["searchClassName"],
                minStudents = Request.Query["minStudents"],
                maxStudents = Request.Query["maxStudents"],
                pageIndex = CurrentPage // Sayfayı koru
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
    }
}

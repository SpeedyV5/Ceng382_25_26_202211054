using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassManagement.Data;
using ClassManagement.Models;
using ClassManagement.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClassManagement.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly SchoolDbContext _context;

        public IndexModel(SchoolDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Class NewClass { get; set; }

        [BindProperty]
        public int? EditClassId { get; set; }

        public List<Class> FilteredData { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public List<string> SelectedColumns { get; set; }

        public async Task OnGetAsync(
            string searchClassName,
            int? minStudents,
            int? maxStudents,
            int pageIndex = 1,
            int pageSize = 10,
            List<string> selectedColumns = null,
            int? editClassId = null)
        {
            SelectedColumns = selectedColumns ?? new List<string>();

            // Sadece aktif kayıtlar
            var query = _context.Classes.Where(c => c.IsActive);

            if (!string.IsNullOrEmpty(searchClassName))
                query = query.Where(c => c.Name.Contains(searchClassName));

            if (minStudents.HasValue)
                query = query.Where(c => c.PersonCount >= minStudents.Value);

            if (maxStudents.HasValue)
                query = query.Where(c => c.PersonCount <= maxStudents.Value);

            var totalRecords = await query.CountAsync();

            // OrderBy ekleyerek tahmin edilebilir sıralama
            FilteredData = await query
                .OrderBy(c => c.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            CurrentPage = pageIndex;
            TotalPages = (int)System.Math.Ceiling(totalRecords / (double)pageSize);

            // Düzenleme moduna girmek için
            if (editClassId.HasValue)
            {
                var editItem = await _context.Classes.FindAsync(editClassId.Value);
                if (editItem != null)
                {
                    NewClass = new Class
                    {
                        Name = editItem.Name,
                        PersonCount = editItem.PersonCount,
                        Description = editItem.Description
                    };
                    EditClassId = editItem.Id;
                }
            }
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            NewClass.IsActive = true;
            _context.Classes.Add(NewClass);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var item = await _context.Classes.FindAsync(id);
            if (item != null)
            {
                item.IsActive = false;
                _context.Classes.Update(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync()
        {
            if (!ModelState.IsValid || EditClassId == null)
                return RedirectToPage();

            var item = await _context.Classes.FindAsync(EditClassId.Value);
            if (item != null)
            {
                item.Name = NewClass.Name;
                item.PersonCount = NewClass.PersonCount;
                item.Description = NewClass.Description ?? string.Empty;
                item.IsActive = true;

                _context.Classes.Update(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostExportJsonAsync()
        {
            var allClasses = await _context.Classes.ToListAsync();
            var json = Utils.Instance.ExportToJson(allClasses);
            return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", "AllClasses.json");
        }

        public async Task<IActionResult> OnPostExportFilteredJsonAsync(List<string> selectedColumns)
        {
            var allClasses = await _context.Classes.ToListAsync();
            var json = Utils.Instance.ExportToJson(allClasses, selectedColumns);
            return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", "FilteredClasses.json");
        }
    }
}

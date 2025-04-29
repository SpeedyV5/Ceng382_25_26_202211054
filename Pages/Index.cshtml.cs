using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassManagement.Data;
using ClassManagement.Models;
using ClassManagement.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace ClassManagement.Pages
{
    public class IndexModel : PageModel
    {
        private readonly SchoolDbContext _context;

        public IndexModel(SchoolDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Class NewClass { get; set; } = new Class();

        [BindProperty]
        public int? EditClassId { get; set; }

        public List<Class> FilteredData { get; set; } = new List<Class>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public List<string> SelectedColumns { get; set; } = new List<string>();

        private bool IsUserLoggedIn()
        {
            var sessionToken = HttpContext.Session.GetString("token");
            var sessionUser = HttpContext.Session.GetString("username");
            var sessionId = HttpContext.Session.GetString("session_id");

            var cookieToken = Request.Cookies["token"];
            var cookieUser = Request.Cookies["username"];
            var cookieSessionId = Request.Cookies["session_id"];

            return sessionToken != null && cookieToken != null &&
                   sessionUser != null && cookieUser != null &&
                   sessionId != null && cookieSessionId != null &&
                   sessionToken == cookieToken &&
                   sessionUser == cookieUser &&
                   sessionId == cookieSessionId;
        }

        public async Task<IActionResult> OnGetAsync(string searchClassName, int? minStudents, int? maxStudents, int pageIndex = 1, int pageSize = 10, List<string> selectedColumns = null, int? editClassId = null)
        {
            if (!IsUserLoggedIn())
                return RedirectToPage("/Login");

            SelectedColumns = selectedColumns ?? new List<string>();

            var query = _context.Classes.Where(c => c.IsActive);

            if (!string.IsNullOrEmpty(searchClassName))
                query = query.Where(c => c.Name.Contains(searchClassName));

            if (minStudents.HasValue)
                query = query.Where(c => c.PersonCount >= minStudents.Value);

            if (maxStudents.HasValue)
                query = query.Where(c => c.PersonCount <= maxStudents.Value);

            var totalRecords = await query.CountAsync();

            FilteredData = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            CurrentPage = pageIndex;
            TotalPages = (int)System.Math.Ceiling(totalRecords / (double)pageSize);

            // Eğer EditClassId varsa, formu doldur
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

            return Page();
        }

        public async Task<IActionResult> OnPostAddAsync()
        {
            if (!IsUserLoggedIn())
                return RedirectToPage("/Login");

            if (!ModelState.IsValid)
                return Page();

            NewClass.IsActive = true;
            _context.Classes.Add(NewClass);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!IsUserLoggedIn())
                return RedirectToPage("/Login");

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
    if (!IsUserLoggedIn())
        return RedirectToPage("/Login");

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
            if (!IsUserLoggedIn())
                return RedirectToPage("/Login");

            var allClasses = await _context.Classes.ToListAsync();
            var json = Utils.Instance.ExportToJson(allClasses);
            return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", "AllClasses.json");
        }

        public async Task<IActionResult> OnPostExportFilteredJsonAsync(List<string> selectedColumns)
        {
            if (!IsUserLoggedIn())
                return RedirectToPage("/Login");

            var allClasses = await _context.Classes.ToListAsync();
            var json = Utils.Instance.ExportToJson(allClasses, selectedColumns);
            return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", "FilteredClasses.json");
        }
    }
}

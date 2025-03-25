using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using ClassManagement.Models; // Model sınıfını içeri dahil et

namespace ClassManagement.Pages
{
    public class IndexModel : PageModel
    {
        // ✅ ClassList'i statik yaparak tüm isteklerde verinin korunmasını sağlıyoruz
        public static List<ClassInformationModel> ClassList { get; set; } = new List<ClassInformationModel>();

        [BindProperty]
        public ClassInformationModel NewClass { get; set; } = new ClassInformationModel(); // Varsayılan bir nesne başlatıldı

        public void OnGet() { }
        public IActionResult OnPostAdd()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Assign a new ID based on the highest existing ID in the list
            int newId = ClassList.Count > 0 ? ClassList.Max(x => x.Id) + 1 : 1;
            NewClass.Id = newId;

            // Add the new class to the list
            ClassList.Add(NewClass);

            // Reset the form
            NewClass = new ClassInformationModel();

            return RedirectToPage();
        }


        public IActionResult OnPostDelete(int id)
        {
            var item = ClassList.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                    ClassList.Remove(item);
                }
            return RedirectToPage();
        }

        public IActionResult OnPostEdit(int id)
        {
            var item = ClassList.FirstOrDefault(x => x.Id == id);
            if (item != null)
            {
                NewClass = item;
            }
            return Page();
        }
    }
}
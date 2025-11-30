using Microsoft.AspNetCore.Mvc.Rendering;
using KCRM.Models;
using System.Collections.Generic;

namespace KCRM.ViewModels
{
    public class TaskAddViewModel
    {
        public TaskItem Task { get; set; } = new TaskItem();

        // Dropdown Listeleri
        public IEnumerable<SelectListItem>? CustomerList { get; set; }
        public IEnumerable<SelectListItem>? LeadList { get; set; }

        // Kullanıcının ne seçtiğini anlamak için (View'de kullanılacak)
        // "Customer", "Lead" veya "None"
        public string RelatedType { get; set; } = "None";
    }
}
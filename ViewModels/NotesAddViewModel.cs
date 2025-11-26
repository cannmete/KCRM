using Microsoft.AspNetCore.Mvc.Rendering;
using KCRM.Models;
using System.Collections.Generic;

namespace KCRM.ViewModels
{
    public class NotesAddViewModel
    {
        // 1. Notun kendisi (form verileri)
        public Notes Note { get; set; } = new Notes();

        // 2. Müşteri Seçimi için Liste
        public IEnumerable<SelectListItem> CustomerList { get; set; }
    }
}
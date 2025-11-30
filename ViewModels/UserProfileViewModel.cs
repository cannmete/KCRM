using KCRM.Models;
using System.Collections.Generic;

namespace KCRM.ViewModels
{
    public class UserProfileViewModel
    {
        public User User { get; set; } // Kullanıcı bilgileri (Ad, Email vb.)
        public string Role { get; set; } // Rolü (Admin/User)

        // İstatistikler
        public int TotalNotesCount { get; set; }
        public int TotalTasksCount { get; set; }
        public int PendingTasksCount { get; set; } // Bekleyen görevler (Bonus)

        // (İsteğe Bağlı) Son Eklediği Görevler Listesi
        public List<TaskItem> RecentTasks { get; set; } = new List<TaskItem>();
    }
}
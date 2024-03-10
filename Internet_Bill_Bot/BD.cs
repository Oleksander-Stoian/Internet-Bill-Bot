using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace Internet_Bill_Bot
{
    

    public class ApplicationDbContext : DbContext
    {
        public DbSet<Application> Applications { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("server=localhost;database=telegram_bot_bd;user=root;password=Qaz186186",
                new MySqlServerVersion(new Version(8, 0, 36))); // Вказуйте актуальну версію MySQL сервера
        }
    }
    public class Application
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public int ApartmentNumber { get; set; }
        public string Complaint { get; set; }
        public DateTime Date { get; set; } // Час подання заявки буде встановлено автоматично
        public string FirstName { get; set; } // Ім'я
        public string LastName { get; set; } // Прізвище
        public string Patronymic { get; set; } // По батькові
    }

    // Структура для збереження персональних даних користувача
    class PersonalInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Patronymic { get; set; }
    }

}

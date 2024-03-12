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
        public int DocumentNumber { get; set; } // Переконайтеся, що це поле відображається на відповідну колонку в БД
        public int ApartmentNumber { get; set; }
        public string Complaint { get; set; }
        public DateTime Date { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Patronymic { get; set; }
    }

    // Структура для збереження персональних даних користувача
    class PersonalInfo
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Patronymic { get; private set; }

        public PersonalInfo(string firstName, string lastName, string patronymic)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(patronymic))
            {
                throw new ArgumentException("Персональні дані не можуть бути пустими.");
            }

            FirstName = firstName;
            LastName = lastName;
            Patronymic = patronymic;
        }

        public string GetFullName()
        {
            return $"{FirstName} {LastName} {Patronymic}";
        }
    }

}

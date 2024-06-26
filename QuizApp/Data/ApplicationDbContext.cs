using Microsoft.EntityFrameworkCore;
using QuizApp.Models;
using System.Collections.Generic;

namespace QuizApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
    }
}

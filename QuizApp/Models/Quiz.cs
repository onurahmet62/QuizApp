using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models
{
    public class Quiz
    {
       
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public List<Question> Questions { get; set; } = new List<Question>();
    }
}

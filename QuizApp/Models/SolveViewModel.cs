using QuizApp.Models;

namespace QuizApp.ViewModels
{
    public class SolveQuizViewModel
    {
        public Quiz Quiz { get; set; }
        public Question Question { get; set; }
        public int QuestionIndex { get; set; }
        public int CorrectCount { get; set; }
        public string SelectedOption { get; set; }
    }

    public class FinishQuizViewModel
    {
        public Quiz Quiz { get; set; }
        public double Score { get; set; }
    }
}

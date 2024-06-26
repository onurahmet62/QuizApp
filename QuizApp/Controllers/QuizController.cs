using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizApp.Data;
using QuizApp.Models;
using QuizApp.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuizApp.Controllers
{
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuizController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Quizzes.ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = new QuizViewModel();
            viewModel.Questions.Add(new QuestionViewModel()); // Change to QuestionViewModel
            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Questions")] QuizViewModel quizViewModel)
        {
            if (ModelState.IsValid)
            {
                var quiz = new Quiz
                {
                    Title = quizViewModel.Title,
                    Questions = quizViewModel.Questions.Select(q => new Question
                    {
                        Text = q.Text,
                        OptionA = q.OptionA,
                        OptionB = q.OptionB,
                        OptionC = q.OptionC,
                        OptionD = q.OptionD,
                        CorrectOption = q.CorrectOption
                    }).ToList()
                };

                foreach (var question in quiz.Questions)
                {
                    question.Quiz = quiz; // Ensure the Quiz reference is set
                }

                try
                {
                    _context.Add(quiz);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while saving the quiz.");
                    return View(quizViewModel);
                }
            }
            return View(quizViewModel);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quizzes.Include(q => q.Questions).FirstOrDefaultAsync(m => m.Id == id);
            if (quiz == null)
            {
                return NotFound();
            }

            var quizViewModel = new QuizViewModel
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Questions = quiz.Questions.Select(q => new QuestionViewModel
                {
                    Id = q.Id,
                    Text = q.Text,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD,
                    CorrectOption = q.CorrectOption
                }).ToList()
            };

            return View(quizViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Questions")] QuizViewModel quizViewModel)
        {
            if (ModelState.IsValid)
            {
                var quiz = await _context.Quizzes.Include(q => q.Questions).FirstOrDefaultAsync(q => q.Id == quizViewModel.Id);
                if (quiz == null)
                {
                    return NotFound();
                }

                quiz.Title = quizViewModel.Title;
                quiz.Questions = quizViewModel.Questions.Select(q => new Question
                {
                    Text = q.Text,
                    OptionA = q.OptionA,
                    OptionB = q.OptionB,
                    OptionC = q.OptionC,
                    OptionD = q.OptionD,
                    CorrectOption = q.CorrectOption
                }).ToList();

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuizExists(quizViewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(quizViewModel);
        }

        private bool QuizExists(int id)
        {
            return _context.Quizzes.Any(e => e.Id == id);
        }





        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quizzes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        // Solve action (GET)
        public async Task<IActionResult> Solve(int id, int questionIndex = 0, int correctCount = 0)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                return NotFound();
            }

            if (questionIndex >= quiz.Questions.Count || questionIndex < 0)
            {
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new SolveQuizViewModel
            {
                Quiz = quiz,
                QuestionIndex = questionIndex,
                Question = quiz.Questions[questionIndex],
                CorrectCount = correctCount
            };

            return View(viewModel);
        }

        // Solve action (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Solve(SolveQuizViewModel model, string action)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == model.Quiz.Id);

            if (quiz == null)
            {
                return NotFound();
            }

            var question = quiz.Questions[model.QuestionIndex];

            // Case-insensitive comparison
            if (string.Equals(model.SelectedOption, question.CorrectOption, StringComparison.OrdinalIgnoreCase))
            {
                model.CorrectCount++;
            }

            switch (action)
            {
                case "Next":
                    model.QuestionIndex++;
                    break;
                case "Previous":
                    model.QuestionIndex--;
                    break;
                case "Finish":
                    return RedirectToAction(nameof(Finish), new { id = model.Quiz.Id, correctCount = model.CorrectCount });
            }

            return RedirectToAction(nameof(Solve), new { id = model.Quiz.Id, questionIndex = model.QuestionIndex, correctCount = model.CorrectCount });
        }

        // Finish action
        public async Task<IActionResult> Finish(int id, int correctCount)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quiz == null)
            {
                return NotFound();
            }

            var totalQuestions = quiz.Questions.Count;
            var score = Math.Round((100.0 / totalQuestions) * correctCount);

            var viewModel = new FinishQuizViewModel
            {
                Quiz = quiz,
                Score = score
            };

            return View(viewModel);
        }

    }
}

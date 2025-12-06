using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;
using BilBakalimOnline.Services;
using BilBakalimOnline.Models;

namespace BilBakalimOnline.Controllers
{
    public class QuizController : Controller
    {
        // İlk sayfa (soru ekranı)
        public IActionResult Index()
        {
            var question = QuizState.GetCurrentQuestion();

            // Session’dan oyuncu adını al
            var playerName = HttpContext.Session.GetString("PlayerName");

            int currentScore = 0;

            if (!string.IsNullOrWhiteSpace(playerName) &&
                QuizState.Scores.TryGetValue(playerName, out var score))
            {
                currentScore = score;
            }

            ViewBag.PlayerName = playerName;
            ViewBag.Score = currentScore;

            return View(question);
        }

        // Cevap gönderildiğinde
        [HttpPost]
        public IActionResult Answer(string? playerName, string selectedOption)
        {
            // İsim boşsa session’dan çek
            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = HttpContext.Session.GetString("PlayerName");
            }
            else
            {
                // İlk defa isim giriliyorsa session’a kaydet
                HttpContext.Session.SetString("PlayerName", playerName);
            }

            var question = QuizState.GetCurrentQuestion();
            if (question == null)
                return Content("Soru bulunamadı.");

            bool correct = false;

            if (!string.IsNullOrWhiteSpace(selectedOption))
            {
                char opt = char.ToUpper(selectedOption[0]);
                correct = opt == question.CorrectOption;

                if (!string.IsNullOrWhiteSpace(playerName))
                {
                    if (!QuizState.Scores.ContainsKey(playerName))
                        QuizState.Scores[playerName] = 0;

                    if (correct)
                        QuizState.Scores[playerName] += 10;
                }
            }

            ViewBag.PlayerName = playerName;
            ViewBag.Correct = correct;
            ViewBag.CorrectOption = question.CorrectOption;
            ViewBag.QuestionText = question.Text;

            var scores = QuizState.Scores
                .OrderByDescending(x => x.Value)
                .ToList();

            ViewBag.Scores = scores;

            return View("Result");
        }

        // Sonraki soruya geç
        [HttpPost]
        public IActionResult NextQuestion()
        {
            // Eğer şu an SON sorudaysak Final ekranına git
            if (QuizState.CurrentIndex >= QuizState.Questions.Count - 1)
            {
                return RedirectToAction("Final");
            }

            // Değilsek bir sonraki soruya geç
            QuizState.NextQuestion();
            return RedirectToAction("Index");
        }


        // Yarışı baştan başlat (soruları en başa sar)
        public IActionResult Restart()
        {
            QuizState.CurrentIndex = 0;
            // İstersen skorları da sıfırlayabilirsin:
            // QuizState.Scores.Clear();
            return RedirectToAction("Index");
        }

        // Final ekranı
        public IActionResult Final()
        {
            // Oyuncu adı
            var playerName = HttpContext.Session.GetString("PlayerName");

            int score = 0;

            if (!string.IsNullOrWhiteSpace(playerName) &&
                QuizState.Scores.TryGetValue(playerName, out var s))
            {
                score = s;
            }

            int totalQuestions = QuizState.Questions.Count;

            // Her doğru 10 puansa:
            int correctCount = score / 10;
            if (correctCount > totalQuestions) correctCount = totalQuestions;

            int wrongCount = totalQuestions - correctCount;

            // Basit yorum
            string comment;
            if (score >= totalQuestions * 10 * 0.8)
                comment = "Efsanesin! Gerçek bir Bil Bakalım ustası! 🎉";
            else if (score >= totalQuestions * 10 * 0.5)
                comment = "Gayet iyi! Biraz daha deneme ile şampiyon olursun. 💪";
            else
                comment = "Fena değil, 1 tur daha atıp intikamını alırsın. 😉";

            // Leaderboard
            var rankList = QuizState.Scores
                .OrderByDescending(x => x.Value)
                .ToList();

            ViewBag.PlayerName = playerName;
            ViewBag.Score = score;
            ViewBag.TotalQuestions = totalQuestions;
            ViewBag.CorrectCount = correctCount;
            ViewBag.WrongCount = wrongCount;
            ViewBag.Comment = comment;
            ViewBag.RankList = rankList;

            return View();
        }
    }
}

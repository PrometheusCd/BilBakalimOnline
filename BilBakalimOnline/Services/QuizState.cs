using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BilBakalimOnline.Models;

namespace BilBakalimOnline.Services
{
    public static class QuizState
    {
        // Sorular (JSON'dan yüklenecek)
        public static List<Question> Questions { get; private set; } = LoadQuestions();

        // Şu anki soru index'i
        public static int CurrentIndex { get; set; } = 0;

        // Oyuncu adı → Puan
        public static Dictionary<string, int> Scores { get; } = new Dictionary<string, int>();

        // Aktif soruyu getir
        public static Question? GetCurrentQuestion()
        {
            if (!Questions.Any()) return null;

            if (CurrentIndex < 0 || CurrentIndex >= Questions.Count)
                CurrentIndex = 0;

            return Questions[CurrentIndex];
        }

        // Bir sonraki soruya geç (son soruda sabit kal)
        public static void NextQuestion()
        {
            if (CurrentIndex < Questions.Count - 1)
            {
                CurrentIndex++;
            }
        }

        // JSON'dan soruları yükle
        private static List<Question> LoadQuestions()
        {
            try
            {
                var baseDir = Directory.GetCurrentDirectory();
                var path = Path.Combine(baseDir, "Data", "questions.json");

                if (!File.Exists(path))
                {
                    // Dosya yoksa default sorular
                    return GetDefaultQuestions();
                }

                var json = File.ReadAllText(path);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var list = JsonSerializer.Deserialize<List<Question>>(json, options);

                if (list == null || list.Count == 0)
                    return GetDefaultQuestions();

                return list;
            }
            catch
            {
                // Herhangi bir hata olursa default sorulara dön
                return GetDefaultQuestions();
            }
        }

        // JSON okunamazsa kullanılacak yedek sorular
        private static List<Question> GetDefaultQuestions()
        {
            return new List<Question>()
            {
                new Question
                {
                    Id = 1,
                    Text = "Türkiye'nin başkenti neresidir?",
                    OptionA = "İstanbul",
                    OptionB = "Ankara",
                    OptionC = "İzmir",
                    OptionD = "Bursa",
                    CorrectOption = 'B'
                },
                new Question
                {
                    Id = 2,
                    Text = "Aşağıdakilerden hangisi bir programlama dilidir?",
                    OptionA = "HTML",
                    OptionB = "CSS",
                    OptionC = "C#",
                    OptionD = "Photoshop",
                    CorrectOption = 'C'
                },
                new Question
                {
                    Id = 3,
                    Text = "TRT'nin açılımı nedir?",
                    OptionA = "Türkiye Radyo Televizyon Kurumu",
                    OptionB = "Türkiye Radyo Teşkilatı",
                    OptionC = "Toplu Radyo Televizyon",
                    OptionD = "Türkiye Radyo Topluluğu",
                    CorrectOption = 'A'
                }
            };
        }
    }
}

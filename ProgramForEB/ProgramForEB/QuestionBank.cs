using ProgramForEB.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;


namespace ProgramForEB
{
    public static class QuestionBank
    {
        /// <summary>Путь к папке с файлами вопросов.</summary>
        private static readonly string QuestionsFolder =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Questions");

        /// <summary>Загрузить все вопросы из файла для указанной группы.</summary>
        public static List<Question> LoadQuestions(int group)
        {
            string fileName = $"Вопросы_{group}_группа.json";
            string filePath = Path.Combine(QuestionsFolder, fileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    $"Файл с вопросами для {group} группы не найден:\n{filePath}\n\n" +
                    "Убедитесь, что файл находится в папке Questions рядом с программой.");
            }

            string json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };

            var questions = JsonSerializer.Deserialize<List<Question>>(json, options);

            if (questions == null || questions.Count == 0)
            {
                throw new InvalidDataException(
                    $"Файл для {group} группы пуст или имеет неверный формат.");
            }

            // Проверяем, что в каждом вопросе ровно один правильный ответ
            foreach (var q in questions)
            {
                if (q.Options == null || q.Options.Length < 2)
                    throw new InvalidDataException(
                        $"Вопрос {q.Id} не содержит достаточного количества вариантов ответа.");

                if (q.CorrectIndex < 0 || q.CorrectIndex >= q.Options.Length)
                    throw new InvalidDataException(
                        $"Вопрос {q.Id}: индекс правильного ответа выходит за пределы массива.");
            }

            return questions;
        }

        /// <summary>Получить N случайных вопросов для указанной группы.</summary>
        public static List<Question> GetRandomQuestions(int group, int count = 20)
        {
            var all = LoadQuestions(group);
            return all
                .OrderBy(_ => Guid.NewGuid())
                .Take(Math.Min(count, all.Count))
                .ToList();
        }

        /// <summary>Количество вопросов в файле для группы.</summary>
        public static int GetCount(int group)
        {
            try
            {
                return LoadQuestions(group).Count;
            }
            catch
            {
                return 0;
            }
        }
    }
}

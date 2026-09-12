using ProgramForEB.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProgramForEB
{
    public static class ResultStorage
    {
        private static readonly string FilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "results.json");

        public static List<TestResult> Load()
        {
            if (!File.Exists(FilePath)) return new List<TestResult>();
            string json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<TestResult>>(json)
                   ?? new List<TestResult>();
        }

        public static void Save(List<TestResult> results)
        {
            string json = JsonSerializer.Serialize(results,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }

        public static void Add(TestResult result)
        {
            var list = Load();
            list.Add(result);
            Save(list);
        }

        public static void Clear() => Save(new List<TestResult>());
    }
}

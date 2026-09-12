using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramForEB.Models
{
    public class TestResult
    {
        public DateTime Date { get; set; }
        public string FullName { get; set; }
        public string Department { get; set; }
        public int Group { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int Errors { get; set; }
        public double ScorePercent { get; set; }
        public string Grade { get; set; }       // "Сдал" / "Не сдал"
        public string Mark { get; set; }        // оценка
    }
}

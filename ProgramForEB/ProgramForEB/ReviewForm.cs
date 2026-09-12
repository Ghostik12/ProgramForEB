using ProgramForEB.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace ProgramForEB
{
    public class ReviewForm : Form
    {
        private TableLayoutPanel mainLayout;
        private FlowLayoutPanel scrollPanel;

        public ReviewForm(List<Question> questions,
                          int[] userAnswers,
                          TestResult result)
        {
            Text = "Результаты теста — разбор ошибок";
            Size = new Size(900, 750);
            MinimumSize = new Size(600, 500);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 10);

            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(12),
                BackColor = Color.WhiteSmoke
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));   // шапка
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // список
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));   // кнопка

            // --- Шапка ---
            var header = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = result.Grade == "Сдал" ? Color.DarkGreen : Color.DarkRed,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4),
                Text = $"ФИО: {result.FullName}    Группа: {result.Group}\n" +
                       $"Вопросов: {result.TotalQuestions}   " +
                       $"Правильных: {result.CorrectAnswers}   " +
                       $"Ошибок: {result.Errors}   " +
                       $"Результат: {result.ScorePercent:F1}%   " +
                       $"Оценка: {result.Mark}   " +
                       $"Итог: {result.Grade}"
            };
            mainLayout.Controls.Add(header, 0, 0);

            // --- Панель со списком ошибок ---
            scrollPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(6),
                BackColor = Color.White
            };
            scrollPanel.Resize += (s, e) => UpdateBlockWidths();
            mainLayout.Controls.Add(scrollPanel, 0, 1);

            // --- Кнопка закрытия ---
            var btnClose = new Button
            {
                Text = "Закрыть",
                Dock = DockStyle.Right,
                Width = 180,
                Margin = new Padding(4),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };
            btnClose.Click += (s, e) => Close();

            var footer = new Panel { Dock = DockStyle.Fill };
            btnClose.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnClose.Location = new Point(footer.Width - 184, 8);
            footer.Resize += (s, e) => btnClose.Location = new Point(footer.Width - 184, 8);
            footer.Controls.Add(btnClose);
            mainLayout.Controls.Add(footer, 0, 2);

            Controls.Add(mainLayout);

            // Заполняем список ошибок
            BuildErrorList(questions, userAnswers);

            Shown += (s, e) => UpdateBlockWidths();
        }

        private List<ErrorBlock> _blocks = new List<ErrorBlock>();

        private void BuildErrorList(List<Question> questions, int[] userAnswers)
        {
            scrollPanel.SuspendLayout();
            scrollPanel.Controls.Clear();
            _blocks.Clear();

            int errorNumber = 0;

            for (int i = 0; i < questions.Count; i++)
            {
                var q = questions[i];
                int userIdx = userAnswers[i];

                // Пропускаем правильные ответы
                if (userIdx == q.CorrectIndex) continue;

                errorNumber++;

                string userAnswerText = (userIdx >= 0 && userIdx < q.Options.Length)
                    ? q.Options[userIdx]
                    : "— нет ответа —";

                string correctAnswerText = q.Options[q.CorrectIndex];

                var block = new ErrorBlock(
                    number: errorNumber,
                    questionText: q.Text,
                    userAnswer: userAnswerText,
                    correctAnswer: correctAnswerText);

                scrollPanel.Controls.Add(block);
                _blocks.Add(block);
            }

            if (_blocks.Count == 0)
            {
                var lbl = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.DarkGreen,
                    Text = "Все ответы верны! Ошибок нет.",
                    Padding = new Padding(8)
                };
                scrollPanel.Controls.Add(lbl);
            }

            scrollPanel.ResumeLayout();
        }

        private void UpdateBlockWidths()
        {
            if (scrollPanel == null || _blocks.Count == 0) return;

            int w = scrollPanel.ClientSize.Width
                    - scrollPanel.Padding.Horizontal
                    - SystemInformation.VerticalScrollBarWidth - 8;
            if (w < 200) return;

            scrollPanel.SuspendLayout();
            foreach (var b in _blocks)
                b.LayoutForWidth(w);
            scrollPanel.ResumeLayout();
        }
    }

    /// <summary>Блок разбора одного неправильного ответа.</summary>
    public class ErrorBlock : Panel
    {
        public ErrorBlock(int number, string questionText, string userAnswer, string correctAnswer)
        {
            Margin = new Padding(0, 0, 0, 12);
            Padding = new Padding(10);
            BorderStyle = BorderStyle.FixedSingle;
            BackColor = Color.White;
            AutoSize = true;

            int width = 600;
            int top = 0;

            // Заголовок с номером
            var lblHeader = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DimGray,
                Location = new Point(0, top),
                Text = $"Вопрос #{number}"
            };
            Controls.Add(lblHeader);
            top += 24;

            // Текст вопроса
            var lblQuestion = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(0, top),
                MaximumSize = new Size(width, 0),
                Text = questionText
            };
            Controls.Add(lblQuestion);
            top += lblQuestion.PreferredHeight + 12;

            // Ответ пользователя
            var lblUserCaption = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                Location = new Point(0, top),
                Text = "Ваш ответ:"
            };
            Controls.Add(lblUserCaption);
            top += lblUserCaption.PreferredHeight + 2;

            var lblUser = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DarkRed,
                Location = new Point(12, top),
                MaximumSize = new Size(width - 12, 0),
                Text = userAnswer
            };
            Controls.Add(lblUser);
            top += lblUser.PreferredHeight + 12;

            // Правильный ответ
            var lblCorrectCaption = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                Location = new Point(0, top),
                Text = "Правильный ответ:"
            };
            Controls.Add(lblCorrectCaption);
            top += lblCorrectCaption.PreferredHeight + 2;

            var lblCorrect = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DarkGreen,
                Location = new Point(12, top),
                MaximumSize = new Size(width - 12, 0),
                Text = correctAnswer
            };
            Controls.Add(lblCorrect);
            top += lblCorrect.PreferredHeight + 6;

            Height = top + Padding.Vertical;

            _labels = new[] { lblQuestion, lblUser, lblCorrect };
        }

        private Label[] _labels;

        /// <summary>Пересчитать ширину под контейнер.</summary>
        public void LayoutForWidth(int width)
        {
            if (width < 200) width = 200;
            int inner = width - Padding.Horizontal - 10;
            if (inner < 100) inner = 100;

            foreach (var lbl in _labels)
            {
                lbl.MaximumSize = new Size(
                    lbl.Left > 0 ? inner - lbl.Left : inner,
                    0);
            }

            // пересчёт высоты блока
            int bottom = 0;
            foreach (Control c in Controls)
                bottom = Math.Max(bottom, c.Bottom);
            Height = bottom + Padding.Bottom + 4;
        }
    }
}

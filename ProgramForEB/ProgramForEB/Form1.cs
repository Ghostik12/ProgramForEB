using ProgramForEB.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProgramForEB
{
    public partial class Form1 : Form
    {
        private ComboBox cmbGroup;
        private TextBox txtLastName, txtFirstName, txtMiddleName, txtDepartment;
        private Button btnStart;

        public Form1()
        {
            Text = "Электробезопасность — начало тестирования";
            Size = new Size(600, 450);
            MinimumSize = new Size(480, 380);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(20)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            for (int i = 0; i < 5; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            layout.Controls.Add(new Label { Text = "Группа допуска:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            cmbGroup = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbGroup.Items.AddRange(new object[] { "2 группа", "3 группа", "4 группа" });
            cmbGroup.SelectedIndex = 0;
            layout.Controls.Add(cmbGroup, 1, 0);

            layout.Controls.Add(new Label { Text = "Фамилия:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            txtLastName = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtLastName, 1, 1);

            layout.Controls.Add(new Label { Text = "Имя:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
            txtFirstName = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtFirstName, 1, 2);

            layout.Controls.Add(new Label { Text = "Отчество:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
            txtMiddleName = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtMiddleName, 1, 3);

            layout.Controls.Add(new Label { Text = "Подразделение:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 4);
            txtDepartment = new TextBox { Dock = DockStyle.Fill };
            layout.Controls.Add(txtDepartment, 1, 4);

            btnStart = new Button
            {
                Text = "Начать тест",
                Dock = DockStyle.Bottom,
                Height = 45,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = Color.LightGreen,
                Margin = new Padding(0, 15, 0, 0)
            };
            btnStart.Click += BtnStart_Click;
            layout.Controls.Add(btnStart, 0, 5);
            layout.SetColumnSpan(btnStart, 2);

            Controls.Add(layout);
        }


        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtMiddleName.Text) ||
                string.IsNullOrWhiteSpace(txtDepartment.Text))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int group = cmbGroup.SelectedIndex + 2; // 2, 3, 4

            if (QuestionBank.GetCount(group) < 10)
            {
                MessageBox.Show($"В банке недостаточно вопросов для {group} группы. " +
                                $"Добавьте минимум 10 вопросов.", "Ошибка");
                return;
            }

            string fullName = $"{txtLastName.Text.Trim()} {txtFirstName.Text.Trim()} " +
                              $"{txtMiddleName.Text.Trim()}";

            var testForm = new TestForm(group, fullName, txtDepartment.Text.Trim());
            Hide();
            testForm.ShowDialog();
            Show();
        }

        public partial class TestForm : Form
        {
            private const int TestDurationSeconds = 600;
            private const int QuestionCount = 10;

            private List<Question> questions;
            private int[] selectedAnswers;
            private int currentIndex = 0;
            private int secondsLeft;
            private Timer timer;
            private int group;
            private string fullName, department;

            private TableLayoutPanel mainLayout;
            private Label lblTimer, lblProgress, lblQuestion;
            private FlowLayoutPanel answersPanel;
            private List<AnswerOption> currentOptions = new List<AnswerOption>();
            private Button btnPrev, btnNext, btnFinish;

            public TestForm(int group, string fullName, string department)
            {
                this.group = group;
                this.fullName = fullName;
                this.department = department;

                var all = QuestionBank.LoadQuestions(group);
                questions = all.OrderBy(_ => Guid.NewGuid())
                               .Take(Math.Min(QuestionCount, all.Count))
                               .ToList();

                if (questions.Count == 0)
                {
                    MessageBox.Show("Нет вопросов для выбранной группы.", "Ошибка");
                    Close();
                    return;
                }

                selectedAnswers = Enumerable.Repeat(-1, questions.Count).ToArray();
                secondsLeft = TestDurationSeconds;

                SetupUI();

                Shown += (s, e) => RelayoutForResize();
                Resize += (s, e) => RelayoutForResize();
                answersPanel.Resize += (s, e) => UpdateRadioWidths();

                timer = new Timer { Interval = 1000 };
                timer.Tick += Timer_Tick;
                timer.Start();

                ShowQuestion(0);
            }

            private void SetupUI()
            {
                Text = $"Тест по электробезопасности — {group} группа";
                Size = new Size(900, 700);
                MinimumSize = new Size(500, 400);
                StartPosition = FormStartPosition.CenterScreen;
                Font = new Font("Segoe UI", 10);

                mainLayout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 1,
                    RowCount = 4,
                    Padding = new Padding(12),
                    BackColor = Color.WhiteSmoke
                };
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
                mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

                // --- Шапка ---
                var headerPanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 1
                };
                headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
                headerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

                lblTimer = new Label
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 13, FontStyle.Bold),
                    ForeColor = Color.DarkRed,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                lblProgress = new Label
                {
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleRight,
                    Font = new Font("Segoe UI", 10, FontStyle.Italic),
                    ForeColor = Color.DimGray
                };
                headerPanel.Controls.Add(lblTimer, 0, 0);
                headerPanel.Controls.Add(lblProgress, 1, 0);
                mainLayout.Controls.Add(headerPanel, 0, 0);

                // --- Текст вопроса ---
                lblQuestion = new Label
                {
                    AutoSize = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    Padding = new Padding(4, 8, 4, 12),
                    Margin = new Padding(0),
                    Text = ""
                };
                mainLayout.Controls.Add(lblQuestion, 0, 1);

                // --- Варианты ответов ---
                answersPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    AutoScroll = true,
                    Padding = new Padding(6),
                    BackColor = Color.White
                };
                mainLayout.Controls.Add(answersPanel, 0, 2);

                // --- Кнопки ---
                var buttonsPanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 3,
                    RowCount = 1
                };
                buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
                buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
                buttonsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));

                btnPrev = new Button { Text = "◀ Назад", Dock = DockStyle.Fill, Margin = new Padding(4) };
                btnNext = new Button { Text = "Вперёд ▶", Dock = DockStyle.Fill, Margin = new Padding(4) };
                btnFinish = new Button
                {
                    Text = "Завершить тест",
                    Dock = DockStyle.Fill,
                    Margin = new Padding(4),
                    BackColor = Color.LightCoral
                };
                buttonsPanel.Controls.Add(btnPrev, 0, 0);
                buttonsPanel.Controls.Add(btnNext, 1, 0);
                buttonsPanel.Controls.Add(btnFinish, 2, 0);
                mainLayout.Controls.Add(buttonsPanel, 0, 3);

                Controls.Add(mainLayout);

                btnPrev.Click += (s, e) => { if (currentIndex > 0) ShowQuestion(currentIndex - 1); };
                btnNext.Click += (s, e) => { if (currentIndex < questions.Count - 1) ShowQuestion(currentIndex + 1); };
                btnFinish.Click += (s, e) => FinishTest();
            }

            private void RelayoutForResize()
            {
                if (lblQuestion == null || mainLayout == null) return;

                int qWidth = mainLayout.ClientSize.Width - mainLayout.Padding.Horizontal - 20;
                if (qWidth > 80)
                {
                    lblQuestion.MaximumSize = new Size(qWidth, 0);
                    lblQuestion.Width = qWidth;
                }
                UpdateRadioWidths();
            }

            private void UpdateRadioWidths()
            {
                if (answersPanel == null || currentOptions.Count == 0) return;

                // ширина панели минус padding и вертикальный скроллбар (если он есть)
                int w = answersPanel.ClientSize.Width
                        - answersPanel.Padding.Horizontal
                        - SystemInformation.VerticalScrollBarWidth - 6;
                if (w < 100) return;

                answersPanel.SuspendLayout();
                foreach (var opt in currentOptions)
                    opt.LayoutForWidth(w);
                answersPanel.ResumeLayout();
            }

            private void Timer_Tick(object sender, EventArgs e)
            {
                secondsLeft--;
                if (secondsLeft <= 0)
                {
                    timer.Stop();
                    MessageBox.Show("Время вышло! Тест завершён.", "Время",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    FinishTest();
                    return;
                }
                UpdateTimerLabel();
            }

            private void UpdateTimerLabel()
            {
                int min = secondsLeft / 60;
                int sec = secondsLeft % 60;
                lblTimer.Text = $"⏱ {min:00}:{sec:00}";
            }

            private void ShowQuestion(int index)
            {
                currentIndex = index;
                var q = questions[index];

                lblProgress.Text = $"Вопрос {index + 1} из {questions.Count}";
                lblQuestion.Text = q.Text;

                answersPanel.SuspendLayout();
                answersPanel.Controls.Clear();
                currentOptions.Clear();

                int w = answersPanel.ClientSize.Width
                        - answersPanel.Padding.Horizontal
                        - SystemInformation.VerticalScrollBarWidth - 6;
                if (w < 100) w = 400;

                for (int i = 0; i < q.Options.Length; i++)
                {
                    var opt = new AnswerOption(q.Options[i], i);
                    opt.LayoutForWidth(w);

                    int idx = i;
                    opt.Radio.Checked = (selectedAnswers[index] == i);
                    opt.Radio.CheckedChanged += (s, e) =>
                    {
                        if (opt.Radio.Checked) selectedAnswers[currentIndex] = idx;
                    };

                    answersPanel.Controls.Add(opt);
                    currentOptions.Add(opt);
                }

                answersPanel.ResumeLayout();
                answersPanel.PerformLayout();

                btnPrev.Enabled = index > 0;
                btnNext.Enabled = index < questions.Count - 1;
                UpdateTimerLabel();

                // Безопасный вызов: только если handle уже создан
                if (IsHandleCreated)
                    BeginInvoke(new Action(UpdateRadioWidths));
            }

            private void FinishTest()
            {
                timer.Stop();

                int correct = 0;
                for (int i = 0; i < questions.Count; i++)
                    if (selectedAnswers[i] == questions[i].CorrectIndex) correct++;

                int errors = questions.Count - correct;
                double percent = (double)correct / questions.Count * 100;
                double errorPercent = (double)errors / questions.Count * 100;

                string grade, mark;
                if (errorPercent <= 10) { grade = "Сдал"; mark = "Отлично"; }
                else if (errorPercent <= 20) { grade = "Сдал"; mark = "Хорошо"; }
                else if (errorPercent <= 30) { grade = "Сдал"; mark = "Удовлетворительно"; }
                else { grade = "Не сдал"; mark = "Неудовлетворительно"; }

                var result = new TestResult
                {
                    Date = DateTime.Now,
                    FullName = fullName,
                    Department = department,
                    Group = group,
                    TotalQuestions = questions.Count,
                    CorrectAnswers = correct,
                    Errors = errors,
                    ScorePercent = Math.Round(percent, 1),
                    Grade = grade,
                    Mark = mark
                };

                ResultStorage.Add(result);

                // Показываем форму с разбором ошибок
                using (var review = new ReviewForm(questions, selectedAnswers, result))
                {
                    review.ShowDialog(this);
                }

                Close();
            }
        }


        public partial class ResultsForm : Form
        {
            private DataGridView grid;
            private Button btnRefresh, btnDelete, btnClearAll;

            public ResultsForm()
            {
                Text = "Результаты тестирования";
                Size = new Size(1000, 500);
                StartPosition = FormStartPosition.CenterScreen;

                grid = new DataGridView
                {
                    Left = 10,
                    Top = 10,
                    Width = 960,
                    Height = 380,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                };

                btnRefresh = new Button { Text = "Обновить", Left = 10, Top = 400, Width = 120, Height = 35 };
                btnDelete = new Button { Text = "Удалить выбранную", Left = 140, Top = 400, Width = 160, Height = 35 };
                btnClearAll = new Button { Text = "Очистить все", Left = 310, Top = 400, Width = 120, Height = 35 };

                btnRefresh.Click += (s, e) => LoadResults();
                btnDelete.Click += BtnDelete_Click;
                btnClearAll.Click += BtnClearAll_Click;

                Controls.AddRange(new Control[] { grid, btnRefresh, btnDelete, btnClearAll });
                LoadResults();
            }

            private void LoadResults()
            {
                var list = ResultStorage.Load();
                grid.DataSource = null;
                grid.DataSource = list;
                // Настройка заголовков
                if (grid.Columns.Count > 0)
                {
                    grid.Columns["Date"].HeaderText = "Дата";
                    grid.Columns["FullName"].HeaderText = "ФИО";
                    grid.Columns["Department"].HeaderText = "Подразделение";
                    grid.Columns["Group"].HeaderText = "Группа";
                    grid.Columns["TotalQuestions"].HeaderText = "Всего";
                    grid.Columns["CorrectAnswers"].HeaderText = "Верно";
                    grid.Columns["Errors"].HeaderText = "Ошибок";
                    grid.Columns["ScorePercent"].HeaderText = "%";
                    grid.Columns["Grade"].HeaderText = "Итог";
                    grid.Columns["Mark"].HeaderText = "Оценка";
                }
            }

            private void BtnDelete_Click(object sender, EventArgs e)
            {
                if (grid.SelectedRows.Count == 0) return;
                var list = ResultStorage.Load();
                int index = grid.SelectedRows[0].Index;
                if (index >= 0 && index < list.Count)
                {
                    list.RemoveAt(index);
                    ResultStorage.Save(list);
                    LoadResults();
                }
            }

            private void BtnClearAll_Click(object sender, EventArgs e)
            {
                if (MessageBox.Show("Удалить все результаты?", "Подтверждение",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    ResultStorage.Clear();
                    LoadResults();
                }
            }
        }
    }
}

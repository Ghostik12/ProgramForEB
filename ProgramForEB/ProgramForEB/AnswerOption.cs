using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProgramForEB
{
    public class AnswerOption : Panel
    {
        public RadioButton Radio { get; private set; }
        public Label TextLabel { get; private set; }
        public int AnswerIndex { get; private set; }

        private bool _wasChecked;

        public AnswerOption(string text, int answerIndex)
        {
            AnswerIndex = answerIndex;

            Margin = new Padding(0, 4, 0, 4);
            Padding = new Padding(0);
            BackColor = Color.Transparent;

            Radio = new RadioButton
            {
                AutoSize = true,
                Location = new Point(4, 4),
                Text = "",
                Padding = new Padding(0),
                Margin = new Padding(0),
                TabStop = false
            };

            TextLabel = new Label
            {
                AutoSize = true,
                Location = new Point(28, 6),
                Text = text,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0),
                Padding = new Padding(0),
                MaximumSize = new Size(100, 0)
            };

            // --- Клик по радиокнопке: toggle ---
            Radio.MouseDown += (s, e) =>
            {
                _wasChecked = Radio.Checked;
            };
            Radio.Click += (s, e) =>
            {
                if (_wasChecked)
                    Radio.Checked = false; // повторный клик — снять выбор
            };

            // --- Клик по тексту: toggle ---
            TextLabel.Click += (s, e) =>
            {
                Radio.Checked = !Radio.Checked;
            };

            Controls.Add(Radio);
            Controls.Add(TextLabel);
        }

        public void LayoutForWidth(int width)
        {
            if (width < 80) width = 80;
            Width = width;

            int labelMax = width - TextLabel.Left - 8;
            if (labelMax < 50) labelMax = 50;

            TextLabel.MaximumSize = new Size(labelMax, 0);
            TextLabel.Width = labelMax;

            int bottom = Math.Max(Radio.Bottom, TextLabel.Bottom);
            Height = bottom + 4;
        }

        public bool Checked
        {
            get => Radio.Checked;
            set => Radio.Checked = value;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ProgramForEB.Form1;

namespace ProgramForEB
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Главное меню (можно реализовать как отдельную форму или через простое меню)
            while (true)
            {
                var choice = MessageBox.Show(
                    "Нажмите «Да» чтобы начать новый тест,\n" +
                    "«Нет» — чтобы просмотреть результаты,\n" +
                    "«Отмена» — для выхода.",
                    "Электробезопасность",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (choice == DialogResult.Yes)
                {
                    Application.Run(new Form1());
                }
                else if (choice == DialogResult.No)
                {
                    Application.Run(new ResultsForm());
                }
                else
                {
                    break;
                }
            }
        }
    }
}

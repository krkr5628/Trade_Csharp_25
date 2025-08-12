using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

// ERROR: The project uses the default "WindowsFormsApp1" namespace.
// This should be renamed to something descriptive for the project, like "TradeAutoKiwoom",
// to improve code clarity and organization.
namespace WindowsFormsApp1
{
    static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Trade_Auto());
        }
    }
}

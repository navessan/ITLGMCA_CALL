using System;
using System.Threading;
using System.Windows.Forms;

namespace ITLGMCA_CALL
{
	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			bool flag = false;
			Mutex mutex = new Mutex(true, "ITLGMCA_CALL", out flag);
			if (flag)
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new FormMain());
				return;
			}
			MessageBox.Show("Приложение уже запущено", "Сообщение", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}
}
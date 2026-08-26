using StockControl.Infrastructure;

namespace StockControl
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            DbPath.Load();
            var init = new DbInitializer();
            init.Initialize();

            //if (LicenciaHelper.LicenciaInstaladaYValida())
            //{
                Application.Run(new StockMain());
                return;
            //}

            // Si no, abrimos el formulario de activación
            using (var frm = new ActivationForm())
            {
                var result = frm.ShowDialog();
                if (result == DialogResult.OK && LicenciaHelper.LicenciaInstaladaYValida())
                {
                    Application.Run(new StockMain());
                }
                else
                {
                    MessageBox.Show("El programa no está activado.", "Licencia", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
using TaskMaster.Data;
using TaskMaster.Views.Forms;

namespace TaskMaster;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Inicializa o banco de dados
        DatabaseManager.Instance.Initialize();

        Application.Run(new FormMain());
    }
}

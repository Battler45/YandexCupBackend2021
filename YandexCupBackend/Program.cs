using System.Threading.Tasks;

namespace YandexCupBackend
{
    class Program
    {
        static async Task Main()
        {
            await TaskA.Solve();
            TaskB.Solve();
            TaskC.Solve();
            await TaskD.Solve();
            await TaskE.Solve();
        }
    }
}

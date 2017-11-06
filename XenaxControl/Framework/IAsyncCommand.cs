using System.Threading.Tasks;
using System.Windows.Input;

namespace XenaxControl
{
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync(object parameter);
    }
}

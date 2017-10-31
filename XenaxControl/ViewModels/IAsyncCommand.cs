using System.Threading.Tasks;
using System.Windows.Input;

namespace XenaxControl.ViewModels
{
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync(object parameter);
    }
}

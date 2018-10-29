using System.Threading.Tasks;
using System.Windows.Input;

namespace Framework.MVVM
{
    public interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync(object parameter);
    }
}

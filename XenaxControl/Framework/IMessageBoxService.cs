using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Framework.MVVM
{
    public interface IMessageBoxService
    {
        bool Show(string text, string caption, MessageBoxButton button, MessageBoxImage image);
    }
}

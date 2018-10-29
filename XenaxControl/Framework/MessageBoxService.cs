using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Framework.MVVM
{
    public class MessageBoxService : IMessageBoxService
    {
        public bool Show(string text, string caption, MessageBoxButton button, MessageBoxImage image)
        {
            MessageBoxResult result = MessageBox.Show(text, caption, button, image);

            if (result == MessageBoxResult.OK || result == MessageBoxResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

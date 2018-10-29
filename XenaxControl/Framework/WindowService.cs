using System.Windows;

namespace Framework.MVVM
{
    public class WindowService : IWindowService
    {
        public void Show<T>(object dataContext)
            where T : Window, new()
        {
            T window = new T
            {
                DataContext = dataContext
            };

            window.Show();
        }

        public bool? ShowDialog<T>(object dataContext)
            where T : Window, new()
        {
            T window = new T
            {
                DataContext = dataContext
            };

            return window.ShowDialog();
        }

        public T CreateWindow<T>(object dataContext)
            where T : Window, new()
        {
            T window = new T
            {
                DataContext = dataContext
            };

            return window;
        }
    }
}

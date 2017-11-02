using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace XenaxControl.ViewModels
{
    public class AsyncCommand<TResult> : IAsyncCommand, INotifyPropertyChanged
    {
        private readonly Func<Task<TResult>> command;
        private NotifyTaskCompletion<TResult> execution;

        public AsyncCommand(Func<Task<TResult>> command)
        {
            this.command = command;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Raises PropertyChanged
        public NotifyTaskCompletion<TResult> Execution
        {
            get
            {
                return this.execution;
            }

            private set
            {
                this.execution = value;
                this.OnPropertyChanged();
            }
        }

        public Task ExecuteAsync(object parameter)
        {
            this.Execution = new NotifyTaskCompletion<TResult>(this.command());
            return this.Execution.TaskCompletion;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            await this.ExecuteAsync(parameter);
        }

        private void RaieCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

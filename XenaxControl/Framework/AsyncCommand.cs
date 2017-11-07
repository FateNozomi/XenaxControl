using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace XenaxControl
{
    public class AsyncCommand<TResult> : IAsyncCommand, INotifyPropertyChanged
    {
        private readonly Func<CancellationToken, Task<TResult>> command;
        private readonly CancelAsyncCommand cancelCommand;
        private readonly Predicate<object> canExecute;
        private NotifyTaskCompletion<TResult> execution;

        public AsyncCommand(Func<CancellationToken, Task<TResult>> command, Predicate<object> canExecute = null)
        {
            this.command = command;
            this.cancelCommand = new CancelAsyncCommand();
            this.canExecute = canExecute;
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

        public ICommand CancelCommand
        {
            get
            {
                return this.cancelCommand;
            }
        }

        public bool CanExecute(object parameter)
        {
            if (this.canExecute == null || this.canExecute(parameter) == true)
            {
                return this.Execution == null || this.Execution.IsCompleted;
            }
            else
            {
                return false;
            }
        }

        public async void Execute(object parameter)
        {
            await this.ExecuteAsync(parameter);
        }

        public async Task ExecuteAsync(object parameter)
        {
            this.cancelCommand.NotifyCommandStarting();
            this.Execution = new NotifyTaskCompletion<TResult>(this.command(this.cancelCommand.Token));
            this.RaiseCanExecuteChanged();
            await this.Execution.TaskCompletion;
            this.cancelCommand.NotifyCommandFinished();
            this.RaiseCanExecuteChanged();
        }

        private void RaiseCanExecuteChanged()
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

        private sealed class CancelAsyncCommand : ICommand
        {
            private CancellationTokenSource cts = new CancellationTokenSource();
            private bool commandExecuting;

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public CancellationToken Token
            {
                get
                {
                    return this.cts.Token;
                }
            }

            bool ICommand.CanExecute(object parameter)
            {
                return this.commandExecuting && !this.cts.IsCancellationRequested;
            }

            void ICommand.Execute(object parameter)
            {
                this.cts.Cancel();
                this.RaiseCanExecuteChanged();
            }

            public void NotifyCommandStarting()
            {
                this.commandExecuting = true;
                if (!this.cts.IsCancellationRequested)
                {
                    return;
                }

                this.cts = new CancellationTokenSource();
                this.RaiseCanExecuteChanged();
            }

            public void NotifyCommandFinished()
            {
                this.commandExecuting = false;
                this.RaiseCanExecuteChanged();
            }

            private void RaiseCanExecuteChanged()
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }
}

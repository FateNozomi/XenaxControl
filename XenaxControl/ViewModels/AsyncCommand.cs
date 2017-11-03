using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace XenaxControl.ViewModels
{
    public class AsyncCommand<TResult> : IAsyncCommand, INotifyPropertyChanged
    {
        private readonly Func<CancellationToken, Task<TResult>> command;
        private readonly CancelAsyncCommand cancelCommand;
        private NotifyTaskCompletion<TResult> execution;

        public AsyncCommand(Func<CancellationToken, Task<TResult>> command)
        {
            this.command = command;
            this.cancelCommand = new CancelAsyncCommand();
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
            return this.Execution == null || this.Execution.IsCompleted;
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

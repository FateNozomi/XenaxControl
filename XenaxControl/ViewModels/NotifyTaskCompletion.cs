using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace XenaxControl.ViewModels
{
    public class NotifyTaskCompletion<TResult> : INotifyPropertyChanged
    {
        public NotifyTaskCompletion(Task<TResult> task)
        {
            this.Task = task;
            if (!task.IsCompleted)
            {
                this.TaskCompletion = this.WatchTaskAsync(task);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Task<TResult> Task { get; private set; }

        public TResult Result
        {
            get
            {
                return (this.Task.Status == TaskStatus.RanToCompletion) ?
                    this.Task.Result : default(TResult);
            }
        }

        public TaskStatus Status
        {
            get
            {
                return this.Task.Status;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return this.Task.IsCompleted;
            }
        }

        public bool IsNotCompleted
        {
            get
            {
                return !this.Task.IsCompleted;
            }
        }

        public bool IsSuccessfullyCompleted
        {
            get
            {
                return this.Task.Status == TaskStatus.RanToCompletion;
            }
        }

        public bool IsCanceled
        {
            get
            {
                return this.Task.IsCanceled;
            }
        }

        public bool IsFaulted
        {
            get
            {
                return this.Task.IsFaulted;
            }
        }

        public AggregateException Exception
        {
            get
            {
                return this.Task.Exception;
            }
        }

        public Exception InnerException
        {
            get
            {
                return (this.Exception == null) ? null : this.Exception.InnerException;
            }
        }

        public string ErrorMessage
        {
            get
            {
                return (this.InnerException == null) ? null : this.InnerException.Message;
            }
        }

        public Task TaskCompletion { get; private set; }

        private async Task WatchTaskAsync(Task task)
        {
            try
            {
                await task;
            }
            catch
            {
            }

            var propertyChanged = this.PropertyChanged;
            if (propertyChanged == null)
            {
                return;
            }

            propertyChanged(this, new PropertyChangedEventArgs("Status"));
            propertyChanged(this, new PropertyChangedEventArgs("IsCompleted"));
            propertyChanged(this, new PropertyChangedEventArgs("IsNotCompleted"));

            if (this.Task.IsCanceled)
            {
                propertyChanged(this, new PropertyChangedEventArgs("IsCanceled"));
            }
            else if (this.Task.IsFaulted)
            {
                propertyChanged(this, new PropertyChangedEventArgs("IsFaulted"));
                propertyChanged(this, new PropertyChangedEventArgs("Exception"));
                propertyChanged(this, new PropertyChangedEventArgs("InnerException"));
                propertyChanged(this, new PropertyChangedEventArgs("ErrorMessage"));
            }
            else
            {
                propertyChanged(this, new PropertyChangedEventArgs("IsSuccessfullyCompleted"));
                propertyChanged(this, new PropertyChangedEventArgs("Result"));
            }
        }
    }
}

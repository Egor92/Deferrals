using System;

namespace Egor92.Deferrals
{
    public interface IDeferral
    {
        bool IsCompleted { get; }

        event EventHandler Completed;

        void Complete();
    }

    public class Deferral : IDeferral
    {
        #region Fields

        private readonly object _completeSyncRoot = new object();

        #endregion

        #region Implementation of IDeferral

        public bool IsCompleted { get; private set; }

        #region Completed

        public event EventHandler Completed;

        private void RaiseContinueInvoked()
        {
            Completed?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        public void Complete()
        {
            lock (_completeSyncRoot)
            {
                if (IsCompleted)
                    return;
                IsCompleted = true;
                RaiseContinueInvoked();
            }
        }

        #endregion
    }
}

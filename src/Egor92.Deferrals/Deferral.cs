using System;

namespace Egor92.Deferrals
{
    public interface IDeferral
    {
        bool IsCompleted { get; }

        void Complete();
    }

    public class Deferral : IDeferral
    {
        #region Fields

        private readonly object _completeSyncRoot = new object();

        #endregion

        #region Properties

        #region IsCompleted

        public bool IsCompleted { get; private set; }

        #endregion

        #endregion

        #region Completed

        public event EventHandler Completed;

        private void RaiseContinueInvoked()
        {
            Completed?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Implementation of IDeferral

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

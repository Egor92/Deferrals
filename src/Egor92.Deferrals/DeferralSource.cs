using System;
using System.Collections.Generic;
using System.Linq;

namespace Egor92.Deferrals
{
    public class DeferralSource : IDisposable
    {
        #region Fields

        private readonly object _syncRoot = new object();
        private bool _isCompleted;
        private readonly Action _deferrableAction;
        private readonly IList<Deferral> _notCompletedDeferrals = new List<Deferral>();

        #endregion

        #region Ctor

        public DeferralSource(Action deferrableAction)
        {
            if (deferrableAction == null)
                throw new ArgumentNullException(nameof(deferrableAction));
            _deferrableAction = deferrableAction;
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            lock (_syncRoot)
            {
                foreach (var deferral in _notCompletedDeferrals)
                {
                    deferral.Completed -= OnDeferralCompleted;
                }
                _isCompleted = true;
            }
        }

        #endregion

        public IDeferral CreateDeferral()
        {
            lock (_syncRoot)
            {
                var deferral = new Deferral();
                if (!_isCompleted)
                {
                    _notCompletedDeferrals.Add(deferral);
                    deferral.Completed += OnDeferralCompleted;
                }
                return deferral;
            }
        }

        public void Invoke()
        {
            InvokeDeferrableActionIfCan();
        }

        private void InvokeDeferrableActionIfCan()
        {
            lock (_syncRoot)
            {
                var canInvoke = !_notCompletedDeferrals.Any();
                if (canInvoke)
                {
                    _deferrableAction();
                    _isCompleted = true;
                }
            }
        }

        private void OnDeferralCompleted(object sender, EventArgs e)
        {
            lock (_syncRoot)
            {
                var completedDeferral = (Deferral) sender;
                completedDeferral.Completed -= OnDeferralCompleted;
                _notCompletedDeferrals.Remove(completedDeferral);
            }
            InvokeDeferrableActionIfCan();
        }
    }
}

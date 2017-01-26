using System;
using System.Collections.Generic;
using System.Linq;

namespace Egor92.Deferrals
{
    public class DeferralSource
    {
        #region Fields

        private bool _isCompleted;
        private readonly Action _deferrableAction;
        private readonly IList<IDeferral> _deferrals = new List<IDeferral>();

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

        //public void Dispose()
        //{
        //    foreach (var continuable in _deferrals)
        //    {
        //        continuable.Completed -= OnDeferralCompleted;
        //    }
        //}

        #endregion

        public IDeferral CreateDeferral()
        {
            var continuable = new Deferral();
            _deferrals.Add(continuable);
            continuable.Completed += OnDeferralCompleted;
            return continuable;
        }

        public void Invoke()
        {
            InvokeIfCan();
        }

        private void InvokeIfCan()
        {
            var canContinue = _deferrals.All(x => x.IsCompleted);
            if (!canContinue)
                return;

            _deferrableAction();
        }

        private void OnDeferralCompleted(object sender, EventArgs e)
        {
            InvokeIfCan();
        }
    }
}

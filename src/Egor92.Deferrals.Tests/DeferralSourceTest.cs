using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Egor92.Deferrals.Tests
{
    [TestFixture]
    public class DeferralSourceTest
    {
        [Test]
        public void CanCreateDeferralSource()
        {
            new DeferralSource(() =>
            {
            });
        }

        [Test]
        public void CannotPassNullActionToConstructor()
        {
            Assert.That(() => new DeferralSource(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void CanCreateDefferal()
        {
            var deferralSource = new DeferralSource(() =>
            {
            });
            var deferral = deferralSource.CreateDeferral();
            Assert.IsNotNull(deferral);
        }

        [Test]
        public void IfDontCreateDeferralsAndInvokeDeferralSource_ThenDeferredActionWillNotBeInvoked()
        {
            bool isDeferredActionInvoked = false;
            var deferralSource = new DeferralSource(() => isDeferredActionInvoked = true);
            deferralSource.Invoke();
            Assert.IsTrue(isDeferredActionInvoked, "Deferred action was not invoked");
        }

        [Test]
        public void IfDeferralIsNotCompleted_ThenDeferredActionWillNotInvoke()
        {
            bool isDeferredActionInvoked = false;
            var deferralSource = new DeferralSource(() => isDeferredActionInvoked = true);
            var deferral = deferralSource.CreateDeferral();
            deferralSource.Invoke();
            Assert.IsFalse(isDeferredActionInvoked, "Deferred action was invoked");
        }

        [Test]
        public void IfCompleteDeferralBeforeDeferralSourceInvocation_ThenDeferredActionWillInvoke()
        {
            bool isDeferredActionInvoked = false;
            var deferralSource = new DeferralSource(() => isDeferredActionInvoked = true);
            var deferral = deferralSource.CreateDeferral();
            deferral.Complete();
            deferralSource.Invoke();
            Assert.IsTrue(isDeferredActionInvoked, "Deferred action was not invoked");
        }

        [Test]
        public void IfCompleteDeferralAfterDeferralSourceInvocation_ThenDeferredActionWillInvoke()
        {
            bool isDeferredActionInvoked = false;
            var deferralSource = new DeferralSource(() => isDeferredActionInvoked = true);
            var deferral = deferralSource.CreateDeferral();
            deferralSource.Invoke();
            deferral.Complete();
            Assert.IsTrue(isDeferredActionInvoked, "Deferred action was not invoked");
        }

        [Test]
        public void IfCreateAndCompleteManyDeferrals_ThenDeferredActionWillInvoke()
        {
            bool isDeferredActionInvoked = false;
            var deferralSource = new DeferralSource(() => isDeferredActionInvoked = true);
            const int deferralCount = 300;
            var deferrals = Enumerable.Range(0, deferralCount)
                                      .Select(x => deferralSource.CreateDeferral())
                                      .ToList();
            for (int i = 0; i < deferralCount / 2; i++)
            {
                deferrals[i].Complete();
            }

            deferralSource.Invoke();
            for (int i = deferralCount / 2; i < deferralCount; i++)
            {
                deferrals[i].Complete();
            }

            Assert.IsTrue(isDeferredActionInvoked, "Deferred action was not invoked");
        }

        [Test]
        public void IfCreateAndCompleteManyDeferralsExceptOne_ThenDeferredActionWillNotInvoke()
        {
            bool isDeferredActionInvoked = false;
            var deferralSource = new DeferralSource(() => isDeferredActionInvoked = true);
            const int deferralCount = 300;
            var deferrals = Enumerable.Range(0, deferralCount)
                                      .Select(x => deferralSource.CreateDeferral())
                                      .ToList();
            for (int i = 0; i < deferralCount / 2; i++)
            {
                deferrals[i].Complete();
            }

            deferralSource.Invoke();
            for (int i = deferralCount / 2; i < deferralCount - 1; i++)
            {
                deferrals[i].Complete();
            }

            Assert.IsFalse(isDeferredActionInvoked, "Deferred action was invoked");
        }

        [Test]
        public void IfDisposeDeferralSource_ThenDeferredActionWillNotInvoke()
        {
            bool isDeferredActionInvoked = false;
            var deferralSource = new DeferralSource(() => isDeferredActionInvoked = true);

            deferralSource.Dispose();

            Assert.IsFalse(isDeferredActionInvoked, "Deferred action was invoked");
        }

        [Test]
        public void IfDisposeDeferralSource_AndCreateDeferralAndCompleteIt_ThenDeferredActionWillNotInvoke()
        {
            bool isDeferredActionInvoked = false;
            var deferralSource = new DeferralSource(() => isDeferredActionInvoked = true);

            deferralSource.Dispose();

            var deferral = deferralSource.CreateDeferral();
            deferral.Complete();

            Assert.IsFalse(isDeferredActionInvoked, "Deferred action was invoked");
        }

        [Test]
        public void IfSeveralThreadsCreateDeferrals_ThenAllDeferralsWillBeAddedToNotCompletedDeferralsList()
        {
            var deferralSource = new DeferralSource(() =>
            {
            });

            const int taskCont = 1000;
            var deferrals = Enumerable.Range(0, taskCont)
                                      .AsParallel()
                                      .Select(_ => deferralSource.CreateDeferral())
                                      .ToArray();
            var notCompletedDeferralsField = typeof(DeferralSource).GetField("_notCompletedDeferrals",
                                                                             BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(notCompletedDeferralsField, Is.Not.Null, "DeferralSource._notCompletedDeferrals field is not found");

            var notCompletedDeferrals = (IList<Deferral>)notCompletedDeferralsField.GetValue(deferralSource);
            Assert.That(notCompletedDeferrals.Count, Is.EqualTo(taskCont));
            CollectionAssert.AreEquivalent(deferrals, notCompletedDeferrals);
        }
    }
}

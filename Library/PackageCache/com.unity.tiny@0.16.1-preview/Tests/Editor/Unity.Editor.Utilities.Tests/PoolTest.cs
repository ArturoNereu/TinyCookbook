using System.Collections.Generic;
using NUnit.Framework;
using System;
using System.Linq;

namespace Unity.Editor.Tests
{
    internal class PoolTest
    {
        [Test]
        public void CanGetAndReleasePooledList()
        {
            var list = ListPool<int>.Get();
            Assert.IsNotNull(list);
            Assert.DoesNotThrow(() => ListPool<int>.Release(list));
            
            var disposableList = ListPool<int>.GetDisposable();
            Assert.IsNotNull(disposableList);
            Assert.DoesNotThrow(() => disposableList.Dispose());
        }

        [Test]
        public void ReleasingAPooledListClearsIt()
        {
            const int count = 5;
            var list = ListPool<int>.Get();
            Assert.IsNotNull(list);
            Assert.IsTrue(list.Count == 0);
            for(var i = 0; i < count; ++i)
            {
                list.Add(i);
            }
            
            Assert.IsTrue(list.Count == count);
            Assert.DoesNotThrow(() => ListPool<int>.Release(list));
            Assert.IsTrue(list.Count == 0);
        }

        [Test]
        public void ReleasingUnownedPooledListThrows()
        {
            Assert.Throws<InvalidOperationException>(() => ListPool<int>.Release(new List<int>()));
        }

        [Test]
        public void ReleasingMultipleTimesThrows()
        {
            var list = ListPool<int>.Get();
            Assert.DoesNotThrow(() => ListPool<int>.Release(list));
            Assert.Throws<InvalidOperationException>(() => ListPool<int>.Release(list));
        }

        [Test]
        public void CanGetAndReleaseInDifferentOrder()
        {
            var list = ListPool<int>.Get();
            var list2 = ListPool<int>.GetDisposable();
            Assert.DoesNotThrow(() => list2.Dispose());
            Assert.DoesNotThrow(() => ListPool<int>.Release(list));
        }

        [Test]
        public void MultipleGetResultsInDifferentPooledLists()
        {
            var list = ListPool<int>.Get();
            var list2 = ListPool<int>.GetDisposable();

            Assert.AreNotSame(list, list2.List);

            Assert.DoesNotThrow(() => ListPool<int>.Release(list));
            Assert.DoesNotThrow(() => list2.Dispose());
        }

        [Test]
        public void PooledListAreIndeedPooled()
        {
            var  releasedPools = new HashSet<List<int>>();
            releasedPools.Add(ListPool<int>.Get());
            releasedPools.Add(ListPool<int>.Get());
            releasedPools.Add(ListPool<int>.Get());

            foreach (var pooledList in releasedPools)
            {
                Assert.DoesNotThrow(() => ListPool<int>.Release(pooledList));
            }            
            
            var matchesList1 = ListPool<int>.GetDisposable().List;
            var matchesList2 = ListPool<int>.GetDisposable().List;
            var matchesList3 = ListPool<int>.GetDisposable().List;
            
            Assert.IsTrue(releasedPools.Contains(matchesList1));
            Assert.IsTrue(releasedPools.Contains(matchesList2));
            Assert.IsTrue(releasedPools.Contains(matchesList3));

            foreach (var pooledList in releasedPools)
            {
                Assert.DoesNotThrow(() => ListPool<int>.Release(pooledList));
            }   
        }

        [Test]
        public void InvalidLifetimePolicyThrows()
        {
            Assert.Throws<InvalidOperationException>(() => ListPool<int>.Get((LifetimePolicy)5));
        }

        [Test]
        public void ReleaseNullListThrows()
        {
            Assert.Throws<InvalidOperationException>(() => ListPool<int>.Release(null));
        }
    }
}

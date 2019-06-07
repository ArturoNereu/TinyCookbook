using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Editor;

namespace UnityEditor.Searcher
{
    internal class ECSComponentDatabase : SearcherDatabaseBase
    {
        class Result
        {
            public SearcherItem item;
            public float maxScore;
        }

        private Func<string, SearcherItem, bool> MatchFilter { get; set; }

        public ECSComponentDatabase(IReadOnlyCollection<SearcherItem> db)
            : this("", db)
        {
        }

        private ECSComponentDatabase(string databaseDirectory, IReadOnlyCollection<SearcherItem> db)
            : base(databaseDirectory)
        {
            m_ItemList = new List<SearcherItem>();
            var nextId = 0;

            if (db != null)
                foreach (var item in db)
                    AddItemToIndex(item, ref nextId, null);
        }

        public override List<SearcherItem> Search(string query, out float localMaxScore)
        {
            localMaxScore = 0;
            if (!string.IsNullOrEmpty(query))
            {
                var filter = FilterUtility.CreateAddComponentFilter(query);

                MatchFilter = (s, item) =>
                {
                    if (!(item is TypeSearcherItem))
                    {
                        return false;
                    }

                    return filter.Keep(item.Name);
                };
            }
            else
            {
                MatchFilter = null;
                return m_ItemList;
            }

            var finalResults = new List<SearcherItem> { null };
            var max = new Result();

            // ReSharper disable once RedundantLogicalConditionalExpressionOperand
            if (m_ItemList.Count > 100)
            {
                SearchMultithreaded(query, max, finalResults);
            }
            else
            {
                SearchSingleThreaded(query, max, finalResults);
            }

            localMaxScore = max.maxScore;
            if (max.item != null)
            {
                finalResults[0] = max.item;
            }
            else
            {
                finalResults.RemoveAt(0);
            }

            return finalResults;
        }

        private bool Match(string query, SearcherItem item)
        {
            return MatchFilter?.Invoke(query, item) ?? true;
        }

        private void SearchSingleThreaded(string query, Result max, ICollection<SearcherItem> finalResults)
        {
            foreach (var item in m_ItemList)
            {
                if (query.Length == 0 || Match(query, item))
                {
                    finalResults.Add(item);
                }
            }
        }

        private void SearchMultithreaded(string query, Result max, List<SearcherItem> finalResults)
        {
            var count = Environment.ProcessorCount;
            var tasks = new Task[count];
            var localResults = new Result[count];
            var queue = new ConcurrentQueue<SearcherItem>();
            var itemsPerTask = (int)Math.Ceiling(m_ItemList.Count / (float)count);

            for (var i = 0; i < count; i++)
            {
                var i1 = i;
                localResults[i1] = new Result();
                tasks[i] = Task.Run(() =>
                {
                    var result = localResults[i1];
                    for (var j = 0; j < itemsPerTask; j++)
                    {
                        var index = j + itemsPerTask * i1;
                        if (index >= m_ItemList.Count)
                            break;
                        var item = m_ItemList[index];
                        if (query.Length == 0 || Match(query, item))
                        {
                            queue.Enqueue(item);
                        }
                    }
                });
            }

            Task.WaitAll(tasks);

            for (var i = 0; i < count; i++)
            {
                if (localResults[i].maxScore > max.maxScore)
                {
                    max.maxScore = localResults[i].maxScore;
                    if (max.item != null)
                        queue.Enqueue(max.item);
                    max.item = localResults[i].item;
                }
                else if (localResults[i].item != null)
                    queue.Enqueue(localResults[i].item);
            }

            finalResults.AddRange(queue.OrderBy(i => i.Id));
        }
    }
}

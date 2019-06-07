using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Authoring;

namespace Unity.Editor
{
    internal static class EntityNameHelper
    {
        private static readonly Regex s_EntityNameRegex = new Regex(@"(?<entityName>.+) (\(\d+\))", RegexOptions.Compiled);

        internal static string GetUniqueEntityName(string name, IWorldManager worldManager, List<ISceneGraphNode> siblings)
        {
            if (siblings?.Count == 0)
                return name;

            using (var pooledList = ListPool<string>.GetDisposable())
            {
                foreach (var sibling in siblings)
                {
                    if(sibling is EntityNode entityNode)
                    {
                        pooledList.List.Add(worldManager.GetEntityName(entityNode.Entity));
                    }
                }

                return GetUniqueEntityName(name, pooledList.List);
            }
        }

        private static string GetUniqueEntityName(string originalEntityName, List<string> siblingsNames)
        {
            var match = s_EntityNameRegex.Match(originalEntityName);
            if (match.Success)
            {
                originalEntityName = match.Groups["entityName"].Value;
            }

            if (siblingsNames?.Count == 0)
                return originalEntityName;

            var targetEntityName = originalEntityName;
            var counter = 0;
            for (var i = 0; i < siblingsNames.Count; i++)
            {
                if (targetEntityName == siblingsNames[i])
                {
                    targetEntityName = $"{originalEntityName} ({++counter})";
                    i = 0;
                }
            }

            return targetEntityName;
        }

    }
}

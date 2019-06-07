using System;

namespace Unity.Authoring.Core
{
    /// <summary>
    /// Allows you to add descriptions for fields in the Inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class TooltipAttribute : Attribute
    {
        public readonly string Tooltip;

        public TooltipAttribute(string tooltip)
        {
            Tooltip = tooltip;
        }
    }
}

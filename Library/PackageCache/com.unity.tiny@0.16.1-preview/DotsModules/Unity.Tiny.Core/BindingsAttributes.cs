using System;
using System.Runtime.InteropServices;
using Unity.Authoring.Core;

namespace Unity.Tiny
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PureAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.All)]
    public class IgnoreAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class ModuleHideInEditorAttribute : Attribute
    {
        public ModuleHideInEditorAttribute(string mod)
        {
            ThisModule = mod;
        }

        public string ThisModule;
    }

    public enum Platform
    {
        Web = 1,
        PC = 2,
        WeChat = 4,
        FBInstant = 8,
        iOS = 16,
        Android = 32
    }
}



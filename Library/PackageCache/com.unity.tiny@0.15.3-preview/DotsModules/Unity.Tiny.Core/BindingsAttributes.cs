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
    public class ModuleDescriptionAttribute : Attribute
    {
        public ModuleDescriptionAttribute(string mod, string desc)
        {
            ThisModule = mod;
            Description = desc;
        }

        public string ThisModule, Description;
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

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class IncludedPlatformAttribute : Attribute
    {
        public IncludedPlatformAttribute(Platform p)
        {
            Platform = p;
        }

        public Platform Platform;
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class ExcludedPlatformAttribute : Attribute
    {
        public ExcludedPlatformAttribute(Platform p)
        {
            Platform = p;
        }

        public Platform Platform;
    }

}



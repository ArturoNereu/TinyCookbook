using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;

namespace Unity.Tiny.Debugging
{
    public static class StringFormatter
    {
        // Implements similar functionality to string.Format.
        public static string Format(string format, params object[] args)
        {
            string result = "";
            int index = 0;
            int argIndex = 0;
            bool isParsingArg = false;
            bool isParsingArgIndex = false;

            while (index < format.Length)
            {
                if (!isParsingArg && format[index] == '{')
                {
                    argIndex = 0;
                    isParsingArg = true;
                    isParsingArgIndex = true;
                    index++;
                    continue;
                }

                if (isParsingArg && format[index] == '}')
                {
                    isParsingArg = false;
                    isParsingArgIndex = false;
                    if (argIndex < args.Length)
                        result += ResolveParam(args[argIndex]);
                    else
                        result += "[Bad argument index in Debug.Format]";
                    index++;
                    continue;
                }

                if (isParsingArg && format[index] != '}')
                {
                    if (isParsingArgIndex && format[index] >= '0' && format[index] <= '9')
                    {
                        argIndex *= 10;
                        argIndex += format[index] - '0';
                    }
                    else
                        isParsingArgIndex = false;

                    index++;
                    continue;
                }

                result += format[index];
                index++;
            }

            return result;
        }

        private static string ResolveParam(object param)
        {
            if (param is int)
                return ((int)param).ToString();
            if (param is float)
                return NumberConverter.FloatToString((float)param);
            if (param is double)
                return NumberConverter.DoubleToString((double)param);
            if (param is string)
                return (string)param;
            if (param is bool)
                return (bool)param ? "true" : "false";
            if (param is Entity)
            {
                var mgr = World.Active.EntityManager;
                string desc;
                var entity = (Entity)param;
                if (mgr.Exists(entity)) {
                    if (mgr.HasComponent<EntityName>(entity))
                        desc = mgr.GetBufferAsString<EntityName>(entity);
                    else
                        desc = "Unnamed Entity";
                } else {
                    desc = "Non Existing Entity";
                }
                return Format("[{0} {1}:{2}]", desc, entity.Index, entity.Version);
            }
            if (param is float2)
            {
                var x = (float2)param;
                return Format("({0}, {1})",x.x, x.y);
            }
            if (param is float3)
            {
                var x = (float3)param;
                return Format("({0}, {1}, {2})",x.x, x.y, x.z);
            }
            if (param is float4)
            {
                var x = (float4)param;
                return Format("({0}, {1}, {2}, {3})",x.x, x.y, x.z, x.w);
            }
            if (param is Unity.Entities.TypeManager.TypeInfo)
            {
                return "[TypeInfo]";
            }
            if (param is IComponentData)
                return "[IComponentData]";

            return "[type not supported]";
        }
    }
}

//#define WRITE_LOG

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using JobTypeDefinitionMap = System.Collections.Generic.Dictionary<string, Unity.ZeroPlayer.JobGenInfo>;

namespace Unity.ZeroPlayer
{
    public struct JobGenInfo
    {
        public AssemblyDefinition InterfaceAssembly;
        public Dictionary<TypeDefinition, List<FieldDefinition>> TypeFieldMap;
    }

    internal class JobsGen
    {
        readonly static string[] JobInterfaceNames =
        {
            "Unity.Jobs.IJob",
            "Unity.Jobs.IJobParallelFor",
            "Unity.Entities.IJobChunk",
            "Unity.Entities.JobForEachExtensions/IBaseJobForEach"
        };
        readonly static string[] JobInterfaceExtensionNames =
        {
            "Unity.Jobs.IJobExtensions",
            "Unity.Jobs.IJobParallelForExtensions",
            "Unity.Entities.JobChunkExtensions",
            "Unity.Entities.JobForEachExtensions"
        };

        public static JobTypeDefinitionMap GetDeallocateOnCompletionJobs(in List<AssemblyDefinition> assemblies)
        {
            // asm.MainModule.Types doesn't included nested types. (Those that contain a '/' in the full name.)
            // We have to call GetAllTypes() to get to nested types.

            JobTypeDefinitionMap jobTypeDefMap = new JobTypeDefinitionMap();
            foreach(var jobInterfaceName in JobInterfaceNames)
            {
                AssemblyDefinition interfaceAssembly = null;
                foreach(var asm in assemblies)
                {
                    var td = asm.MainModule.GetAllTypes().FirstOrDefault(t => t.FullName == jobInterfaceName);
                    if (td != null)
                    {
                        interfaceAssembly = asm;
                        break;
                    }
                }
                if (interfaceAssembly == null) throw new InvalidOperationException($"Could not find interface '{jobInterfaceName}'");

                jobTypeDefMap[jobInterfaceName] = new JobGenInfo()
                {
                    InterfaceAssembly = interfaceAssembly,
                    TypeFieldMap = new Dictionary<TypeDefinition, List<FieldDefinition>>()
                };
            }

            foreach (var asm in assemblies)
            {
                foreach (var jobInterfaceName in JobInterfaceNames)
                {
                    var jobGenInfo = jobTypeDefMap[jobInterfaceName];
                    var typeFieldMap = jobGenInfo.TypeFieldMap;

                    foreach (var job in asm.MainModule.GetAllTypes().Where(t => t.IsValueType &&
                            t.Interfaces.Select(f => f.InterfaceType.Resolve().FullName).Contains(jobInterfaceName)))
                    {
                        foreach (var field in job.Fields)
                        {
                            var deallocateOnJobCompletionAttr = field.CustomAttributes.FirstOrDefault(ca => ca.Constructor.DeclaringType.Name == "DeallocateOnJobCompletionAttribute");
                            if(deallocateOnJobCompletionAttr != null)
                            {
                                if (!typeFieldMap.ContainsKey(job))
                                {
                                    typeFieldMap[job] = new List<FieldDefinition>();
                                }
                                typeFieldMap[job].Add(field);
                            }
                        }
                    }
                    jobGenInfo.TypeFieldMap = typeFieldMap;
                    jobTypeDefMap[jobInterfaceName] = jobGenInfo;
                }
            }

            return jobTypeDefMap;
        }

        public static void GenerateDealllocateOnJobCompletionFn(MethodReference getTypeFnRef, MethodReference getTypeFromHandleFnRef, in JobTypeDefinitionMap jobTypes)
        {
            for(int i = 0; i < JobInterfaceNames.Length; ++i)
            {
                string jobInterfaceName = JobInterfaceNames[i];
                string jobExtensionName = JobInterfaceExtensionNames[i];
                var jobGenInfo = jobTypes[jobInterfaceName];
                var iJobExtDef = jobGenInfo.InterfaceAssembly.MainModule.GetType(jobExtensionName);
                var deallocateFn = iJobExtDef.Methods.First(m => m.Name == "DoDeallocateOnJobCompletion");

                GenerateFieldDeallocationsPerJobType(deallocateFn, in getTypeFnRef, in getTypeFromHandleFnRef, in jobGenInfo);
            }
        }

        private static void GenerateFieldDeallocationsPerJobType(MethodDefinition deallocateMethod, in MethodReference getTypeFn, in MethodReference getTypeFromHandleFnRef, in JobGenInfo jobGenInfo)
        {
            deallocateMethod.Body.Instructions.Clear();
            deallocateMethod.Body.InitLocals = true;

            var jobAsm = jobGenInfo.InterfaceAssembly;
            var typeFieldMap = jobGenInfo.TypeFieldMap;

            var il = deallocateMethod.Body.Instructions;
            var typeofRef = jobAsm.MainModule.ImportReference(getTypeFromHandleFnRef);
            var getTypeRef = jobAsm.MainModule.ImportReference(getTypeFn);

            foreach (var job in typeFieldMap.Keys)
            {
                var jobRef = jobAsm.MainModule.ImportReference(job);

                il.Add(Instruction.Create(OpCodes.Ldtoken, jobRef));
                il.Add(Instruction.Create(OpCodes.Call, typeofRef));
                il.Add(Instruction.Create(OpCodes.Ldarg_0));
                il.Add(Instruction.Create(OpCodes.Callvirt, getTypeRef));

                int branchToNext = il.Count;
                il.Add(Instruction.Create(OpCodes.Nop)); // replaced with branch

                var fieldList = typeFieldMap[job];
                if (fieldList.Count > 0)
                {
                    var local = new VariableDefinition(jobRef);
                    deallocateMethod.Body.Variables.Add(local);

                    il.Add(Instruction.Create(OpCodes.Ldarg_0));
                    il.Add(Instruction.Create(OpCodes.Unbox_Any, jobRef));
                    il.Add(Instruction.Create(OpCodes.Stloc, local));

                    // Free Fields
                    foreach (var field in typeFieldMap[job])
                    {
                        il.Add(Instruction.Create(OpCodes.Ldloca, local));
                        il.Add(Instruction.Create(OpCodes.Ldflda, jobAsm.MainModule.ImportReference(field)));

                        var disposeFnDef = field.FieldType.Resolve().Methods.FirstOrDefault(m => m.Name == "Dispose");
                        disposeFnDef.IsPublic = true;
                        var disposeFnRef = jobAsm.MainModule.ImportReference(disposeFnDef);
                        if(field.FieldType is GenericInstanceType)
                        {
                            GenericInstanceType git = (GenericInstanceType) field.FieldType;
                            List<TypeReference> genericArgs = new List<TypeReference>();
                            foreach(var specializationType in git.GenericArguments)
                            {
                                genericArgs.Add(jobAsm.MainModule.ImportReference(specializationType));
                            }

                            disposeFnRef = jobAsm.MainModule.ImportReference(disposeFnDef.MakeHostInstanceGeneric(genericArgs.ToArray()));
                        }

                        if (disposeFnRef == null)
                            throw new Exception($"{job.Name}::{field.Name} is missing a {field.FieldType.Name}::Dispose() implementation");

                        il.Add(Instruction.Create(OpCodes.Call, disposeFnRef));
                    }
                }
                il.Add(Instruction.Create(OpCodes.Ret));
                var nextTest = Instruction.Create(OpCodes.Nop);
                il.Add(nextTest);

                il[branchToNext] = Instruction.Create(OpCodes.Bne_Un, nextTest);
            }

            il.Add(Instruction.Create(OpCodes.Ret));
        }
    }
}

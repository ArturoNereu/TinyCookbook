using System;
using System.Runtime.InteropServices;
#if !NET_DOTS
using System.Text.RegularExpressions;
#endif
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine.Assertions;

namespace Unity.Jobs
{
#if !UNITY_SINGLETHREADED_JOBS && UNITY_AVOID_REFLECTION
    [JobProducerType(typeof(IJobExtensions.JobStruct<>))]
#endif
    public interface IJob
    {
        void Execute();
    }

#if !UNITY_SINGLETHREADED_JOBS && UNITY_AVOID_REFLECTION
    [JobProducerType(typeof(IJobParallelForExtensions.ParallelForJobStruct<>))]
#endif
    public interface IJobParallelFor
    {
        void Execute(int index);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JobHandle
    {
        internal IntPtr JobGroup;
        internal uint Version;

        public static void ScheduleBatchedJobs() {}

        public void Complete()
        {
#if !UNITY_SINGLETHREADED_JOBS
            if (JobsUtility.JobQueue != IntPtr.Zero)
                JobsUtility.WaitForJobGroupID(this);
#endif
        }

        public static bool CheckFenceIsDependencyOrDidSyncFence(JobHandle dependency, JobHandle writer) => true;

        public static unsafe JobHandle CombineDependencies(NativeArray<JobHandle> jobHandles)
        {
#if UNITY_SINGLETHREADED_JOBS
            return default(JobHandle);
#else
            var fence = new JobHandle();
            JobsUtility.ScheduleMultiDependencyJob(ref fence, JobsUtility.BatchScheduler, new IntPtr(jobHandles.GetUnsafeReadOnlyPtr()), jobHandles.Length);
            return fence;
#endif
        }

        public static unsafe JobHandle CombineDependencies(JobHandle mProducerHandle, JobHandle foo)
        {
#if UNITY_SINGLETHREADED_JOBS
            return default(JobHandle);
#else
            var fence = new JobHandle();
            var dependencies = stackalloc JobHandle[]
            {
                mProducerHandle,
                foo,
            };
            JobsUtility.ScheduleMultiDependencyJob(ref fence, JobsUtility.BatchScheduler, new IntPtr(UnsafeUtility.AddressOf(ref dependencies[0])), 2);
            return fence;
#endif
        }
    }

    public static class IJobExtensions
    {

#if !UNITY_SINGLETHREADED_JOBS && UNITY_AVOID_REFLECTION
        internal struct JobStruct<T> where T : struct, IJob
        {
            public static JobsUtility.ManagedJobDelegate ExecuteDelegate;
            public static GCHandle ExecuteHandle;
            public static IntPtr ExecuteFunctionPtr;

            public T JobData;

            public static unsafe void Execute(void* structPtr)
            {
                var jobStruct = UnsafeUtility.AsRef<JobStruct<T>>(structPtr);
                var jobData = jobStruct.JobData;
                jobData.Execute();
                DoDeallocateOnJobCompletion(jobData);
                UnsafeUtility.Free(structPtr, Allocator.TempJob);
            }
        }
#endif

        public static JobHandle Schedule<T>(this T jobData, JobHandle dependsOn = default(JobHandle)) where T : struct, IJob
        {
#if UNITY_SINGLETHREADED_JOBS
            jobData.Run();
            return default(JobHandle);
#elif UNITY_AVOID_REFLECTION
            unsafe
            {
                // Protect against garbage collection
                if (!JobStruct<T>.ExecuteHandle.IsAllocated)
                {
                    JobStruct<T>.ExecuteDelegate = JobStruct<T>.Execute;
                    JobStruct<T>.ExecuteHandle = GCHandle.Alloc(JobStruct<T>.ExecuteDelegate);
                    JobStruct<T>.ExecuteFunctionPtr = Marshal.GetFunctionPointerForDelegate(JobStruct<T>.ExecuteDelegate);
                }

                var jobStruct = new JobStruct<T>()
                {
                    JobData = jobData
                };

                var jobDataPtr = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<JobStruct<T>>(), UnsafeUtility.AlignOf<JobStruct<T>>(), Allocator.TempJob);
                UnsafeUtility.CopyStructureToPtr(ref jobStruct, jobDataPtr);

                return JobsUtility.ScheduleJob(JobStruct<T>.ExecuteFunctionPtr, new IntPtr(jobDataPtr), dependsOn);
            }
#endif
        }

        public static void Run<T>(this T jobData) where T : struct, IJob
        {
            jobData.Execute();
            DoDeallocateOnJobCompletion(jobData);
        }

        static void DoDeallocateOnJobCompletion(object jobData)
        {
            throw new NotImplementedException("This function should have been replaced by codegen");
        }
    }

    public static class IJobParallelForExtensions
    {
#if !UNITY_SINGLETHREADED_JOBS && UNITY_AVOID_REFLECTION
        internal struct ParallelForJobStruct<T> where T : struct, IJobParallelFor
        {
            public static JobsUtility.ManagedJobForEachDelegate ExecuteDelegate;
            public static GCHandle ExecuteHandle;
            public static IntPtr ExecuteFunctionPtr;

            public static JobsUtility.ManagedJobDelegate CleanupDelegate;
            public static GCHandle CleanupHandle;
            public static IntPtr CleanupFunctionPtr;

            public T JobData;
            public JobRanges Ranges;

            public static unsafe void Execute(void* structPtr, int jobIndex)
            {
                var jobStruct = UnsafeUtility.AsRef<ParallelForJobStruct<T>>(structPtr);
                var ranges = jobStruct.Ranges;
                var jobData = jobStruct.JobData;

                while (true)
                {
                    if (!JobsUtility.GetWorkStealingRange(ref ranges, jobIndex, out var begin, out var end))
                        break;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                JobsUtility.PatchBufferMinMaxRanges(IntPtr.Zero, UnsafeUtility.AddressOf(ref jobData), begin, end - begin);
#endif

                    for (var i = begin; i < end; ++i)
                    {
                        jobData.Execute(i);
                    }

                    break;
                }
            }

            public static unsafe void Cleanup(void* structPtr)
            {
                var jobStruct = UnsafeUtility.AsRef<ParallelForJobStruct<T>>(structPtr);
                var jobData = jobStruct.JobData;
                DoDeallocateOnJobCompletion(jobData);
                UnsafeUtility.Free(structPtr, Allocator.TempJob);
            }
        }
#endif

        public static JobHandle Schedule<T>(this T jobData, int arrayLength, int innerloopBatchCount, JobHandle dependsOn = default(JobHandle)) where T : struct, IJobParallelFor
        {
#if UNITY_SINGLETHREADED_JOBS
            for (int i = 0; i != arrayLength; i++)
            {
                jobData.Execute(i);
            }

            DoDeallocateOnJobCompletion(jobData);
            return new JobHandle();
#elif UNITY_AVOID_REFLECTION
            // NOTE: In testing, some jobs were coming in from Entities with arrayLength 0, so there's nothing to actually do
            if (arrayLength == 0)
            {
                DoDeallocateOnJobCompletion(jobData);
                return new JobHandle();
            }

            unsafe
            {
                // Protect against garbage collection
                if (!ParallelForJobStruct<T>.ExecuteHandle.IsAllocated)
                {
                    ParallelForJobStruct<T>.ExecuteDelegate = ParallelForJobStruct<T>.Execute;
                    ParallelForJobStruct<T>.ExecuteHandle = GCHandle.Alloc(ParallelForJobStruct<T>.ExecuteDelegate);
                    ParallelForJobStruct<T>.ExecuteFunctionPtr = Marshal.GetFunctionPointerForDelegate(ParallelForJobStruct<T>.ExecuteDelegate);
                }

                // Protect against garbage collection
                if (!ParallelForJobStruct<T>.CleanupHandle.IsAllocated)
                {
                    ParallelForJobStruct<T>.CleanupDelegate = ParallelForJobStruct<T>.Cleanup;
                    ParallelForJobStruct<T>.CleanupHandle = GCHandle.Alloc(ParallelForJobStruct<T>.CleanupDelegate);
                    ParallelForJobStruct<T>.CleanupFunctionPtr = Marshal.GetFunctionPointerForDelegate(ParallelForJobStruct<T>.CleanupDelegate);
                }

                var jobFunctionPtr = ParallelForJobStruct<T>.ExecuteFunctionPtr;
                var completionFuncPtr = ParallelForJobStruct<T>.CleanupFunctionPtr;

                var jobStruct = new ParallelForJobStruct<T>()
                {
                    JobData = jobData,
                    Ranges = new JobRanges()
                    {
                        ArrayLength =  arrayLength,
                        IndicesPerPhase = JobsUtility.GetDefaultIndicesPerPhase(arrayLength),
                    },
                };

                var jobDataPtr = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<ParallelForJobStruct<T>>(),
                    UnsafeUtility.AlignOf<ParallelForJobStruct<T>>(), Allocator.TempJob);
                UnsafeUtility.CopyStructureToPtr(ref jobStruct, jobDataPtr);

                return JobsUtility.ScheduleJobForEach(jobFunctionPtr, completionFuncPtr, new IntPtr(jobDataPtr),
                    arrayLength, innerloopBatchCount, dependsOn);
            }
#endif
        }

        static void DoDeallocateOnJobCompletion(object jobData)
        {
            throw new NotImplementedException("This function should have been replaced by codegen");
        }
    }
}


namespace Unity.Jobs.LowLevel.Unsafe
{
    public static class JobsUtility
    {
        public const int JobQueueThreadCount = 4;
        public const int MaxJobThreadCount = 128;
        public const int CacheLineSize = 64;

        public static bool JobCompilerEnabled = false;
        public static bool JobDebuggerEnabled => false;

#if UNITY_AVOID_REFLECTION
        public unsafe delegate void ManagedJobDelegate(void* ptr);
        public unsafe delegate void ManagedJobForEachDelegate(void* ptr, int jobIndex);
#endif

#if !UNITY_SINGLETHREADED_JOBS || UNITY_AVOID_REFLECTION
        internal static IntPtr JobQueue
        {
            get
            {
                if (s_JobQueue == IntPtr.Zero)
                {
                    s_JobQueue = CreateJobQueue("job-queue", "worker-bee", JobQueueThreadCount);
                }

                return s_JobQueue;
            }
        }
        internal static IntPtr BatchScheduler
        {
            get
            {
                if (s_BatchScheduler == IntPtr.Zero)
                {
                    Assert.IsTrue(JobQueue != IntPtr.Zero);
                    s_BatchScheduler = CreateJobBatchScheduler();
                }

                return s_BatchScheduler;
            }
        }

        static IntPtr s_JobQueue;
        static IntPtr s_BatchScheduler;

        public enum JobQueuePriority : byte
        {
            kNormalJobPriority = 0,
            kHighJobPriority = 1 << 0,
        };

        public static JobHandle ScheduleJob(IntPtr jobFuncPtr, IntPtr jobDataPtr, JobHandle dependsOn)
        {
            Assert.IsTrue(JobQueue != IntPtr.Zero);
            return ScheduleJob(jobFuncPtr, jobDataPtr, dependsOn, JobQueuePriority.kNormalJobPriority);
        }

        public static JobHandle ScheduleJobForEach(IntPtr jobFuncPtr, IntPtr jobCompletionFuncPtr, IntPtr jobDataPtr, int arrayLength, int innerloopBatchCount, JobHandle dependsOn)
        {
            Assert.IsTrue(JobQueue != IntPtr.Zero && BatchScheduler != IntPtr.Zero);
            return ScheduleJobBatchForEach(BatchScheduler, jobFuncPtr, jobDataPtr, arrayLength, innerloopBatchCount, jobCompletionFuncPtr, dependsOn);
        }

        // TODO: Need to find a good place to shut down jobs on application quit/exit
        public static void Shutdown()
        {
            if (s_BatchScheduler != IntPtr.Zero)
            {
                DestroyJobBatchScheduler(s_BatchScheduler);
                s_BatchScheduler = IntPtr.Zero;
            }

            if (s_JobQueue != IntPtr.Zero)
            {
                DestroyJobQueue();
                s_JobQueue = IntPtr.Zero;
            }
        }

        [DllImport("nativejobs")]
        static extern IntPtr CreateJobQueue(string queueName, string workerName, int numJobWorkerThreads);

        [DllImport("nativejobs")]
        static extern void DestroyJobQueue();

        [DllImport("nativejobs")]
        static extern IntPtr CreateJobBatchScheduler();

        [DllImport("nativejobs")]
        static extern void DestroyJobBatchScheduler(IntPtr scheduler);

        [DllImport("nativejobs")]
        static extern JobHandle ScheduleJob(IntPtr func, IntPtr userData, JobHandle dependency, JobQueuePriority priority);

        [DllImport("nativejobs")]
        static extern JobHandle ScheduleJobBatchForEach(IntPtr scheduler, IntPtr func, IntPtr userData, int arrayLength, int innerloopBatchCount, IntPtr completionFunc, JobHandle dependency);

        [DllImport("nativejobs")]
        internal static extern void ScheduleMultiDependencyJob(ref JobHandle fence, IntPtr dispatch, IntPtr dependencies, int fenceCount);

        [DllImport("nativejobs")]
        internal static extern void WaitForJobGroupID(JobHandle groupID);
#endif

        public static unsafe IntPtr CreateJobReflectionData(Type wrapperJobType, Type userJobType, JobType jobType, MulticastDelegate execute)
        {
            throw new NotImplementedException();
        }

        public static IntPtr CreateJobReflectionData(Type type, JobType jobType, object managedJobFunction0,
            object managedJobFunction1 = null, object managedJobFunction2 = null)
        {
            throw new NotImplementedException();
        }

        public static int GetDefaultIndicesPerPhase(int arrayLength)
        {
            return Math.Max((int)Math.Ceiling((double)arrayLength / JobQueueThreadCount), 1);
        }

        // TODO: Currently, the actual work stealing code sits in (big) Unity's native code w/ some dependencies
        //     For now, let's simply split the work for each thread over the number of job threads
        public static bool GetWorkStealingRange(ref JobRanges ranges, int jobIndex, out int begin, out int end)
        {
            begin = jobIndex * ranges.IndicesPerPhase;
            end = Math.Min(begin + ranges.IndicesPerPhase, ranges.ArrayLength);

            return true;
        }

        public static JobHandle ScheduleParallelFor(ref JobScheduleParameters scheduleParams, int i, int minIndicesPerJobCount)
        {
            throw new NotImplementedException();
        }

#if UNITY_SINGLETHREADED_JOBS
        public static unsafe JobHandle ScheduleParallelForDeferArraySize(ref JobScheduleParameters scheduleParams, int innerloopBatchCount, void* getInternalListDataPtrUnchecked, void* atomicSafetyHandlePtr) => throw new NotImplementedException();
#endif

        public class JobScheduleParameters
        {
            public unsafe JobScheduleParameters(void* addressOfPayload, IntPtr jobReflectionData, JobHandle dependsOn, object batched)
            {
            }
        }

        public static JobHandle Schedule(ref JobScheduleParameters parameters) => throw new NotImplementedException();

        public static unsafe void PatchBufferMinMaxRanges(IntPtr bufferRangePatchData, void* jobdata, int startIndex,
            int rangeSize)
        {
        }
    }


    public static class JobHandleUnsafeUtility
    {
        public static unsafe JobHandle CombineDependencies(JobHandle* jobs, int count)
        {
#if UNITY_SINGLETHREADED_JOBS
            return default(JobHandle);
#else
            var fence = new JobHandle();
            JobsUtility.ScheduleMultiDependencyJob(ref fence, JobsUtility.BatchScheduler, new IntPtr(jobs), count);
            return fence;
#endif
        }
    }

    public enum JobType
    {
        Single,
        ParallelFor
    }

    // NOTE: This doesn't match (big) Unity's JobRanges because JobsUtility.GetWorkStealingRange isn't fully implemented
    public struct JobRanges
    {
        public int ArrayLength;
        public int IndicesPerPhase;
    }

    public enum ScheduleMode
    {
        Run,
        Batched
    }

    [AttributeUsage(AttributeTargets.Interface)]
    public sealed class JobProducerTypeAttribute : Attribute
    {
        public JobProducerTypeAttribute(Type producerType) => throw new NotImplementedException();
        public Type ProducerType => throw new NotImplementedException();
    }
}

namespace Unity.Collections
{

    public class NativeContainerIsAtomicWriteOnlyAttribute : Attribute {}
    public class NativeSetThreadIndexAttribute : Attribute {}
    public class ReadOnlyAttribute : Attribute {}
    public class WriteOnlyAttribute : Attribute {}
    public class NativeDisableParallelForRestrictionAttribute : Attribute {}
    public class DeallocateOnJobCompletionAttribute : Attribute {}
    public class WriteAccessRequiredAttribute : Attribute {}
}

namespace Unity.Collections.LowLevel.Unsafe
{
    public sealed class NativeContainerAttribute : Attribute {}
    public class NativeDisableUnsafePtrRestrictionAttribute : Attribute {}
    public sealed class NativeContainerSupportsMinMaxWriteRestriction : Attribute {}
    public class NativeSetClassTypeToNullOnSchedule : Attribute {}
    public class NativeContainerIsReadOnly : Attribute {}
    public sealed class NativeDisableContainerSafetyRestrictionAttribute : Attribute {}
}


namespace UnityEngine.Profiling
{
    public class CustomSampler
    {
        public static CustomSampler Create(string s) => throw new NotImplementedException();
        public void Begin() => throw new NotImplementedException();
        public void End() => throw new NotImplementedException();
    }

    public static class Profiler
    {
        public static void BeginSample(string s)
        {
        }

        public static void EndSample(){}
    }
}

namespace UnityEngine.Scripting
{
    public class PreserveAttribute : Attribute {}
}

namespace UnityEngine
{
    public static class Debug
    {
        internal static string lastLog;
        internal static string lastWarning;
        internal static string lastError;

        public static void LogError(object message)
        {
            if (message == null)
                lastError = "LogError: null (null message, maybe a format which is unsupported?)";
            else if (message is string)
                lastError = (string) message;
            else
                lastError = "LogError: NON-String OBJECT LOGGED";
            Console.WriteLine(lastError);
        }

        public static void LogWarning(string message)
        {
            lastWarning = message;
            Console.WriteLine(message);
        }

        public static void Log(string message)
        {
            lastLog = message;
            Console.WriteLine(message);
        }

        public static void Log(int message) => Log(message.ToString());
        public static void Log(float message) => Log(message.ToString());

        public static void LogException(Exception exception)
        {
            lastLog = "Exception";
            Console.WriteLine(exception.Message + "\n" + exception.StackTrace);
        }
    }


    namespace TestTools
    {
        public static class LogAssert
        {
            public static void Expect(LogType type, string message)
            {
                if (type == LogType.Log) {
                    if (!message.Equals(Debug.lastLog))
                        throw new InvalidOperationException();
                } else if (type == LogType.Warning) {
                    if (!message.Equals(Debug.lastWarning))
                        throw new InvalidOperationException();
                }
            }
#if !NET_DOTS
            public static void Expect(LogType type, Regex message)
            {
                if (type == LogType.Log) {
                    if (!message.Match(Debug.lastLog).Success)
                        throw new InvalidOperationException();
                } else if (type == LogType.Warning) {
                    if (!message.Match(Debug.lastWarning).Success)
                        throw new InvalidOperationException();
                }
            }
#endif
            public static void NoUnexpectedReceived()
            {
            }
        }
    }
}

namespace UnityEngine.Experimental.PlayerLoop
{
    public struct Initialization {}

    public struct Update
    {
        public struct ScriptRunBehaviourUpdate
        {
        }

        public struct ScriptRunDelayedDynamicFrameRate
        {
        }
    }
}

namespace UnityEngine.Experimental.LowLevel
{
    public struct PlayerLoopSystem
    {
        public Type type;
        public PlayerLoopSystem[] subSystemList;
        public UpdateFunction updateDelegate;

        /*
        public IntPtr updateFunction;
        public IntPtr loopConditionFunction;*/
        public delegate void UpdateFunction();
    }

    public static class PlayerLoop
    {
        private static readonly PlayerLoopSystem _default = new PlayerLoopSystem()
        {
            type = typeof(int), subSystemList = new PlayerLoopSystem[1]
            {
                new PlayerLoopSystem()
                {
                    subSystemList = Array.Empty<PlayerLoopSystem>(),
                    type = null,
                    updateDelegate = Nothing
                }
            }, updateDelegate = Tick
        };

        private static void Nothing()
        {
        }

        private static PlayerLoopSystem _current;

        public static PlayerLoopSystem GetDefaultPlayerLoop() => _default;

        public static void Tick()
        {
            ProcessSystem(_current);
        }

        private static void ProcessSystem(PlayerLoopSystem playerLoopSystem)
        {
            playerLoopSystem.updateDelegate?.Invoke();

            foreach (var subSystem in playerLoopSystem.subSystemList ?? Array.Empty<PlayerLoopSystem>())
                ProcessSystem(subSystem);
        }

        public static void SetPlayerLoop(PlayerLoopSystem loop) => _current = loop;
    }
}

//unity.properties has an unused "using UnityEngine.Bindings".
namespace UnityEngine.Bindings
{
    public class Dummy
    {
    }
}

namespace UnityEngine
{
    public class Component {}

    public class Random
    {
        public static void InitState(int state)
        {
        }

        public static int Range(int one, int two)
        {
            return one;
        }
    }

    // The type of the log message in the delegate registered with Application.RegisterLogCallback.
    public enum LogType
    {
        // LogType used for Errors.
        Error = 0,
        // LogType used for Asserts. (These indicate an error inside Unity itself.)
        Assert = 1,
        // LogType used for Warnings.
        Warning = 2,
        // LogType used for regular log messages.
        Log = 3,
        // LogType used for Exceptions.
        Exception = 4
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ExecuteAlwaysAttribute : Attribute
    {
        public ExecuteAlwaysAttribute()
        {
        }
    }

    public static class Time
    {
        [DllImport("lib_unity_zerojobs")]
        public static extern long Time_GetTicksMicrosecondsMonotonic();

        public static float time => Time_GetTicksMicrosecondsMonotonic() / 1_000_000.0f;
    }
}

namespace UnityEngine.Internal
{
    public class ExcludeFromDocsAttribute : Attribute {}
}


namespace Unity.Burst
{
    //why is this not in the burst package!?
    public class BurstDiscardAttribute : Attribute{}
}

namespace UnityEngine.Assertions
{
    public static class Assert
    {
        public static void AreEqual(object one, object two)
        {
        }

        public static void AreNotEqual(object one, object two)
        {
        }

        public static void IsTrue(bool b, string msg = null)
        {
        }

        public static void IsFalse(bool b, string msg = null)
        {
        }

        public static void AreApproximatelyEqual(object one, object two, object three = null, object msg =null)
        {
        }
    }
}

namespace Unity.Profiling
{
    public class ProfilerMarker
    {
        public ProfilerMarker(string s)
        {
        }

        public void Begin()
        {
        }

        public void End()
        {
        }

        class DummyDisposable : IDisposable
        {
            public void Dispose()
            {
            }

        }

        public IDisposable Auto()
        {
            return new DummyDisposable();
        }
    }
}

using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

#if !UNITY_DOTSPLAYER

namespace Unity.Jobs
{
    /// <summary>
    /// A replacement for IJobParallelFor when the number of work items is not known at Schedule time.
    /// IJobParallelForDefer lets you calculate the number of iterations to perform in a job that must execute before the IJobParallelForDefer job.
    /// 
    /// When Scheduling the job's Execute(int index) method will be invoked on multiple worker threads in parallel to each other.
    /// Execute(int index) will be executed once for each index from 0 to the provided length. Each iteration must be independent from other iterations (The safety system enforces this rule for you). The indices have no guaranteed order and are executed on multiple cores in parallel.
    /// Unity automatically splits the work into chunks of no less than the provided batchSize, and schedules an appropriate number of jobs based on the number of worker threads, the length of the array and the batch size.
    /// Batch size should generally be chosen depending on the amount of work performed in the job. A simple job, for example adding a couple of float3 to each other should probably have a batch size of 32 to 128. However if the work performed is very expensive then it is best to use a small batch size, for expensive work a batch size of 1 is totally fine. IJobParallelFor performs work stealing using atomic operations. Batch sizes can be small but they are not for free.
    /// The returned JobHandle can be used to ensure that the job has completed. Or it can be passed to other jobs as a dependency, thus ensuring the jobs are executed one after another on the worker threads.
    /// </summary>
    [JobProducerType(typeof (IJobParallelForDeferExtensions.JobStructDefer<>))]
    public interface IJobParallelForDefer
    {
        /// <summary>
        /// Implement this method to perform work against a specific iteration index.
        /// </summary>
        /// <param name="index">The index of the Parallel for loop at which to perform work.</param>
        void Execute(int index);
    }
    
    public static class IJobParallelForDeferExtensions
    {
        internal struct JobStructDefer<T> where T: struct, IJobParallelForDefer
        {
            public static IntPtr jobReflectionData;

            public static IntPtr Initialize()
            {
                if (jobReflectionData == IntPtr.Zero)
                    jobReflectionData = JobsUtility.CreateJobReflectionData(typeof(T), typeof(T), JobType.ParallelFor, (ExecuteJobFunction)Execute);

                return jobReflectionData;
            }

            public delegate void ExecuteJobFunction(ref T data, System.IntPtr additionalPtr, System.IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex);

            public unsafe static void Execute(ref T self, System.IntPtr additionalPtr, System.IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex)
            {
                while (true)
                {
                    int begin;
                    int end;
                    if (!JobsUtility.GetWorkStealingRange(ref ranges, jobIndex, out begin, out end))
                        break;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    JobsUtility.PatchBufferMinMaxRanges(bufferRangePatchData, UnsafeUtility.AddressOf(ref self), begin, end - begin);
#endif

                    for (var i = begin; i < end; ++i)
                        self.Execute(i);
                }

            }
        }

        
        /// <summary>
        /// Schedule the job for execution on worker threads.
        /// list.Length is used as the iteration count.
        /// Note that it is required to embed the list on the job struct as well. 
        /// </summary>
        /// <param name="jobData">The job and data to schedule.</param>
        /// <param name="list">list.Length is used as the iteration count.</param>
        /// <param name="innerloopBatchCount">Granularity in which workstealing is performed. A value of 32, means the job queue will steal 32 iterations and then perform them in an efficient inner loop.</param>
        /// <param name="dependsOn">Dependencies are used to ensure that a job executes on workerthreads after the dependency has completed execution. Making sure that two jobs reading or writing to same data do not run in parallel.</param>
        /// <returns>JobHandle The handle identifying the scheduled job. Can be used as a dependency for a later job or ensure completion on the main thread.</returns>
        unsafe public static JobHandle Schedule<T, U>(this T jobData, NativeList<U> list, int innerloopBatchCount, JobHandle dependsOn = new JobHandle()) 
            where T : struct, IJobParallelForDefer 
            where U : struct
        {
            var scheduleParams = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref jobData), JobStructDefer<T>.Initialize(), dependsOn, ScheduleMode.Batched);

            void* atomicSafetyHandlePtr = null;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var safety = NativeListUnsafeUtility.GetAtomicSafetyHandle(ref list);
            atomicSafetyHandlePtr = UnsafeUtility.AddressOf(ref safety);
#endif
            return JobsUtility.ScheduleParallelForDeferArraySize(ref scheduleParams, innerloopBatchCount, NativeListUnsafeUtility.GetInternalListDataPtrUnchecked(ref list), atomicSafetyHandlePtr);
        }
        
        /// <summary>
        /// Schedule the job for execution on worker threads.
        /// forEachCount is a pointer to the number of iterations, when dependsOn has completed.
        /// This API is unsafe, it is recommended to use the NativeList based Schedule method instead.
        /// </summary>
        /// <param name="jobData"></param>
        /// <param name="forEachCount"></param>
        /// <param name="innerloopBatchCount"></param>
        /// <param name="dependsOn"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        unsafe public static JobHandle Schedule<T>(this T jobData, int* forEachCount, int innerloopBatchCount, JobHandle dependsOn = new JobHandle()) 
            where T : struct, IJobParallelForDefer 
        {
            var scheduleParams = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref jobData), JobStructDefer<T>.Initialize(), dependsOn, ScheduleMode.Batched);
            var forEachListPtr = (byte*)forEachCount - sizeof(void*);
            return JobsUtility.ScheduleParallelForDeferArraySize(ref scheduleParams, innerloopBatchCount, forEachListPtr, null);
        }
    }
}

#endif
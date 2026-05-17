using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Box2dNet.Interop
{
    public partial class B2Api
    {
        /// <summary>
        /// Returns b2DefaultWorldDef() but with the .NET Task Parallel Library preconfigured, so Box2D runs multi-threaded.
        /// </summary>
        public static b2WorldDef b2DefaultWorldDef_WithDotNetTpl(int? processorCount = null)
        {
            // Box2D code comments claims using hyperthreading and 'efficiency cores' (Intel)
            // doesn't add much performance because of the shared cache,
            // so we take an educated guess that physical CPUs on most modern machines = logical processor count / 2;
            // (performance tests show that using Environment.ProcessorCount/2 is indeed much faster than Environment.ProcessorCount)

            var workerCount = processorCount.HasValue
                ? int.Min(processorCount.Value, Environment.ProcessorCount)
                : Environment.ProcessorCount / 2;

            var worldDef = b2DefaultWorldDef();
            worldDef.enqueueTask = Marshal.GetFunctionPointerForDelegate((b2EnqueueTaskCallback)EnqueueTaskCallback);
            worldDef.finishTask = Marshal.GetFunctionPointerForDelegate((b2FinishTaskCallback)FinishTaskCallback);
            worldDef.workerCount = workerCount;
            worldDef.userTaskContext = workerCount; // abusing this pointer to pass workerCount to our task scheduling impl.
            return worldDef;
        }

        private static IntPtr EnqueueTaskCallback(
            IntPtr /* b2TaskCallback */ task,
            IntPtr taskContext,
            IntPtr userContext)
        {
            var taskCallback = Marshal.GetDelegateForFunctionPointer<b2TaskCallback>(task);

            // Run this single task on a worker thread. Box2D handles all range
            // splitting and worker-index assignment internally; we just need to
            // execute the callback off the calling thread and return a waitable handle.
            var t = Task.Run(() => taskCallback.Invoke(taskContext));

            return NativeHandle.Alloc(t);
        }

        private static void FinishTaskCallback(IntPtr userTask, IntPtr userContext)
        {
            var handle = GCHandle.FromIntPtr(userTask);
            var task = (Task)handle.Target!;
            task.Wait();
            NativeHandle.Free(userTask);
        }

        // BELOW is the old implementation pre 2026/04 kept for reference. Box2D used to expect the application to 
        // distribute a number of tasks over multiple threads (= parallel for). But it now calculates this distribution
        // itself and only asks the application to launch each single task it gives us on a new background thread.
        // I think those opaque tasks now contain the ranges of tasks for the one thread, which we used to calculate here.

        //private static void FinishTaskCallback(IntPtr userTaskIntPtr, IntPtr userContext)
        //{
        //    // Box2D calls this when it wants our parallelTask to be finished, so we block the thread to wait for it here.
        //    var parallelTask = NativeHandle.GetObject<Task>(userTaskIntPtr);
        //    parallelTask.Wait();
        //    NativeHandle.Free(userTaskIntPtr);
        //}

        //private static IntPtr EnqueueTaskCallback(IntPtr /* b2TaskCallback */ task, int itemCount, int minRange, IntPtr taskContext, IntPtr userContext)
        //{
        //    var maxWorkerCount = (int)userContext; // set to workerCount by b2DefaultWorldDef_WithDotNetTpl()

        //    var taskCallback = Marshal.GetDelegateForFunctionPointer<b2TaskCallback>(task);

        //    // split the work in equal partitions of size 'minRange'
        //    var partitionCount = itemCount / minRange;
        //    if (itemCount % minRange > 0) partitionCount++;

        //    // schedule 1 concurrent task per partition, with a worker-pool with an exact size of 'maxWorkerCount'.
        //    var parallelTask = ParallelForWithMaxWorkerControl(partitionCount, maxWorkerCount, (partitionIdx, workerId) =>
        //    {
        //        var startIndex = partitionIdx * minRange;
        //        var endIndexExclusive = int.Min(startIndex + minRange, itemCount);
        //        taskCallback.Invoke(startIndex, endIndexExclusive, (uint)workerId, taskContext);
        //        return Task.CompletedTask;
        //    });

        //    return NativeHandle.Alloc(parallelTask);
        //}

        ///// <summary>
        ///// Like a Parallel.For but supporting the Box2D requirements of maxWorkerCount cap, and each worker knowing it's unique worker ID.
        ///// </summary>
        ///// <returns></returns>
        //private static async Task ParallelForWithMaxWorkerControl(int toExclusive, int maxWorkerCount, Func<int, int, Task> processItemFunc)
        //{
        //    // Box2D want to know an ID of the worker doing a certain part of the job to partition the internal data for that work.
        //    // To my knowledge, .NET TPL does not offer this ability (to know the nr of the worker doing a certain task)

        //    // So, we use our own system that supports these requirements:

        //    // We create a thread-safe 'queue' (BlockingCollection) with exactly 'maxWorkerCount' concurrent workers listening to that queue.
        //    // Each worker knows it's numerical ID, and can pass it to Box2D.

        //    var collection = new BlockingCollection<int>();
        //    var tasks = new Task[maxWorkerCount];

        //    // enqueue all work
        //    for (var i = 0; i < toExclusive; i++)
        //    {
        //        collection.Add(i);
        //    }
        //    collection.CompleteAdding();

        //    // run 'maxWorkerCount' workers that compete for processing the queued work.
        //    for (var workerId = 0; workerId < maxWorkerCount; workerId++)
        //    {
        //        var currentWorkerId = workerId; // Capture workerId for closure
        //        tasks[workerId] = Task.Run(async () =>
        //        {
        //            foreach (var i in collection.GetConsumingEnumerable())
        //            {
        //                await processItemFunc(i, currentWorkerId);
        //            }
        //        });
        //    }

        //    await Task.WhenAll(tasks)
        //        .ConfigureAwait(false); // prevents deadlock on Godot
        //}
    }
}

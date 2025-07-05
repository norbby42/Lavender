using System;
using System.Collections.Generic;
using System.Text;

namespace Lavender.TaskLib
{
    // TaskStatusInfo is only used in TaskInfo.requiredActivateTasks
    // This wrapper is tailored to that usecase
    // When the resolver is wrapped, it temporarily replaces the task reference in the TaskStatusInfo instance with a reference to itself
    //  This ties its lifespan to the TaskStatusInfo object that is using it.  If that object cleans up early, then we will also clean up early
    public class TaskStatusInfoResolverWrapper : TaskInfo
    {
        private TaskStatusInfo TaskStatusInfo;
        private TaskResolver<TaskInfo> Resolver;
        private TaskInfo OwningTask;

        private TaskStatusInfoResolverWrapper(TaskStatusInfo taskStatusInfo, TaskResolver<TaskInfo> taskResolver, TaskInfo owningTask)
        {
            TaskStatusInfo = taskStatusInfo;
            Resolver = taskResolver;
            OwningTask = owningTask;

            Resolver.OnFound += TaskFound;
            Resolver.OnNotFound += TaskNotFound;
        }

        ~TaskStatusInfoResolverWrapper()
        {
            UnregisterEvents();
        }

        public static void Wrap(TaskStatusInfo taskStatusInfo, TaskResolver<TaskInfo> taskResolver, TaskInfo owningTask)
        {
            if (taskResolver.Running() || taskResolver.Done())
            {
                throw new ArgumentException($"Cannot wrap: TaskResolver {taskResolver} for task {taskResolver.Id} is already started.");
            }
            TaskStatusInfoResolverWrapper wrapper = new TaskStatusInfoResolverWrapper(taskStatusInfo, taskResolver, owningTask);
            taskStatusInfo.task = wrapper;
            taskResolver.Start();
        }

        private void Cleanup()
        {
            UnregisterEvents();
            OwningTask = null!;
            TaskStatusInfo = null!;
        }

        private void UnregisterEvents()
        {
            if (Resolver != null)
            {
                Resolver.OnFound -= TaskFound;
                Resolver.OnNotFound -= TaskNotFound;
                Resolver = null!;
            }
        }

        private void TaskFound(string id, TaskInfo taskInfo)
        {
            if (TaskStatusInfo != null && OwningTask != null && TaskStatusInfo.task == this)
            {
                // Until the resolver has finished, the task variable should be pointing at *this*
                // So we only update it if it still is.  If it isn't, then external code changed it and we shouldn't be interfering any more
                TaskStatusInfo.task = taskInfo;
            }

            // We're done.  Time to get cleaned up
            Cleanup();
        }

        private void TaskNotFound(string id)
        {
            if (TaskStatusInfo != null && OwningTask != null && TaskStatusInfo.task == this)
            {
                // Resolver failed and the data still is pointing at *this* as the task.
                // Remove the TaskStatusInfo entry entirely - it cannot be valid.  Log this.
                OwningTask.requiredActivateTasks.Remove(TaskStatusInfo);
                LavenderLog.Detailed($"Failed to resolve task id {id} for requiredActivateTasks on task {OwningTask.id}.  Removing this criteria.");
            }

            Cleanup();
        }
    }
}

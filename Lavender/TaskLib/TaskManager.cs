using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lavender.TaskLib
{
    public class TaskManager
    {
        private static TaskManager _instance = null!;

        public static TaskManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TaskManager();
                }
                return _instance;
            }
        }

        private TaskManager()
        {

        }

        public Dictionary<string, TaskInfo> Tasks = new Dictionary<string, TaskInfo>();
        public Dictionary<string, TaskObjective> Objectives = new Dictionary<string, TaskObjective>();


        public TaskInfo? GetLoadedTask(string id)
        {
            // If it's a modded task, easy to find
            if (Tasks.TryGetValue(id, out TaskInfo taskInfo))
            {
                return taskInfo;
            }

            // Okay, so not a modded task (or the mod providing it has not loaded yet)
            // If the TaskController is loaded, check it and see if it's one of the loaded tasks
            if (TaskController.instance != null)
            {
                TaskInfo info = TaskController.instance.allTasks.Where(status => status.task != null && status.task.id == id).Select(status => status.task).First();
                if (info != null)
                {
                    return info;
                }
            }

            // Alright, so at this point we're in "non-ideal" territory.
            // There are a few possibilities:
            // 1) The requested task does not exist (maybe due to game update or removed mod)
            // 2) The requested task exists, and the character even has it active, but the savegame hasn't loaded (probable on initial registration, we'll be on main menu)
            // 3) The requested task exists and we have an addressableAssetPath or assetPath pointing at it.  However, loading the asset right now would be a blocking I/O operation
            // 4) The requested task exists and we have no hints about its asset location
            // And possibly others I haven't considered
            //
            // I would like to try some sort of "unresolved reference" - where we pass back a placeholder object.  If we have access to hints, we can try to initiate async loads
            // and if those are successful then use the loaded assets.  Otherwise, try to re-resolve the reference just before the task is attempted to be accessed.
            // This might be overly complicated...

            // New functions to return deferred resolution handler if resolution is unsuccessful right this moment: TryFindTask/TryFindObjective

            // Deferred resolution handlers have this behavior:
            //  If asset hints are provided, start async load attempts immediately.  If those succeed, then fire "found" callbacks and update references
            //  When SaveController OnSaveLoaded callback fires, search TaskController again.  If found, fire "found" callbacks etc
            //  If both of the above have happened and neither found the task, then fire "notfound" callbacks
            // Try to keep the lifespan of the TaskResolver as short as possible while still covering all relevant events

            return null;
        }

        public TaskResolver<TaskInfo> TryFindTask(string id, string addressableAssetPath = "")
        {
            return new TaskResolver<TaskInfo>(id, addressableAssetPath);
        }

        public TaskObjective? GetLoadedObjective(string id)
        {
            throw new NotImplementedException();
        }
    }
}

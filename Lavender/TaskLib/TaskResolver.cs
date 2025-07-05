using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

namespace Lavender.TaskLib
{
    public class TaskResolver<T>
    {
        private enum ResolverState
        {
            Pending,
            Running,
            Done
        }

        public string Id { get; private set; }
        private string AddressableName = "";
        private AsyncOperationHandle<T> AddressableHandle;
        private T? ResolvedAsset;
        private ResolverState State = ResolverState.Pending;

        public delegate void Found(string Id, T TaskInfo);
        public delegate void NotFound(string Id);

        public event Found? OnFound;
        public event NotFound? OnNotFound;

        internal TaskResolver(string id, string addressable)
        {
            this.Id = id;
            this.AddressableName = addressable;
        }

        ~TaskResolver()
        {
            if (State == ResolverState.Running)
            {
                Cleanup(); // Clean up bindings
            }
        }

        // Call after binding to events
        public void Start()
        {
            if (State != ResolverState.Pending)
            {
                return;
            }
            State = ResolverState.Running;

            // If addressable is set, try to load addressable async
            if (!string.IsNullOrEmpty(AddressableName))
            {
                AddressableHandle = Addressables.LoadAssetAsync<T>(AddressableName);
                AddressableHandle.Completed += OnAddressableFound;
            }

            // Bind into SaveController.LoadingDone.  When it fires, we are at the last moment at which we can still successfully resolve.
            // Either the asset needs to be loaded and known, or we block waiting on addressables (if it knows what the asset is) *or* we have to give up and say it can't be found

            SaveController.LoadingDone += LoadingDone;
        }

        public bool Running()
        {
            return State == ResolverState.Running;
        }

        public bool Done()
        {
            return State == ResolverState.Done;
        }

        private void OnAddressableFound(AsyncOperationHandle<T> handle)
        {
            if (State != ResolverState.Running)
            {
                return;
            }

            ResolvedAsset = handle.Result;

            if (ResolvedAsset != null)
            {
                State = ResolverState.Done;
                Report();
            }
            else
            {
                // We got a null back from Addressables?!?  Ooookay then....
                if (Lavender.instance.LoadingDone)
                {
                    // Only give up if we've passed the SaveController.LoadingDone gate
                    State = ResolverState.Done;
                    Report();
                    
                }
            }
        }

        private void LoadingDone()
        {
            if (State != ResolverState.Running)
            {
                return;
            }

            // Weird hack to route execution to the correct TaskManager lookup
            if (typeof(T) == typeof(TaskInfo))
            {
                TaskInfo? task = TaskManager.Instance.GetLoadedTask(Id);
                if (task != null)
                {
                    // If this was C++ I would have a worker function that has different definitions for TaskInfo and TaskObjective templates
                    // Unfortunately, generics don't work that way in C# and I don't know it well enough to figure out a better way xD
                    ResolvedAsset = (T)(Object)task;
                }
            }
            else if (typeof(T) == typeof(TaskObjective))
            {
                TaskObjective? obj = TaskManager.Instance.GetLoadedObjective(Id);
                if (obj != null)
                {
                    ResolvedAsset = (T)(Object)obj;
                }
            }

            if (ResolvedAsset == null && AddressableHandle.IsValid())
            {
                AddressableHandle.Completed -= OnAddressableFound; // Unbind delegate, we're going to get the result asset here
                ResolvedAsset = AddressableHandle.WaitForCompletion(); // Blocking op, but the SaveController *also* does this when loading so it's acceptable
            }

            State = ResolverState.Done;
            Report();
        }

        private void Cleanup()
        {
            if (AddressableHandle.IsValid())
            {
                AddressableHandle.Completed -= OnAddressableFound;
            }

            SaveController.LoadingDone -= LoadingDone;
        }

        private void Report()
        {
            Cleanup();

            if (ResolvedAsset != null)
            {
                OnFound?.Invoke(Id, ResolvedAsset);
            }
            else
            {
                OnNotFound?.Invoke(Id);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Lavender.TaskLib
{
    public class TaskXMLParser
    {
        private string SourceName;
        private XElement DataRoot;
        private List<Tuple<TaskInfo, XElement>> TasksPendingSecondPass = new List<Tuple<TaskInfo, XElement>>();
        private List<Tuple<TaskObjective, XElement>> ObjectivesPendingSecondPass = new List<Tuple<TaskObjective, XElement>>();

        public Dictionary<string, TaskInfo> Tasks = new Dictionary<string, TaskInfo>();
        public Dictionary<string, TaskObjective> Objectives = new Dictionary<string, TaskObjective>();
        
        public TaskXMLParser(string dataSource)
        {
            DataRoot = XElement.Load(dataSource);
            SourceName = dataSource;
        }

        public bool IsValid()
        {
            return DataRoot != null && DataRoot.Element("Task") != null;
        }

        public void Process()
        {
            // We parse the xml file in 2 passes:
            // First pass is to load the definitions and internally stable data (ie id, title, description, etc)
            // Second pass then resolves inter-objective/inter-task links
            foreach (XElement xTask in DataRoot.Elements("task"))
            {
                TaskInfo taskInfo = ParseTaskStage1(xTask);

                if (taskInfo != null)
                {
                    IEnumerable<XElement> xObjectives = xTask.Elements("objective");
                    if (xObjectives.Any())
                    {
                        TasksPendingSecondPass.Add(Tuple.Create(taskInfo, xTask));
                        Tasks.Add(taskInfo.id, taskInfo);
                        foreach (XElement xObj in xObjectives)
                        {
                            TaskObjective? taskObj = ParseObjectiveStage1(xObj, taskInfo.id);
                            if (taskObj != null)
                            {
                                if (Objectives.ContainsKey(taskObj.id))
                                {
                                    LavenderLog.Error($"Objective {taskObj.id} in task {taskInfo.id} in {SourceName} has multiple definitions.  Objective names must be unique.");
                                }
                                else
                                {
                                    ObjectivesPendingSecondPass.Add(Tuple.Create(taskObj, xObj));
                                    Objectives.Add(taskObj.id, taskObj);
                                }
                            }
                        }
                    }
                    else
                    {
                        LavenderLog.Error($"Task {taskInfo.id} in {SourceName} has no <objective> elements.  A task requires at least 1 objective.");
                    }
                }
            }

            // 2nd pass: resolve links between elements
            foreach (var tTask in TasksPendingSecondPass)
            {
                ParseTaskStage2(tTask.Item1, tTask.Item2);
            }
            TasksPendingSecondPass.Clear();


            foreach (var tObjective in ObjectivesPendingSecondPass)
            {
                ParseObjectiveStage2(tObjective.Item1, tObjective.Item2);
            }
            ObjectivesPendingSecondPass.Clear();

        }

        private TaskInfo ParseTaskStage1(XElement xml)
        {
            TaskInfo result = new TaskInfo();
            if ((result.id = (string?)xml.Element("id")) == null)
            {
                LavenderLog.Error($"TaskXMLParser: File {SourceName} task element without id.  Ignoring.");
                return null!;
            }
            result.title = (string?)xml.Element("title") ?? "<title> tag missing";
            result.description = (string?)xml.Element("description") ?? "<description> tag missing";
            result.finishTaskWhenTheresNoActiveTasks = (bool)xml.Element("finishTaskWhenTheresNoActiveTasks");

            return result;
        }

        private void ParseTaskStage2(TaskInfo Task, XElement xml)
        {
            XElement requiredTasks = xml.Element("requiredActivateTasks");
            if (requiredTasks != null)
            {
                // Parse all the <taskid> child elements into task status objects, and build list from them
                Task.requiredActivateTasks = requiredTasks.Elements("taskstatusinfo").Select(x => ParseTaskStatusInfo(x, Task)).Where(x => x != null).ToList();
            }
            else
            {
                Task.requiredActivateTasks = new List<TaskStatusInfo>(); // Yeaaaah, so this field has no default initializer and we don't want to crash in vanilla code.
            }

            XElement firstObjectives = xml.Element("firstObjectives");
            if (firstObjectives != null)
            {
                Task.firstObjectives = firstObjectives.Elements("objectivereference").Select(ParseTaskObjectiveReference).Where(x => x != null).ToList();
            }
        }

        private TaskObjective ParseObjectiveStage1(XElement xml, string TaskId)
        {
            TaskObjective result = new TaskObjective();
            if ((result.id = (string?)xml.Element("id")) == null)
            {
                LavenderLog.Error($"TaskXMLParser: File {SourceName} objective element in task {TaskId} without id.  Ignoring.");
                return null!;
            }
            result.title = (string?)xml.Element("title") ?? "<title> tag missing";
            result.description = (string?)xml.Element("description") ?? "<description> tag missing";
            result.counterAmount = (int)xml.Element("counterAmount");


            return result;
        }

        private void ParseObjectiveStage2(TaskObjective Objective, XElement xml)
        {
            XElement onFinishedObjectives = xml.Element("onFinishedObjectives");
            if (onFinishedObjectives != null)
            {
                Objective.onFinishedObjectives = onFinishedObjectives.Elements("objectivereference").Select(ParseTaskObjectiveReference).Where(x => x != null).ToList();
            }

            XElement onAbandonedObjectives = xml.Element("onAbandonedObjectives");
            if (onAbandonedObjectives != null)
            {
                Objective.onAbandonedObjectives = onAbandonedObjectives.Elements("objectivereference").Select(ParseTaskObjectiveReference).Where(x => x != null).ToList();
            }

            XElement onFailedObjectives = xml.Element("onFailedObjectives");
            if (onFailedObjectives != null)
            {
                Objective.onFailedObjectives = onFailedObjectives.Elements("objectivereference").Select(ParseTaskObjectiveReference).Where(x => x != null).ToList();
            }

            XElement requiredActivateObjectives = xml.Element("requiredActivateObjectives");
            if (requiredActivateObjectives != null)
            {
                Objective.requiredActivateObjectives = requiredActivateObjectives.Elements("objectivestatusinfo").Select(ParseObjectiveStatusInfo).Where(x => x != null).ToList();
            }
            else
            {
                // This property doesn't have a default initializer, so we init it here to ensure no crashes
                Objective.requiredActivateObjectives = new List<ObjectiveStatusInfo>();
            }

            XElement requiredCompleteObjectives = xml.Element("requiredCompleteObjectives");
            if (requiredCompleteObjectives != null)
            {
                Objective.requiredCompleteObjectives = requiredCompleteObjectives.Elements("objectivestatusinfo").Select(ParseObjectiveStatusInfo).Where(x => x != null).ToList();
            }
            else
            {
                // This property doesn't have a default initializer, so we init it here to ensure no crashes
                Objective.requiredCompleteObjectives = new List<ObjectiveStatusInfo>();
            }
        }

        private TaskStatusInfo ParseTaskStatusInfo(XElement xml, TaskInfo owningTask)
        {
            TaskResolver<TaskInfo> resolver = null!;

            TaskInfo? taskInfo = null;
            string? taskId = (string?)xml.Element("id");
            if (taskId != null)
            {
                if (!Tasks.TryGetValue(taskId, out taskInfo))
                {
                    taskInfo = TaskManager.Instance.GetLoadedTask(taskId);
                }

                // We don't *currently* know what the task is.  This doesn't mean it's invalid, just that it's not loaded and somewhere we can find it
                // Setup a resolver to handle deferred resolution.
                if (taskInfo == null)
                {
                    XAttribute addressableName = xml.Attribute("addressable");
                    resolver = TaskManager.Instance.TryFindTask(taskId, addressableName != null ? (string)addressableName : "");
                }
            }

            TaskStatusInfo result = new TaskStatusInfo(taskInfo, (CurrentTaskStatus)Enum.Parse(typeof(CurrentTaskStatus), (string?)xml.Element("status") ?? "AnyStatus", true));

            // Setup the TaskStatusInfo resolver wrapper.  When the resolver finds the referenced task, this wrapper will update the TaskStatusInfo instance with it.
            // Or remove the TaskStatusInfo from owningTask's List if no matching Task could be found (and inform in the log)
            // This is fire and forget, and will resolve into valid data before gameplay begins (sometime during the "LoadingDone" event)
            if (resolver != null)
            {
                TaskStatusInfoResolverWrapper.Wrap(result, resolver, owningTask);
            }

            XElement objectives = xml.Element("objectives");
            if (objectives != null)
            {
                result.objectives = objectives.Elements("objectivestatusinfo").Select(ParseObjectiveStatusInfo).Where(x => x != null).ToList();
            }
            else
            {
                // This field doesn't have an initializing default so we'll init here just to be safe
                result.objectives = new List<ObjectiveStatusInfo>();
            }

            return result;
        }

        // Unlike TaskStatusInfo and ObjectiveStatusInfo, this field's xml node' has as' name that doesn't match its C# class
        // This is because its name is super unwieldy.  In XML we use "objectivereference"
        private TaskObjectiveReference ParseTaskObjectiveReference(XElement xml)
        {
            string? objectiveName = (string?)xml;

            if (objectiveName == null)
            {
                LavenderLog.Error($"Failed to parse objectivereference node contents (it doesn't have any?) in {SourceName}");
                return null!;
            }

            TaskObjective? objective = null;

            if (!Objectives.TryGetValue(objectiveName, out objective))
            {
                objective = TaskManager.Instance.GetLoadedObjective(objectiveName);
            }

            if (objective == null)
            {
                LavenderLog.Error($"Failed to find objective with id {objectiveName} in {SourceName} for objectivereference node.");
                return null!;
            }

            XAttribute? optional = xml.Attribute("optional");

            TaskObjectiveReference objectiveRef = new TaskObjectiveReference(objective, optional != null ? (bool)optional : false);

            return objectiveRef;
        }

        private ObjectiveStatusInfo ParseObjectiveStatusInfo(XElement xml) 
        {
            string? objectiveName = (string?)xml;

            if (objectiveName == null)
            {
                LavenderLog.Error($"Failed to parse objectivestatusinfo node contents (it doesn't have any?) in {SourceName}");
                return null!;
            }

            TaskObjective? objective = null;

            if (!Objectives.TryGetValue(objectiveName, out objective))
            {
                //XAttribute? assetAttr = xml.Attribute("addressable"); // Addressable asset path?
                objective = TaskManager.Instance.GetLoadedObjective(objectiveName);//, assetAttr != null ? (string)assetAttr : null);
            }

            if (objective == null)
            {
                LavenderLog.Error($"Failed to find objective with id {objectiveName} in {SourceName} for objectivestatusinfo node.");
                return null!;
            }

            XAttribute? statusAttr = xml.Attribute("status");
            ObjectiveStatus status = ObjectiveStatus.AnyStatus;
            if (statusAttr != null)
            {
                status = (ObjectiveStatus)Enum.Parse(typeof(CurrentTaskStatus), (string?)statusAttr ?? "AnyStatus", true);
            }

            ObjectiveStatusInfo objectiveStatus = new ObjectiveStatusInfo(objective, status, false, 0);

            return objectiveStatus;
        }

    }
}

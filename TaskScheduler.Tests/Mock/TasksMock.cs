using TaskScheduler.Models;

namespace TaskScheduler.Tests.Mock;

public class TasksMock 
{
    private static List<TaskModel> Tasks { get; set; } = 
    [
        new TaskModel{
            Id = "6746407b597a5a4dc9ca1eae",
            TaskName = "task1",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now,
            Priority = 2,
            IsCompleted = false
        },
        new TaskModel{
            Id = "r4",
            TaskName = "Clean the yard",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now,
            Priority = 3,
            IsCompleted = true
        },
        new TaskModel{
            Id = "qwertyuiop",
            TaskName = "This task will be deleted",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now,
            Priority = 4,
            IsCompleted = true
        }
    ];

    public static List<TaskModel> Get()
    {
        return Tasks;
    }

    public static TaskModel? GetById(string id)
    {
        TaskModel? task = (from t in Tasks
                   where t.Id == id
                   select t).SingleOrDefault();
        return task;
    }

    public static Task Create(TaskModel newTask)
    {
        Tasks.Add(newTask);
        return Task.CompletedTask;
    }
    public static Task Update(string id, TaskModel updatedTask)
    {
        TaskModel task = (from t in Tasks
                           where t.Id == id
                           select t).Single();

        task.TaskName = updatedTask.TaskName;
        task.StartDate = updatedTask.StartDate;
        task.EndDate = updatedTask.EndDate;
        task.Priority = updatedTask.Priority;
        task.IsCompleted = updatedTask.IsCompleted;

        return Task.CompletedTask;
    }

    public static Task Remove(string id) {

        TaskModel task = (from t in Tasks
                          where t.Id == id
                          select t).Single();

        Tasks.Remove(task);

        return Task.CompletedTask;
    }
}
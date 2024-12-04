using TaskScheduler.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace TaskScheduler.Services;

public interface ITasksService
{
    Task<List<TaskModel>> GetAsync();
    Task<TaskModel?> GetAsync(string id);
    Task CreateAsync(TaskModel newTaskModel);
    Task UpdateAsync(string id, TaskModel updatedTaskModel);
    Task RemoveAsync(string id);

}

public class TasksService : ITasksService
{
    private readonly IMongoCollection<TaskModel> _tasksCollection;

    public TasksService(
        IOptions<TaskSchedulerDatabaseSettings> taskSchedulerDatabaseSettings)
    {
        var mongoClient = new MongoClient(
            taskSchedulerDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            taskSchedulerDatabaseSettings.Value.DatabaseName);

        _tasksCollection = mongoDatabase.GetCollection<TaskModel>(
            taskSchedulerDatabaseSettings.Value.TasksCollectionName);
    }

    public virtual async Task<List<TaskModel>> GetAsync() =>
        await _tasksCollection.Find(_ => true).ToListAsync();

    public virtual async Task<TaskModel?> GetAsync(string id) =>
        await _tasksCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

    public virtual async Task CreateAsync(TaskModel newTask) =>
        await _tasksCollection.InsertOneAsync(newTask);

    public virtual async Task UpdateAsync(string id, TaskModel updatedTask) =>
        await _tasksCollection.ReplaceOneAsync(x => x.Id == id, updatedTask);

    public virtual async Task RemoveAsync(string id) =>
        await _tasksCollection.DeleteOneAsync(x => x.Id == id);
}
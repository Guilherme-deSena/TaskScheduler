using Microsoft.Extensions.Options;
using TaskScheduler.Models;
using TaskScheduler.Controllers;
using TaskScheduler.Services;
using TaskScheduler.Tests.Mock;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TaskScheduler.Tests;

public class TasksControllerTest
{
    private readonly Mock<IOptions<TaskSchedulerDatabaseSettings>> _mockOptions;
    //private readonly Mock<TasksService> _mockTasksService;

    public TasksControllerTest()
    {
        _mockOptions = new();
        _mockOptions.Setup(_ => _.Value).Returns(new TaskSchedulerDatabaseSettings
        {
            ConnectionString = "mongodb://localhost:27017",
            DatabaseName = "TaskScheduler",
            TasksCollectionName = "Tasks"
        }); 
    }

    [Fact]
    public async void Get_ReturnsAllTasks()
    {
        Mock<TasksService>  _mockTasksService = new(_mockOptions.Object);
        _mockTasksService.Setup(_ => _.GetAsync()).ReturnsAsync(TasksMock.Get());

        TasksController _tasksController = new(_mockTasksService.Object);

        List<TaskModel> result = await _tasksController.Get();

        Assert.NotNull(result);
        Assert.Equal(TasksMock.Get().Count, result.Count);
        _mockTasksService.Verify(s => s.GetAsync(), Times.Once);
    }

    [Theory]
    [InlineData("6746407b597a5a4dc9ca1eae")]
    [InlineData("r4")]
    public async Task Get_ReturnsTask_WhenIdIsValid(string value)
    {
        Mock<TasksService> _mockTasksService = new(_mockOptions.Object);
        _mockTasksService.Setup(_ => _.GetAsync(value)).ReturnsAsync(TasksMock.GetById(value));

        TasksController _tasksController = new(_mockTasksService.Object);

        var result = await _tasksController.Get(value);

        Assert.NotNull(result.Value);
        Assert.Equal(TasksMock.GetById(value)!, result.Value);
        _mockTasksService.Verify(s => s.GetAsync(value), Times.Once);
    }

    [Theory]
    [InlineData("6746407b597")]
    [InlineData("This ID does not exist in the database")]
    public async Task Get_ReturnsNotFound_WhenIdIsNotValid(string value)
    {
        Mock<TasksService> _mockTasksService = new(_mockOptions.Object);
        _mockTasksService.Setup(_ => _.GetAsync(value)).ReturnsAsync(TasksMock.GetById(value));

        TasksController _tasksController = new(_mockTasksService.Object);

        var result = await _tasksController.Get(value);

        Assert.IsType<NotFoundResult>(result.Result);
        _mockTasksService.Verify(s => s.GetAsync(value), Times.Once);
    }

    [Fact]
    public async Task Post_AddsTask()
    {
        Mock<TasksService> _mockTasksService = new(_mockOptions.Object);
        TaskModel newTask = new()
        {
            Id = "0987654321qwerty",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now,
            Priority = 6,
            IsCompleted = false
        };

        _mockTasksService.Setup(_ => _.CreateAsync(newTask)).Returns(TasksMock.Create(newTask));

        TasksController _tasksController = new(_mockTasksService.Object);

        IActionResult result = await _tasksController.Post(newTask);

        Assert.NotNull(result);
        Assert.Equal(TasksMock.GetById(newTask.Id), newTask);
        _mockTasksService.Verify(s => s.CreateAsync(newTask), Times.Once);
    }

    [Fact]
    public async Task Put_UpdatesTask_WhenIdIsValid()
    {
        Mock<TasksService> _mockTasksService = new(_mockOptions.Object);

        string id = "6746407b597a5a4dc9ca1eae";
        TaskModel task = new()
        {
            StartDate = DateTime.Now,
            EndDate = DateTime.Now,
            Priority = 5,
            IsCompleted = true
        };

        _mockTasksService.Setup(_ => _.UpdateAsync(id, task)).Returns(TasksMock.Update(id, task));
        _mockTasksService.Setup(_ => _.GetAsync(id)).ReturnsAsync(TasksMock.GetById(id));

        var controller = new TasksController(_mockTasksService.Object);

        // Act
        var result = await controller.Update(id, task);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(TasksMock.GetById(id)!.Priority, task.Priority);
        _mockTasksService.Verify(s => s.GetAsync(id), Times.Once);
        _mockTasksService.Verify(s => s.UpdateAsync(id, task), Times.Once);
    }
    
    [Fact]
    public async Task Put_ReturnsNotFound_WhenIdIsNotValid()
    {
        Mock<TasksService> _mockTasksService = new(_mockOptions.Object);

        string id = "This id does not exist";
        TaskModel task = new()
        {
            StartDate = DateTime.Now,
            EndDate = DateTime.Now,
            Priority = 5,
            IsCompleted = true
        };

        _mockTasksService.Setup(_ => _.GetAsync(id)).ReturnsAsync(TasksMock.GetById(id));

        var controller = new TasksController(_mockTasksService.Object);

        // Act
        var result = await controller.Update(id, task);

        Assert.IsType<NotFoundResult>(result);
        _mockTasksService.Verify(s => s.GetAsync(id), Times.Once);
        _mockTasksService.Verify(s => s.UpdateAsync(id, task), Times.Never);
    }

    [Fact]
    public async Task Delete_RemovesTask()
    {
        Mock<TasksService> _mockTasksService = new(_mockOptions.Object);
        string id = "qwertyuiop";

        _mockTasksService.Setup(_ => _.GetAsync(id)).ReturnsAsync(TasksMock.GetById(id));
        _mockTasksService.Setup(_ => _.RemoveAsync(id)).Returns(TasksMock.Remove(id));

        TasksController _tasksController = new(_mockTasksService.Object);

        var result = await _tasksController.Delete(id);

        Assert.IsType<NoContentResult>(result);
        _mockTasksService.Verify(s => s.GetAsync(id), Times.Once);
        _mockTasksService.Verify(s => s.RemoveAsync(id), Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenIdIsInvalid()
    {
        Mock<TasksService> _mockTasksService = new(_mockOptions.Object);
        string id = "This is an invalid id";

        _mockTasksService.Setup(_ => _.GetAsync(id)).ReturnsAsync(TasksMock.GetById(id));

        TasksController _tasksController = new(_mockTasksService.Object);

        var result = await _tasksController.Delete(id);

        Assert.IsType<NotFoundResult>(result);
        _mockTasksService.Verify(s => s.GetAsync(id), Times.Once);
    }
}
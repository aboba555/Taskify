using BusinessLogic.DTO;

namespace BusinessLogic.Services.TaskService;

public interface ITaskService
{
    Task CreateTask(CreateTaskDto createTaskDto, int userId);
    Task UpdateTask(UpdateTaskDto updateTaskDto, int userId);
    Task AddComment(CreateCommentDto createCommentDto, int userId);
    Task<List<TaskItemDto>> GetTeamTasks(int teamId, int userId);
    Task<List<TaskItemDto>> GetMyTask(int userId);
}
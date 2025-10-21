using Application.Common.Models;
using Application.Features.ToDos.Dtos;
using Application.Features.ToDos.Handlers;

namespace Application.Features.ToDos;

public static class ToDoMapper
{
    public static ToDoDto ToDto(this ToDo toDo)
    {
        return new ToDoDto
        {
            Id = toDo.Id,
            Title = toDo.Title,
            Description = toDo.Description,
            PercentComplete = toDo.PercentComplete,
            ExpiryDate = toDo.ExpiryDate
        };
    }

    public static void UpdateFromDto(this ToDo toDo, UpdateToDoCommand dto)
    {
        toDo.Title = dto.Title;
        toDo.Description = dto.Description;
        toDo.PercentComplete = dto.PercentComplete;
        toDo.ExpiryDate = dto.ExpiryDate;
    }
}

using Application.Common.Models;
using Application.Features.ToDos.Dtos;
using Application.Infrastructure.Persistence;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.ToDos.Handlers;

public record CreateToDoCommand : IRequest<ErrorOr<ToDoDto>>
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public DateTime ExpiryDate { get; set; }
}

internal sealed class CreateToDoHandler(ApplicationDbContext context) : IRequestHandler<CreateToDoCommand, ErrorOr<ToDoDto>>
{
    public async Task<ErrorOr<ToDoDto>> Handle(CreateToDoCommand request, CancellationToken cancellationToken)
    {
        var toDo = new ToDo
        {
            Title = request.Title,
            Description = request.Description,
            PercentComplete = 0,
            ExpiryDate = request.ExpiryDate
        };

        context.ToDos.Add(toDo);

        await context.SaveChangesAsync(cancellationToken);

        return toDo.ToDto();
    }
}

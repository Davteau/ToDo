using Application.Features.ToDos.Dtos;
using Application.Features.ToDos.Handlers;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Endpoints;

public static class ToDoEndpoints
{
    public static void MapToDoEndpoints(this WebApplication app)
    {
        var serviceGroup = app.MapGroup("/api/todos")
            .WithTags("ToDo");

        serviceGroup.MapPost("/", async (CreateToDoCommand command, IMediator mediator, HttpContext httpContext) =>
        {
            ErrorOr<ToDoDto> result = await mediator.Send(command);
            return result.MatchToResultCreated(httpContext, $"/api/todos/{result.Value?.Id}");
        })
        .WithSummary("Create a new to-do task")
        .WithDescription("Creates a new to-do task with the provided details.")
        .WithCreatedResponse<ToDoDto>();


        serviceGroup.MapDelete("/{id}", async (Guid id, IMediator mediator, HttpContext httpContext) =>
        {
            ErrorOr<Unit> result = await mediator.Send(new DeleteToDoCommand(id));
            return result.MatchToResultNoContent(httpContext);
        })
        .WithSummary("Delete a to-do task")
        .WithDescription("Deletes the to-do task with the specified ID.")
        .WithNoContentResponse();

        serviceGroup.MapGet("/", async (IMediator mediator, HttpContext httpContext) =>
        {
            ErrorOr<IEnumerable<ToDoDto>> result = await mediator.Send(new GetToDosQuery());
            return result.MatchToResult(httpContext);
        })
        .WithSummary("Get all to-do tasks")
        .WithDescription("Retrieves a list of all to-do tasks.")
        .WithGetListResponse<IEnumerable<ToDoDto>>();

        serviceGroup.MapGet("/{id}", async (Guid id, IMediator mediator, HttpContext httpContext) =>
        {
            ErrorOr<ToDoDto> result = await mediator.Send(new GetToDoQuery(id));
            return result.MatchToResult(httpContext);
        })
        .WithSummary("Get a to-do task by ID")
        .WithDescription("Retrieves the to-do task with the specified ID.")
        .WithGetResponse<ToDoDto>();

        serviceGroup.MapGet("/incoming/{period}", async (string period, IMediator mediator, HttpContext httpContext) =>
        {
            ErrorOr<IEnumerable<ToDoDto>> result = await mediator.Send(new GetIncomingToDosQuery(period));
            return result.MatchToResult(httpContext);
        })
        .WithSummary("Get incoming to-do tasks for a specified period")
        .WithDescription("Retrieves to-do tasks that are due within the specified period (e.g., today, next day, current week).")
        .WithGetListResponse<IEnumerable<ToDoDto>>();

        serviceGroup.MapPut("/", async (UpdateToDoCommand command, IMediator mediator, HttpContext httpContext) =>
        {
            ErrorOr<Unit> result = await mediator.Send(command);
            return result.MatchToResultNoContent(httpContext);
        })
        .WithSummary("Update a to-do task")
        .WithDescription("Updates the details of an existing to-do task.")
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithNoContentResponse();

        serviceGroup.MapPatch("/{id}/percent", async (Guid id, int percentComplete, IMediator mediator, HttpContext httpContext) =>
        {
            ErrorOr<Unit> result = await mediator.Send(new SetToDoPercentCommand(id,percentComplete));

            return result.MatchToResultNoContent(httpContext);
        })
        .WithSummary("Set the completion percentage of a to-do task")
        .WithDescription("Sets the percent complete for the specified to-do task.")
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .WithNoContentResponse();

        serviceGroup.MapPatch("/{id}/complete", async (Guid id, IMediator mediator, HttpContext httpContext) =>
        {
            ErrorOr<Unit> result = await mediator.Send(new MarkToDoAsDoneCommand(id));
            return result.MatchToResultNoContent(httpContext);
        })
        .WithSummary("Mark a to-do task as complete")
        .WithDescription("Marks the specified to-do task as completed.")
        .WithNoContentResponse();
    }
}

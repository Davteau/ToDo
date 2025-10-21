using Application.Common.Behaviours;
using Application.Features.ToDos.Validators;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

// Makes internal classes visible to the "Tests" project (for unit testing purposes)
[assembly: InternalsVisibleTo("Tests")]

namespace Application;

// Static class used to register all application-layer services into the DI container
public static class ApplicationServiceRegistration
{
    // Extension method that adds application-level services
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registers all FluentValidation validators found in the specified assembly.
        services.AddValidatorsFromAssembly(
            typeof(CreateToDoValidator).Assembly,
            includeInternalTypes: true
        );

        // Registers MediatR and its pipeline behaviors.
        services.AddMediatR(options =>
        {
            // Adds a global validation behavior that runs before every MediatR request handler.
            // This means all requests will be validated automatically using FluentValidation.
            options.AddOpenBehavior(typeof(ValidationBehaviour<,>));

            // Registers all MediatR handlers (commands, queries, etc.) from this assembly.
            options.RegisterServicesFromAssembly(typeof(ApplicationServiceRegistration).Assembly);
        });

        return services;
    }
}

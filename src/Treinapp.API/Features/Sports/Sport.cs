using System;

namespace Treinapp.API.Features.Sports
{
    public record Sport(Guid Id, string Name, string Description)
    {
        Sport Rename(string newName) => this with { Name = newName };

        Sport ChangeDescription(string newDescription) => this with { Description = newDescription };
    }
}

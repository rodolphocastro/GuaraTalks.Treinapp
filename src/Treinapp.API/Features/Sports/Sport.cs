
using System;

namespace Treinapp.API.Features.Sports
{
    /// <summary>
    /// A Sport controller by this API.
    /// </summary>
    /// <param name="Id">The sport's unique ID</param>
    /// <param name="Name">The sport's Name</param>
    /// <param name="Description">A short description of the sport</param>
    public record Sport(Guid Id, string Name, string Description);
}

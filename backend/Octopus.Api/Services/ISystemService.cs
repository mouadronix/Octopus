using Octopus.Api.Models;

namespace Octopus.Api.Services;

public interface ISystemService
{
    SystemState GetState();
}

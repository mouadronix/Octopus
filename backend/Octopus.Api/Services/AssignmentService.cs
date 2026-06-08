using Octopus.Api.Data;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public class AssignmentService
{
    private readonly AppDbContext _context;

    public AssignmentService(AppDbContext context)
    {
        _context = context;
    }

    public List<Assignment> GetAll()
    {
        return _context.Assignments.ToList();
    }
}

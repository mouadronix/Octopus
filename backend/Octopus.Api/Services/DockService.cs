using Microsoft.EntityFrameworkCore;
using Octopus.Api.Data;
using Octopus.Api.DTOs;
using Octopus.Api.Models;

namespace Octopus.Api.Services;

public class DockService
{
    private readonly AppDbContext _context;

    public DockService(AppDbContext context)
    {
        _context = context;
    }

    public List<DockResponse> GetAll()
    {
        return _context.Docks
            .Include(d => d.Assignments)
            .ThenInclude(a => a.Ship)
            .Select(d => new DockResponse
            {
                Id = d.Id,
                Name = d.Name,
                Size = d.Size,
                Assignments = d.Assignments.Select(a => new AssignmentInfo
                {
                    ShipId = a.ShipId,
                    ShipName = a.Ship.Name,
                    StartDay = a.StartDay,
                    EndDay = a.EndDay
                }).ToList()
            })
            .ToList();
    }

    public Dock? GetById(int id)
    {
        return _context.Docks.Find(id);
    }
}

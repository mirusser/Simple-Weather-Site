using BackupService.Application.Features.Commands;
using BackupService.Application.Features.Queries;
using BackupService.Application.Models.Requests;
using Common.Presentation.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace BackupService.Api.Controllers;

public sealed class BackupController : ApiController
{
    [HttpPost]
    public async Task<IActionResult> CreateSqlBackup(CreateSqlBackupRequest request)
    {
        var command = Mapper.Map<CreateSqlBackupCommand>(request);

        var result = await Mediator.SendAsync(command, HttpContext.RequestAborted);

        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> StartSqlBackup(StartSqlBackupRequest request)
    {
        var command = Mapper.Map<StartSqlBackupCommand>(request);

        var result = await Mediator.SendAsync(command, HttpContext.RequestAborted);

        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> GetSqlBackupStatus(GetSqlBackupStatusRequest request)
    {
        var query = Mapper.Map<GetSqlBackupStatusQuery>(request);

        var result = await Mediator.SendAsync(query, HttpContext.RequestAborted);

        return FromResult(result);
    }
}

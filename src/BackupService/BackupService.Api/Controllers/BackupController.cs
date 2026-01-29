using BackupService.Api.Models;
using BackupService.Api.Services;
using BackupService.Application.Features.Commands;
using Common.Presentation.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace BackupService.Api.Controllers;

public sealed class BackupController(IBackupJobRunner jobRunner) : ApiController
{
    [HttpPost]
    public async Task<IActionResult> CreateSqlBackup(CreateSqlBackupCommand request)
    {
        var result = await Mediator.SendAsync(request);
        return FromResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> StartSqlBackup(StartSqlBackupRequest request)
    {
        var jobId = await jobRunner.StartSqlBackupAsync(request.BackupName, HttpContext.RequestAborted);
        return Ok(new { jobId });
    }

    [HttpGet("{jobId}")]
    public IActionResult GetSqlBackupStatus(string jobId)
    {
        var status = jobRunner.GetStatus(jobId);
        if (status is null)
        {
            return NotFound();
        }

        return Ok(status);
    }
}

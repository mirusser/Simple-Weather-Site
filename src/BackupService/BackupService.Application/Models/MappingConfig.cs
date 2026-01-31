using BackupService.Application.Features.Commands;
using BackupService.Application.Features.Queries;
using BackupService.Application.Models.Requests;
using Mapster;

namespace BackupService.Application.Models;

public class MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateSqlBackupRequest, CreateSqlBackupCommand>();
        config.NewConfig<StartSqlBackupRequest, StartSqlBackupCommand>();
        config.NewConfig<GetSqlBackupStatusRequest, GetSqlBackupStatusQuery>();
    }
}
using Common.Mediator;
using HangfireService.Models;

namespace HangfireService.Features.Queries;

public class GetAllJobTypeQuery : IRequest<IEnumerable<JobTypeDto>>;

public class GetAllJobTypeHandler : IRequestHandler<GetAllJobTypeQuery, IEnumerable<JobTypeDto>>
{
	public async Task<IEnumerable<JobTypeDto>> Handle(GetAllJobTypeQuery request, CancellationToken cancellationToken)
	{
		var jobTypes = (JobType[])Enum.GetValues(typeof(JobType));

		var result = jobTypes
			.Select(jobType => new JobTypeDto { Value = (int)jobType, Name = jobType.ToString() });

		return await Task.FromResult(result);
	}
}
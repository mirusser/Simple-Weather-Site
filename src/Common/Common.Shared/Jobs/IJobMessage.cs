namespace Common.Shared.Jobs;

public interface IJobMessage
{
	public string JobName { get; set; }
	public string ServiceName { get; set; }
}
using System.Threading.Channels;

namespace PatientHealthRecordsEPJ.DokumentlagerTest;

public interface IBackgroundTaskQueue
{
	ValueTask QueueAsync(Func<CancellationToken, ValueTask> workItem);
	ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
}

public sealed class BackgroundTaskQueue : IBackgroundTaskQueue
{
	private readonly Channel<Func<CancellationToken, ValueTask>> _queue;

	public BackgroundTaskQueue()
	{
		_queue = Channel.CreateUnbounded<Func<CancellationToken, ValueTask>>();
	}

	public ValueTask QueueAsync(Func<CancellationToken, ValueTask> workItem)
	{
		if (workItem is null) throw new ArgumentNullException(nameof(workItem));
		return _queue.Writer.WriteAsync(workItem);
	}

	public ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken)
	{
		return _queue.Reader.ReadAsync(cancellationToken);
	}
}
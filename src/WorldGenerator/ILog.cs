namespace WorldGenerator
{
	public interface ILog
	{
		void Log(string message);
		void SetProgress(float? progress);
	}
}

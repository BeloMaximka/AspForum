namespace AspForum.Models
{
	public class ResultWithManyErrorMessages
	{
		public bool Success { get; set; }
		public List<string> Errors { get; set; } = new();
	}
}

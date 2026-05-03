using Markdig;

namespace Himi.Services;

public interface IMarkdownRenderer
{
	string ToHtml(string markdown);
}

public sealed class MarkdownRenderer : IMarkdownRenderer
{
	private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
		.UseAdvancedExtensions()
		.Build();

	public string ToHtml(string markdown)
	{
		if (string.IsNullOrWhiteSpace(markdown))
			return string.Empty;

		return Markdown.ToHtml(markdown, Pipeline);
	}
}


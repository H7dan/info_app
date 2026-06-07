namespace Himi.Services;

/// <summary>
/// News articles are stored as "# title" + body; AI translation receives title separately.
/// </summary>
public static class ArticleMarkdownHelper
{
	public static string StripLeadingHeading(string markdown)
	{
		if (string.IsNullOrWhiteSpace(markdown))
			return string.Empty;

		var lines = markdown.Replace("\r\n", "\n").Split('\n');
		var i = 0;
		while (i < lines.Length && string.IsNullOrWhiteSpace(lines[i]))
			i++;

		if (i < lines.Length && lines[i].StartsWith("# ", StringComparison.Ordinal))
		{
			i++;
			while (i < lines.Length && string.IsNullOrWhiteSpace(lines[i]))
				i++;
		}

		return string.Join('\n', lines.Skip(i)).Trim();
	}

	public static string NormalizeTranslatedBody(string body, string? title)
	{
		var normalized = StripLeadingHeading(body);
		normalized = StripLeadingHeading(normalized);

		if (string.IsNullOrWhiteSpace(title))
			return normalized;

		var titleText = title.Trim();
		var duplicateHeading = "\n# " + titleText;
		var dupIdx = normalized.IndexOf(duplicateHeading, StringComparison.OrdinalIgnoreCase);
		if (dupIdx >= 0)
			normalized = normalized[..dupIdx].Trim();

		while (normalized.StartsWith(titleText, StringComparison.OrdinalIgnoreCase))
			normalized = normalized[titleText.Length..].TrimStart();

		return normalized;
	}
}

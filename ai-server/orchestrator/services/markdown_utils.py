def strip_leading_heading(markdown: str) -> str:
    lines = markdown.replace("\r\n", "\n").splitlines()
    i = 0
    while i < len(lines) and not lines[i].strip():
        i += 1
    if i < len(lines) and lines[i].lstrip().startswith("#"):
        i += 1
        while i < len(lines) and not lines[i].strip():
            i += 1
    return "\n".join(lines[i:]).strip()


def normalize_translated_body(body: str, title: str) -> str:
    normalized = strip_leading_heading(body)
    normalized = strip_leading_heading(normalized)
    if title:
        title_text = title.strip()
        marker = "\n# " + title_text
        idx = normalized.lower().find(marker.lower())
        if idx >= 0:
            normalized = normalized[:idx].rstrip()
        while normalized.lower().startswith(title_text.lower()):
            normalized = normalized[len(title_text) :].lstrip()
    return normalized

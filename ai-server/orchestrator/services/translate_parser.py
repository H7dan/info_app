def parse_translated_article(raw: str) -> tuple[str, str]:
    text = raw.strip()
    if not text:
        return "", ""

    lines = text.splitlines()
    if lines and lines[0].startswith("#"):
        title = lines[0].lstrip("#").strip()
        body = "\n".join(lines[1:]).strip()
        return title, body

    return "", text

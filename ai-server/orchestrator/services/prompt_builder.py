from pathlib import Path

PROMPTS_DIR = Path(__file__).resolve().parent.parent / "prompts"

_LANG_NAMES = {
    "ua": "Ukrainian",
    "pl": "Polish",
    "ru": "Russian",
}


def _load(name: str) -> str:
    return (PROMPTS_DIR / name).read_text(encoding="utf-8")


def _lang_name(code: str) -> str:
    return _LANG_NAMES.get(code, code)


def chat_system(fallback_language: str) -> str:
    base = _load("chat_system.txt")
    return (
        f"{base}\n\n"
        f"If the user's language is unclear, use the app UI language as fallback: "
        f"{fallback_language} ({_lang_name(fallback_language)})."
    )


def translate_system(source_lang: str, target_lang: str) -> str:
    template = _load("translate_system.txt")
    return template.format(
        source_lang=_lang_name(source_lang),
        target_lang=_lang_name(target_lang),
    )


def article_qa_system(
    fallback_language: str, article_title: str, article_markdown: str
) -> str:
    template = _load("article_qa_system.txt")
    return (
        template.format(
            article_title=article_title,
            article_markdown=article_markdown,
        )
        + f"\n\nIf the user's language is unclear, use the app UI language as fallback: "
        f"{fallback_language} ({_lang_name(fallback_language)})."
    )


def translate_user_message(title: str, markdown: str) -> str:
    return f"Title:\n{title}\n\nBody:\n{markdown}"

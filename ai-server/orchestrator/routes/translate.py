from fastapi import APIRouter, HTTPException
from pydantic import BaseModel, Field

from services import ollama_client, prompt_builder, translate_parser, markdown_utils

router = APIRouter()


class TranslateRequest(BaseModel):
    title: str
    markdown: str
    sourceLang: str = Field(pattern="^(ua|pl|ru)$")
    targetLang: str = Field(pattern="^(ua|pl|ru)$")


class TranslateResponse(BaseModel):
    translatedTitle: str
    translatedMarkdown: str


@router.post("/translate", response_model=TranslateResponse)
async def translate(request: TranslateRequest) -> TranslateResponse:
    if not request.markdown.strip():
        raise HTTPException(status_code=400, detail="markdown is required")

    body = markdown_utils.strip_leading_heading(request.markdown)
    system = prompt_builder.translate_system(request.sourceLang, request.targetLang)
    user = prompt_builder.translate_user_message(request.title, body)

    try:
        raw = await ollama_client.generate(system, user)
    except Exception as exc:
        raise HTTPException(status_code=502, detail=f"Ollama error: {exc}") from exc

    translated_title, translated_body = translate_parser.parse_translated_article(raw)
    if not translated_title:
        translated_title = request.title

    translated_body = markdown_utils.normalize_translated_body(
        translated_body, translated_title
    )

    return TranslateResponse(
        translatedTitle=translated_title,
        translatedMarkdown=translated_body,
    )

from fastapi import APIRouter, HTTPException
from pydantic import BaseModel, Field

from services import ollama_client, prompt_builder

router = APIRouter()


class HistoryMessage(BaseModel):
    role: str = Field(pattern="^(user|assistant)$")
    content: str


class ChatRequest(BaseModel):
    message: str
    language: str = Field(pattern="^(ua|pl|ru)$")
    history: list[HistoryMessage] = Field(default_factory=list)


class ChatAboutArticleRequest(BaseModel):
    message: str
    language: str = Field(pattern="^(ua|pl|ru)$")
    articleTitle: str
    articleMarkdown: str
    history: list[HistoryMessage] = Field(default_factory=list)


class ChatResponse(BaseModel):
    reply: str


@router.post("/chat", response_model=ChatResponse)
async def chat(request: ChatRequest) -> ChatResponse:
    if not request.message.strip():
        raise HTTPException(status_code=400, detail="message is required")

    history = [{"role": m.role, "content": m.content} for m in request.history]
    system = prompt_builder.chat_system(request.language)

    try:
        reply = await ollama_client.generate(system, request.message.strip(), history)
    except Exception as exc:
        raise HTTPException(status_code=502, detail=f"Ollama error: {exc}") from exc

    return ChatResponse(reply=reply)


@router.post("/chat-about-article", response_model=ChatResponse)
async def chat_about_article(request: ChatAboutArticleRequest) -> ChatResponse:
    if not request.message.strip():
        raise HTTPException(status_code=400, detail="message is required")
    if not request.articleMarkdown.strip():
        raise HTTPException(status_code=400, detail="articleMarkdown is required")

    history = [{"role": m.role, "content": m.content} for m in request.history]
    system = prompt_builder.article_qa_system(
        request.language,
        request.articleTitle,
        request.articleMarkdown,
    )

    try:
        reply = await ollama_client.generate(system, request.message.strip(), history)
    except Exception as exc:
        raise HTTPException(status_code=502, detail=f"Ollama error: {exc}") from exc

    return ChatResponse(reply=reply)

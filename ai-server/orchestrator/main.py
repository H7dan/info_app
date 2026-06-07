from fastapi import FastAPI

from routes import chat, translate
from services import ollama_client

app = FastAPI(title="Himi AI Orchestrator", version="1.0.0")

app.include_router(chat.router, prefix="/v1")
app.include_router(translate.router, prefix="/v1")


@app.get("/health")
async def health() -> dict:
    ollama_ok = await ollama_client.is_available()
    return {
        "ok": True,
        "ollama": ollama_ok,
        "model": ollama_client.OLLAMA_MODEL,
    }

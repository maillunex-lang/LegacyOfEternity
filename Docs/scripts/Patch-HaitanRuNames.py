#!/usr/bin/env python3
"""Set RU NPC + collection display names for Haitan catalogs."""
import json
from pathlib import Path

MODELS = Path(r"C:\LegacyOfEternity\Client\Assets\Resources\OriginalMap\HaitanModels.json")
DIALOGUES = Path(r"C:\LegacyOfEternity\Client\Assets\Resources\OriginalMap\HaitanDialogues.json")
QUESTS = Path(r"C:\LegacyOfEternity\Client\Assets\Resources\OriginalMap\HaitanQuests.json")

NPC_RU = {
    201001001: "Тиас",
    201001002: "Грейт",
    201001003: "Прадо",
    201001004: "Грей",
    201001005: "Анджела",
    201001006: "Тиас",
    201001007: "Элвин",
    201001008: "Элвин",
    201001009: "Крестьянин",
    201001010: "Оф",
    201001011: "Элвин",
    201001012: "Тиас",
    201001013: "Элвин",
    201001014: "Анджела",
    201001015: "Тиас",
    201001016: "Кайн",
    201001017: "Кайн",
    201002001: "Прадо",
    201002002: "Кайн",
    201002003: "Граф Хелун",
    201002004: "Кайн",
    201002005: "Кайн",
    201002006: "Анджела",
    201002007: "Король Ривен",
    201003001: "Элвин",
    201003002: "Тиас",
    201003003: "Грейт",
    201003004: "Кайн",
    201003005: "Парис",
    201003006: "Элвин",
    201003008: "Анджела",
    201003009: "Тиас",
}


def main():
    dialogues = json.loads(DIALOGUES.read_text(encoding="utf-8"))
    quests = json.loads(QUESTS.read_text(encoding="utf-8"))
    models = json.loads(MODELS.read_text(encoding="utf-8"))

    # Collection id -> RU name from gather quest docs
    collection_names = {}
    docs = {q["id"]: q for q in dialogues.get("quests", [])}
    for q in quests.get("quests", []):
        if int(q.get("kind") or 0) != 4:
            continue
        cid = int(q.get("goalId") or 0)
        doc = docs.get(int(q["id"]))
        if cid and doc and doc.get("goalTarget"):
            collection_names[cid] = doc["goalTarget"]

    models["collectionNames"] = [
        {"id": cid, "name": name} for cid, name in sorted(collection_names.items())
    ]

    for look in models.get("npcLooks", []):
        nid = int(look.get("npcId") or 0)
        if nid in NPC_RU:
            look["name"] = NPC_RU[nid]

    # greetings already RU; ensure npc names in greetings stay
    MODELS.write_text(json.dumps(models, ensure_ascii=False, indent=4) + "\n", encoding="utf-8")
    print("npc patched", sum(1 for n in models["npcLooks"] if n.get("name") in NPC_RU.values()))
    print("collections", models["collectionNames"])


if __name__ == "__main__":
    main()

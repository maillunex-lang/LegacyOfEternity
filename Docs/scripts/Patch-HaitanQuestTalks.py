#!/usr/bin/env python3
"""Merge acceptTalk/goingTalk/finishTalk from client t_quest into HaitanDialogues + HaitanQuests."""
import json
import re
from pathlib import Path

T_QUEST = Path(r"C:\Source\Darklord-Final\Client\Assets\Resources\Lua\Data\t_quest.lua.txt")
DIALOGUES = Path(r"C:\LegacyOfEternity\Client\Assets\Resources\OriginalMap\HaitanDialogues.json")
QUESTS = Path(r"C:\LegacyOfEternity\Client\Assets\Resources\OriginalMap\HaitanQuests.json")


def parse_t_quest():
    text = T_QUEST.read_text(encoding="utf-8", errors="ignore")
    km = re.search(r"local keys = \{(.*?)\}", text, re.S)
    keys = [k.strip().strip('"') for k in km.group(1).split(",")]
    idx = {k: i for i, k in enumerate(keys)}
    rx = re.compile(r"\[(10017\d{2})\]=\{([^\[\]]+)\}")
    out = {}
    for m in rx.finditer(text):
        qid = int(m.group(1))
        raw = m.group(2)
        parts = []
        cur = []
        inq = False
        for ch in raw:
            if ch == '"':
                inq = not inq
                cur.append(ch)
                continue
            if ch == "," and not inq:
                parts.append("".join(cur).strip().strip('"'))
                cur = []
                continue
            cur.append(ch)
        if cur:
            parts.append("".join(cur).strip().strip('"'))

        def g(name):
            i = idx[name]
            return parts[i] if i < len(parts) else ""

        def as_int(s):
            s = (s or "").strip()
            if not s or s == "nil":
                return 0
            try:
                return int(float(s))
            except Exception:
                return 0

        out[qid] = {
            "acceptNpcId": as_int(g("acceptNpc")),
            "acceptTalkId": as_int(g("acceptTalk")),
            "goingTalkId": as_int(g("goingTalk")),
            "finishTalkId": as_int(g("finishTalk")),
        }
    return out


def main():
    talks = parse_t_quest()
    print("t_quest haitan talks", len(talks))

    dialogues = json.loads(DIALOGUES.read_text(encoding="utf-8"))
    for q in dialogues.get("quests", []):
        t = talks.get(int(q["id"]), {})
        # Prefer explicit finishTalk; keep existing acceptStoryId as fallback.
        finish = t.get("finishTalkId") or int(q.get("acceptStoryId") or 0)
        accept = t.get("acceptTalkId") or 0
        going = t.get("goingTalkId") or 0
        # Many Haitan rows store the intro story only on finishTalk; if accept empty, reuse finish for accept intro.
        if accept == 0 and finish != 0:
            accept = finish
        q["acceptStoryId"] = accept
        q["goingStoryId"] = going
        q["finishStoryId"] = finish
    DIALOGUES.write_text(json.dumps(dialogues, ensure_ascii=False, indent=4) + "\n", encoding="utf-8")
    print("patched", DIALOGUES)

    quests = json.loads(QUESTS.read_text(encoding="utf-8"))
    for q in quests.get("quests", []):
        t = talks.get(int(q["id"]), {})
        q["acceptNpcId"] = t.get("acceptNpcId", 0)
        q["acceptTalkId"] = t.get("acceptTalkId", 0)
        q["goingTalkId"] = t.get("goingTalkId", 0)
        q["finishTalkId"] = t.get("finishTalkId", 0)
    QUESTS.write_text(json.dumps(quests, ensure_ascii=False, indent=4) + "\n", encoding="utf-8")
    print("patched", QUESTS)

    # summary
    with_finish = sum(1 for v in talks.values() if v["finishTalkId"])
    with_accept = sum(1 for v in talks.values() if v["acceptTalkId"])
    with_going = sum(1 for v in talks.values() if v["goingTalkId"])
    print(f"finishTalk={with_finish} acceptTalk={with_accept} goingTalk={with_going}")
    for qid in sorted(talks)[:8]:
        print(qid, talks[qid])


if __name__ == "__main__":
    main()

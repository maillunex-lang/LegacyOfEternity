# Legacy of Eternity: development instructions

This is a new, independent Unity 6000.3.27f1 fantasy MMORPG prototype. The creator is LunexUfa (formerly Darklord), who confirmed ownership of the included game resources.

Read Docs/ROADMAP.md, Docs/CURRENT-STATUS.md, Docs/ARCHITECTURE.md and .ai/ before modifying anything. Never change the separate original Darklord-Working source tree.

## Repository rules
- Work in Client/ for Unity code/assets, Server/ for Go backend and Deploy/ for Ubuntu VPS scripts.
- Preserve Unity .meta files/GUIDs; use Git LFS for large imported binaries. Never commit Unity Library/Temp, APKs or credentials.
- Keep Android gameplay as landscape and test all four character classes after animation changes.
- Keep server authority and player data on the backend. The current combat and server are demonstration scaffolds, not production MMO mechanics.
- Use local environment config, never hard-code Windows usernames, domain/IP or private keys into committed files.
- Run the relevant Unity audit, Go tests, Docker Compose config and/or physical Android smoke before marking tasks done.
- Update Docs/ROADMAP.md and Docs/CURRENT-STATUS.md with concrete evidence after milestones.
- Public repo disclosure is authorized by the asset creator. Do not import third-party resources without checking their rights.

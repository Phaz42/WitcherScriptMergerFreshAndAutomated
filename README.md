<div align="center">
  
# 🐺 Script Merger — Fresh & Automated Edition (SM-FAE)
*The modern community evolution of the essential Witcher 3 modding utility.*

[![Nexus Mods](https://img.shields.io/badge/Nexus%20Mods-SM--FAE-orange?style=for-the-badge)](https://www.nexusmods.com/witcher3/mods/8405)
[![License: GPL 2.0](https://img.shields.io/badge/License-GPL%202.0-blue.svg?style=for-the-badge)](LICENSE)

</div>

<br>

> [!CAUTION]
> ### 🧊 DEVELOPMENT FROZEN
> This project is currently in a **frozen state**.  
> 
> A **completely rebuilt, from-scratch next-generation script merger app** is well under way: roughly 75% of it is built, and nearly all of its functionality is already in place — only the user interface remains. The new project features a modern UI framework (Avalonia), **native SteamOS/Linux support**, and a much cleaner, headless architecture using modern .NET.
> 
> The goal is to release this next-gen version well in time for the upcoming **Witcher 3: Songs of the Past** expansion, ensuring the modding community is fully prepared for the new era of content!

---

## 📌 Overview
This repository contains the source code for **Script Merger - Fresh & Automated Edition (SM-FAE)**, a community-driven evolution of the original Witcher 3 script merging utility. SM-FAE introduced automation, modern theming, and improved workflow integrations for modern mod managers.

For actual application downloads, detailed feature lists, user documentation, and comments/contact info, please visit the Nexus Mods page:  
👉 **[SM-FAE on Nexus Mods](https://www.nexusmods.com/witcher3/mods/8405)**

## 📂 Repository State & Commit History (v0.9.8-rc.5)

If you are browsing the source, you may notice a lack of incremental commit history.

All development over the lifecycle of SM-FAE was done locally in this repository without intermediary commits. The first commit is a single "snapshot" of the latest development stage at the time: the **`v0.9.8-rc.3`** Release Candidate, which was never published as binaries.

**Despite the frozen state, one more update was released: `v0.9.8-rc.4`.** The Witcher 3 Remastered (5.x) arrived unanticipated, and with it a changed script encoding and a changed bundle format that SM-FAE could not handle. `v0.9.8-rc.4` adds support for the Remastered version — script and bundle merging, and a warning before merging a mod made for an older game version — while merges on 4.04 give the same results as before. A follow-up, `v0.9.8-rc.5`, fixes choosing the game folder, which `v0.9.8-rc.4` broke for most GOG installations. The changes are committed incrementally on top of the snapshot, and the published binaries are built from each release's tag.

`v0.9.8-rc.5` is intended to be the last SM-FAE release, bridging the gap until the next-generation app is released.

## 📜 License & Lineage

SM-FAE is a continuation of the phenomenal work done by the Witcher 3 modding community:
* Forked from [IDCs/WitcherScriptMerger](https://github.com/IDCs/WitcherScriptMerger)
* Which was originally created by [AnotherSymbiote/WitcherScriptMerger](https://github.com/AnotherSymbiote/WitcherScriptMerger)

Please refer to the `LICENSE` file for distribution and usage rights.

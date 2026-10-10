<div align="center">
  
# 🐺 Script Merger — Fresh and Automated Edition (SM-FAE)
*The modern community evolution of the essential Witcher 3 modding utility.*

[![Nexus Mods](https://img.shields.io/badge/Nexus%20Mods-SM--FAE-orange?style=for-the-badge)](https://www.nexusmods.com/witcher3/mods/8405)
[![License: GPL 2.0](https://img.shields.io/badge/License-GPL%202.0-blue.svg?style=for-the-badge)](LICENSE)

</div>

<br>

> [!NOTE]
> ### ✅ Version 1.0 — maintained
> SM-FAE **1.0** is out, and SM-FAE is **fully supported until at least 6 months after the first stable release of the new app** I'm working on, possibly longer, depending on adoption.
> 
> The new app is a completely rebuilt, from-scratch script merger. More about it in the sticky posts on the [Nexus Mods page](https://www.nexusmods.com/witcher3/mods/8405?tab=posts).

---

## 📌 Overview
This repository contains the source code for **Script Merger - Fresh and Automated Edition (SM-FAE)**, a community-driven evolution of the original Witcher 3 script merging utility. SM-FAE finds the conflicts between your mods and helps you resolve them: it merges scripts, XML files and bundled text files automatically where it can, opens KDiff3 where a person has to decide, and lists what can't be merged in load order. It works with both the original game (4.04) and The Witcher 3 Remastered (5.x).

For downloads, the full feature list, user documentation, and comments and contact info, visit the Nexus Mods page:  
👉 **[SM-FAE on Nexus Mods](https://www.nexusmods.com/witcher3/mods/8405)**

## ✨ What's new in 1.0
1.0 ends the release candidates. The highlights:
* **Search** in both lists, with **Ctrl+F**.
* A **mod problem check** on every scan: scripts made for an older game version, and mods installed incorrectly.
* **Leave out an outdated mod** when merging.
* A **KDiff3 guide** that opens with KDiff3 when two mods change the same lines.
* Mods in **Not Mergeable** conflicts listed **in their load order**, winner first, with their load order numbers.
* A dark theme applied more consistently, an accent color across the theme, and tooltips throughout.

The full list is in the "What's new in 1.0" post on the [Nexus Mods page](https://www.nexusmods.com/witcher3/mods/8405?tab=posts).

## 📂 Repository State & Commit History

If you are browsing the source, you may notice a lack of incremental commit history before `v0.9.8-rc.4`.

All earlier development of SM-FAE was done locally, without intermediary commits. The first commit is a single "snapshot" of the development stage at the time: the **`v0.9.8-rc.3`** release candidate, which was never published as binaries.

Everything since is committed incrementally on top of that snapshot: **`v0.9.8-rc.4`** (support for The Witcher 3 Remastered: its changed script encoding and bundle format), **`v0.9.8-rc.5`** (a fix for choosing the game folder on most GOG installations), and **`v1.0.0`**. The binaries published on Nexus Mods are built from each release's tag.

## 📜 License & Lineage

SM-FAE is a continuation of the phenomenal work done by the Witcher 3 modding community:
* Forked from [IDCs/WitcherScriptMerger](https://github.com/IDCs/WitcherScriptMerger)
* Which was originally created by [AnotherSymbiote/WitcherScriptMerger](https://github.com/AnotherSymbiote/WitcherScriptMerger)

Please refer to the `LICENSE` file for distribution and usage rights.

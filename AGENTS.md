# AGENTS.md

## Release Metadata

- During release or `keep-a-changelog` work, keep `CHANGELOG.md` and `build.yaml` aligned.
- Use bare SemVer for tags and `CHANGELOG.md` headings, for example `1.2.3`.
- Use Jellyfin/JPRM four-part versions in `build.yaml` by appending `.0` to the release SemVer, for example `1.2.3` becomes `1.2.3.0`.
- Set `build.yaml` `changelog` to only the latest released changelog body, excluding older releases, `[Unreleased]`, and the release heading such as `## [1.2.3] - YYYY-MM-DD`.
- Markdown is allowed in `build.yaml` `changelog`; Jellyfin Web renders plugin revision changelogs as Markdown, and JPRM copies this field into the plugin manifest.
- Use a YAML literal block (`|`) for Markdown lists or multi-paragraph content, and a folded block (`>`) only for a single plain paragraph.
- Do not hand-edit `manifest.json` during release preparation unless explicitly requested; it is release output generated from package metadata.

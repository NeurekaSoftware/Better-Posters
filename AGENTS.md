# AGENTS.md

## Release Metadata

- During release or `keep-a-changelog` work, keep `CHANGELOG.md` and `build.yaml` aligned.
- Use bare SemVer for tags and `CHANGELOG.md` headings, for example `1.2.3`.
- Use Jellyfin/JPRM four-part versions in `build.yaml` by appending `.0` to the release SemVer, for example `1.2.3` becomes `1.2.3.0`.
- Set `build.yaml` `changelog` to only the latest released changelog body, excluding older releases, `[Unreleased]`, and the release heading such as `## [1.2.3] - YYYY-MM-DD`.
- Markdown is allowed in `build.yaml` `changelog`; Jellyfin Web renders plugin revision changelogs as Markdown, and JPRM copies this field into the plugin manifest.
- Use a YAML literal block (`|`) for Markdown lists or multi-paragraph content, and a folded block (`>`) only for a single plain paragraph.
- Do not hand-edit `manifest.json` during release preparation unless explicitly requested; it is release output generated from package metadata.

## Release Pipeline

- Releases are published by GitLab CI (`.gitlab-ci.yml`) when a bare SemVer tag (for example `1.2.3`) is pushed. The pipeline packages the plugin with JPRM, publishes a GitLab Release with the zip attached, and regenerates and commits `manifest.json` to the default branch.
- `CHANGELOG.md` must contain a released section matching the tag (`## [1.2.3] - ...`); the `jellyfin-plugin/release` component reads it for the release notes and fails if it is missing.
- The manifest-commit step needs the `MANIFEST_TOKEN` CI/CD variable (a `write_repository` token). Without it the Release still publishes, but `manifest.json` is not updated.

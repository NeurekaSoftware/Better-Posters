#!/usr/bin/env python3
"""Print the CHANGELOG.md release-notes section for a given version.

Ported from the former Forgejo ``release-jellyfin-plugin`` reusable workflow so the
GitLab release job builds the same release body from the Keep a Changelog entries.

Usage:
    python3 scripts/changelog_notes.py <version>

``<version>`` is the bare SemVer tag (for example ``1.2.3``); a ``v``-prefixed
heading is also matched.
"""

import re
import sys
from pathlib import Path

HEADING_PATTERN = re.compile(
    r"^##\s+(?:\[(?P<bracket>[^\]]+)\]|(?P<plain>[^\s]+))(?:\s+-\s+.*)?\s*$",
    re.MULTILINE,
)
LINK_REFERENCE_PATTERN = re.compile(r"^\[[^\]]+\]:\s+\S+")


def release_notes_from_changelog(text: str, version: str) -> str:
    """Return the notes for ``version`` from Keep a Changelog ``text``."""
    candidates = {version, f"v{version}"}
    headings = list(HEADING_PATTERN.finditer(text))

    for index, heading in enumerate(headings):
        heading_version = heading.group("bracket") or heading.group("plain")
        if heading_version not in candidates:
            continue

        start = heading.end()
        end = headings[index + 1].start() if index + 1 < len(headings) else len(text)
        lines = text[start:end].rstrip().splitlines()
        while lines and (
            not lines[-1].strip() or LINK_REFERENCE_PATTERN.match(lines[-1])
        ):
            lines.pop()

        notes = "\n".join(lines).strip()
        if not notes:
            raise SystemExit(f"CHANGELOG.md section for {version} is empty")
        return notes

    raise SystemExit(f"CHANGELOG.md does not contain a section for {version}")


def main() -> None:
    if len(sys.argv) != 2:
        raise SystemExit("usage: changelog_notes.py <version>")

    changelog = Path("CHANGELOG.md")
    if not changelog.is_file():
        raise SystemExit("CHANGELOG.md was not found")

    notes = release_notes_from_changelog(changelog.read_text(encoding="utf-8"), sys.argv[1])
    print(notes)


if __name__ == "__main__":
    main()

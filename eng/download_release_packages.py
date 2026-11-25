"""Download the package pair selected by a published release tag."""

import os
import re
import subprocess
from urllib.parse import quote

from eng.update_release_draft import PACKAGES, gh_api, is_main_build, is_managed_asset


def package_patterns(release, tag):
    if release["draft"] or release["tag_name"] != tag:
        raise SystemExit(f"Expected published release {tag}")
    if not is_main_build(release):
        # Older releases use the original file layout. Verify all manifests after downloading.
        return ["*.nupkg"]

    expected = {f"{package}.{tag.removeprefix('v')}.nupkg" for package in PACKAGES}
    found = set()
    for asset in release["assets"]:
        name = asset["name"]
        if not name.endswith(".nupkg"):
            continue
        if name in expected:
            found.add(name)
            continue
        # During staging or cleanup the draft can contain packages for two versions.
        other_version = any(re.fullmatch(rf"{re.escape(package)}\.\d+\.\d+\.\d+(?:-[\w.-]+)?\.nupkg", name)
                            for package in PACKAGES)
        if not (is_managed_asset(asset) and other_version):
            raise SystemExit(f"Unexpected package {name} in release {tag}")
    if found != expected:
        raise SystemExit(f"Missing packages for {tag}: {sorted(expected - found)}")
    return sorted(expected)


if __name__ == "__main__":
    repo = os.environ["GITHUB_REPOSITORY"]
    tag = os.environ["RELEASE_TAG"]
    release = gh_api(f"repos/{repo}/releases/tags/{quote(tag, safe='')}")
    command = ["gh", "release", "download", "--repo", repo, "--dir", "artifacts"]
    for pattern in package_patterns(release, tag):
        command.extend(["--pattern", pattern])
    subprocess.run([*command, "--", tag], check=True)

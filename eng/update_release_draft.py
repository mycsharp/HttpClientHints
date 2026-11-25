"""Update the main build's marked draft and its two NuGet assets."""

import hashlib
import json
import os
from pathlib import Path
import subprocess
from urllib.parse import urlencode


MARKER = "<!-- managed by main-build.yml -->"
BOT = "github-actions[bot]"
PACKAGES = ("MyCSharp.HttpClientHints", "MyCSharp.HttpClientHints.AspNetCore")


def gh_api(endpoint, *options, payload=None):
    command = ["gh", "api", endpoint, *options]
    if payload is not None:
        command.extend(["--input", "-"])
    output = subprocess.check_output(
        command, input=json.dumps(payload) if payload is not None else None, text=True
    )
    return json.loads(output) if output.strip() else None


def is_main_build(release):
    return release["author"]["login"] == BOT and MARKER in (release["body"] or "").splitlines()


def is_managed(release):
    return release["draft"] and is_main_build(release)


def is_managed_asset(asset):
    return asset["uploader"]["login"] == BOT and asset["label"] == f"{asset['name']} (main build)"


def current_build(sha, tag):
    remote = subprocess.check_output(
        ["git", "ls-remote", "--refs", "origin", "refs/heads/main", f"refs/tags/{tag}"], text=True
    )
    refs = {ref: commit for commit, ref in (line.split() for line in remote.splitlines())}
    return refs["refs/heads/main"] == sha and f"refs/tags/{tag}" not in refs


def checked_draft(endpoint, previous_tag):
    release = gh_api(endpoint)
    if not is_managed(release) or release["tag_name"] != previous_tag:
        raise SystemExit("Release ownership, tag or draft status changed")
    return release


def matches_package(asset, digest):
    return is_managed_asset(asset) and asset["state"] == "uploaded" and asset.get("digest") == digest


def stage_packages(release, releases, digests):
    assets = {asset["name"]: asset for asset in release["assets"]}
    for path, digest in digests.items():
        asset = assets.get(path.name)
        if asset is not None:
            if matches_package(asset, digest):
                continue
            if not (is_managed_asset(asset) and asset["state"] == "starter" and asset["size"] == 0):
                raise SystemExit(f"Existing asset conflicts with the built package: {path.name}")
            # GitHub can leave an empty starter after a failed upload.
            gh_api(f"{releases}/assets/{asset['id']}", "--method", "DELETE")

        query = urlencode({"name": path.name, "label": f"{path.name} (main build)"})
        url = release["upload_url"].split("{", 1)[0]
        uploaded = gh_api(f"{url}?{query}", "--method", "POST", "--input", str(path),
                          "-H", "Content-Type: application/octet-stream", "-H", f"Content-Length: {path.stat().st_size}")
        if uploaded["name"] != path.name or not matches_package(uploaded, digest):
            raise SystemExit(f"Upload verification failed for {path.name}")


def update():
    repo = os.environ["GITHUB_REPOSITORY"]
    sha = os.environ["GITHUB_SHA"]
    version = os.environ["PACKAGE_VERSION"]
    tag = f"v{version}"
    releases = f"repos/{repo}/releases"
    packages = [Path("artifacts") / f"{name}.{version}.nupkg" for name in PACKAGES]
    if set(packages) != set(Path("artifacts").glob("*.nupkg")):
        raise SystemExit(f"Expected both packages for {tag} in artifacts")

    pages = gh_api(f"{releases}?per_page=100", "--paginate", "--slurp")
    managed = []
    for page in pages:
        for release in page:
            if is_managed(release):
                managed.append(release)
            elif release["tag_name"] == tag:
                print(f"A release already owns {tag}. Skipping.")
                return
    if len(managed) > 1:
        raise SystemExit("Multiple managed drafts found")

    notes = gh_api(f"{releases}/generate-notes", payload={
        "tag_name": tag, "target_commitish": sha, "configuration_file_path": ".github/release.yml"
    })["body"]
    if version.startswith("4.0."):
        notes = Path(".github/release-notes/4.0.md").read_text(encoding="utf-8") + "\n" + notes
    body = (f"{MARKER}\n\n{notes}\n\n## NuGet packages\n\n"
            + "\n".join(f"- `dotnet add package {name} --version {version}`" for name in PACKAGES))

    if not current_build(sha, tag):
        print(f"Skipping {tag}. The main branch advanced or the tag exists.")
        return

    if managed:
        release = checked_draft(f"{releases}/{managed[0]['id']}", managed[0]["tag_name"])
    else:
        release = gh_api(releases, payload={"name": "Preparing NuGet packages", "tag_name": f"pending-{sha}",
                                            "target_commitish": sha, "body": MARKER, "draft": True})
        if not is_managed(release):
            raise SystemExit("Expected a managed draft before uploading assets")

    endpoint = f"{releases}/{release['id']}"
    digests = {p: "sha256:" + hashlib.sha256(p.read_bytes()).hexdigest() for p in packages}
    stage_packages(release, releases, digests)
    current = checked_draft(endpoint, release["tag_name"])
    assets = {asset["name"]: asset for asset in current["assets"]}
    for path, digest in digests.items():
        if path.name not in assets or not matches_package(assets[path.name], digest):
            raise SystemExit(f"Release is missing the verified package {path.name}")
    if not current_build(sha, tag):
        print(f"Skipping promotion of {tag}. The main branch advanced or the tag exists.")
        return

    # A retry keeps the maintainer's notes for an already promoted build.
    if current["tag_name"] != tag or current["target_commitish"] != sha:
        gh_api(endpoint, "--method", "PATCH", payload={
            "name": f"Version {version}", "tag_name": tag, "target_commitish": sha, "body": body
        })
    current = checked_draft(endpoint, tag)
    keep = {p.name for p in packages}
    for asset in current["assets"]:
        if is_managed_asset(asset) and asset["name"] not in keep:
            gh_api(f"{releases}/assets/{asset['id']}", "--method", "DELETE")
    print(f"Updated {current['html_url']} for {sha}")


if __name__ == "__main__":
    update()

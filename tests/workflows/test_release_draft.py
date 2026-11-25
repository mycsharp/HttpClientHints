import hashlib
import os
import subprocess
import unittest
from contextlib import chdir
from copy import deepcopy
from pathlib import Path
from tempfile import TemporaryDirectory
from unittest.mock import patch
from urllib.parse import parse_qs, urlsplit

from eng import update_release_draft as draft
from eng.download_release_packages import package_patterns


class ReleaseDraftTests(unittest.TestCase):
    def setUp(self):
        version = "4.0.23"
        self.tag = "v" + version
        self.api = "repos/org/repo/releases"
        self.packages = [f"{name}.{version}.nupkg" for name in draft.PACKAGES]
        self.enterContext(patch.dict(os.environ, GITHUB_REPOSITORY="org/repo", GITHUB_SHA="abc",
                                     PACKAGE_VERSION=version))
        notes = Path(".github/release-notes/4.0.md")
        migration = notes.read_text(encoding="utf-8")
        self.enterContext(chdir(self.enterContext(TemporaryDirectory())))
        notes.parent.mkdir(parents=True)
        notes.write_text(migration, encoding="utf-8")
        Path("artifacts").mkdir()
        for name in self.packages:
            (Path("artifacts") / name).write_bytes(name.encode())

        self.manual = dict(id=1, draft=True, author={"login": "maintainer"}, body="My notes", tag_name="preview")
        self.release = dict(id=2, draft=True, author={"login": draft.BOT}, body=draft.MARKER,
                            tag_name="v4.0.22", target_commitish="old", assets=[],
                            upload_url="https://uploads.github.com/test/2/assets{?name,label}", html_url="draft/2")
        self.uploads = []
        self.fail_upload = None
        self.refs = self.enterContext(patch.object(subprocess, "check_output", return_value="abc\trefs/heads/main\n"))
        self.github = self.enterContext(patch.object(draft, "gh_api", side_effect=self.github_api))

    def github_api(self, endpoint, *args, payload=None):
        if endpoint.endswith("?per_page=100"):
            return deepcopy([[self.manual], [self.release] if self.release else []])
        if endpoint.endswith("/generate-notes"):
            return {"body": "Changes"}
        if endpoint == self.api:
            self.assertIsNone(self.release)
            self.assertTrue(payload["draft"])
            self.assertNotEqual(self.tag, payload["tag_name"])
            self.release = dict(payload, id=2, author={"login": draft.BOT}, assets=[],
                                upload_url="https://uploads.github.com/test/2/assets{?name,label}", html_url="draft/2")
        elif endpoint == self.api + "/2":
            if payload is not None:
                self.assertEqual(args, ("--method", "PATCH"))
                self.release.update(payload)
        elif endpoint.startswith("https://uploads.github.com/test/2/assets?"):
            params = parse_qs(urlsplit(endpoint).query)
            name = params["name"][0]
            if name == self.fail_upload:
                raise subprocess.CalledProcessError(1, "upload")
            self.assertFalse(any(asset["name"] == name for asset in self.release["assets"]))
            data = Path(args[args.index("--input") + 1]).read_bytes()
            self.assertIn(f"Content-Length: {len(data)}", args)
            self.uploads.append(name)
            asset = dict(id=100 + len(self.uploads), name=name, label=params["label"][0],
                         state="uploaded", uploader={"login": draft.BOT}, size=len(data),
                         digest="sha256:" + hashlib.sha256(data).hexdigest())
            self.release["assets"].append(asset)
            return deepcopy(asset)
        elif args == ("--method", "DELETE"):
            asset_id = int(endpoint.removeprefix(self.api + "/assets/"))
            self.release["assets"].remove(next(a for a in self.release["assets"] if a["id"] == asset_id))
            return None
        else:
            self.fail(f"Unexpected GitHub call {endpoint} {args}")
        return deepcopy(self.release)

    def test_creates_marked_draft_and_only_promotes_a_verified_pair(self):
        self.release = None
        self.fail_upload = self.packages[1]
        with self.assertRaises(subprocess.CalledProcessError):
            draft.update()
        self.assertEqual("pending-abc", self.release["tag_name"])
        self.fail_upload = None
        draft.update()
        self.assertEqual(self.tag, self.release["tag_name"])
        self.assertIn("Upgrading from 3.3.5", self.release["body"])
        self.assertEqual(self.packages, self.uploads)
        self.assertEqual("My notes", self.manual["body"])

    def test_interrupted_version_change_keeps_previous_pair_and_resumes(self):
        old_names = [name.replace("4.0.23", "4.0.22") for name in self.packages]
        for i, name in enumerate(old_names):
            self.release["assets"].append(dict(id=i, name=name, label=f"{name} (main build)", uploader={"login": draft.BOT}))
        manual = dict(id=10, name="guide.pdf", label=None, uploader={"login": "maintainer"})
        self.release["assets"].append(manual)
        self.fail_upload = self.packages[1]
        with self.assertRaises(subprocess.CalledProcessError):
            draft.update()
        self.assertEqual(("v4.0.22", "old", draft.MARKER),
                         (self.release["tag_name"], self.release["target_commitish"], self.release["body"]))
        self.assertEqual(sorted(old_names), package_patterns(dict(self.release, draft=False), "v4.0.22"))

        self.fail_upload = None
        draft.update()
        self.assertEqual(self.packages, self.uploads)
        self.assertEqual((self.tag, "abc"), (self.release["tag_name"], self.release["target_commitish"]))
        self.assertEqual({"guide.pdf", *self.packages}, {a["name"] for a in self.release["assets"]})
        self.release["body"] += "\nMaintainer edits"
        draft.update()
        self.assertEqual(self.packages, self.uploads)
        self.assertTrue(self.release["body"].endswith("Maintainer edits"))

    def test_rechecks_both_server_digests_before_promotion(self):
        def corrupt_digest(endpoint, *args, **kwargs):
            result = self.github_api(endpoint, *args, **kwargs)
            if endpoint == self.api + "/2" and len(self.uploads) == 2:
                result["assets"][0]["digest"] = "sha256:bad"
            return result

        self.github.side_effect = corrupt_digest
        with self.assertRaisesRegex(SystemExit, "missing the verified package"):
            draft.update()
        self.assertEqual("v4.0.22", self.release["tag_name"])

    def test_same_name_conflict_preserves_existing_bytes(self):
        draft.update()
        existing = deepcopy(self.release)
        (Path("artifacts") / self.packages[0]).write_bytes(b"different bytes")
        with self.assertRaisesRegex(SystemExit, "conflicts"):
            draft.update()
        self.assertEqual(existing, self.release)
        self.assertEqual(self.packages, self.uploads)

    def test_repairs_only_owned_empty_starter_assets(self):
        name = self.packages[0]
        starter = dict(id=10, name=name, label=f"{name} (main build)", state="starter", size=0,
                       uploader={"login": "maintainer"})
        self.release["assets"].append(starter)
        with self.assertRaisesRegex(SystemExit, "conflicts"):
            draft.update()
        self.assertIn(starter, self.release["assets"])
        starter["uploader"]["login"] = draft.BOT
        draft.update()
        self.assertNotIn(starter, self.release["assets"])
        self.assertEqual(self.packages, self.uploads)

    def test_skips_stale_commit_existing_tag_and_manual_collision(self):
        for refs in ["def\trefs/heads/main\n", f"abc\trefs/heads/main\nabc\trefs/tags/{self.tag}\n"]:
            self.refs.return_value = refs
            draft.update()
        self.manual["tag_name"] = self.tag
        self.refs.return_value = "abc\trefs/heads/main\n"
        draft.update()
        self.assertEqual([], self.uploads)
        self.assertEqual("v4.0.22", self.release["tag_name"])

    def test_main_advancing_during_upload_prevents_promotion(self):
        self.refs.side_effect = ["abc\trefs/heads/main\n", "def\trefs/heads/main\n"]
        draft.update()
        self.assertEqual(self.packages, self.uploads)
        self.assertEqual("v4.0.22", self.release["tag_name"])

    def test_publish_requires_both_packages_and_rejects_unexpected_assets(self):
        with self.assertRaises(SystemExit):
            package_patterns(self.release, "v4.0.22")
        self.assertEqual(["*.nupkg"], package_patterns(dict(self.manual, draft=False), "preview"))
        published = dict(self.release, draft=False, tag_name=self.tag)
        with self.assertRaisesRegex(SystemExit, "Missing packages"):
            package_patterns(published, self.tag)
        draft.update()
        published = dict(self.release, draft=False)
        self.assertEqual(sorted(self.packages), package_patterns(published, self.tag))
        published["assets"].append(dict(self.release["assets"][0], name="Other.4.0.22.nupkg",
                                        label="Other.4.0.22.nupkg (main build)"))
        with self.assertRaisesRegex(SystemExit, "Unexpected package"):
            package_patterns(published, self.tag)

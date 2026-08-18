# Releasing a new plugin version

Follow these in order. Step 3 is the one that breaks submissions if skipped: the server
rejects any plugin version it has no key for, with
`401 Unrecognized or unsupported plugin version.`

## 1. Bump the version

Update both attributes in `CaeloWorks.NINA.Benchmark/Properties/AssemblyInfo.cs`:

```csharp
[assembly: AssemblyVersion("X.Y.Z.B")]
[assembly: AssemblyFileVersion("X.Y.Z.B")]
```

The plugin reports this exact string as `pluginVersion` in every submission
(`Assembly.GetExecutingAssembly().GetName().Version?.ToString()`), so it must match the
server key entry in step 3 character for character.

## 2. Build, test locally, commit, tag

Build and exercise the plugin against a local N.I.N.A. install, then commit and push `main`.
Pushing the tag `vX.Y.Z.B` runs the `Release plugin` workflow, which publishes the GitHub
Release with the DLL, a zip and `SHA256SUMS.txt`.

## 3. Register the version with the submission server (REQUIRED)

Submissions are HMAC-signed and the server maps `pluginVersion -> key` through the
`SUBMIT_KEYS` env var (JSON) in `/opt/nina-benchmark/.env` on the web host. A version that
is missing from that map cannot submit at all.

- If the key embedded in `Core/BenchmarkSigning.cs` is **unchanged**, map the new version
  onto the same key as the previous release.
- If the key **was rotated** for this release, add the new version with its new key and
  keep the older entries so already-installed builds keep working.

Then recreate the web container so it picks up the new env, and verify: posting a payload
with the new `pluginVersion` and a junk nonce must fail with `Invalid or expired nonce.`
(version accepted) rather than `Unrecognized or unsupported plugin version.`

## 4. Submit the N.I.N.A. manifest

Only with an explicit go-ahead. Add
`manifests/b/Benchmark/<nina-version>/<plugin-version>/manifest.json` in a fork of
`isbeorn/nina.plugin.manifests`, pointing `Installer.URL` at the release asset and
`Installer.Checksum` at its SHA256, validate with `npm install && node gather.js`, then
open the PR.

## 5. Update the support knowledge base

Refresh the version references and any changed behaviour in `docs/support-kb.md`.

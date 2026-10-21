# Tech Stack

## Language & Runtime

- **Language:** C#
- **Runtime:** Unity 2022.3.62f2 Mono
- **Compatibility target:** Unity's .NET Standard profile unless assembly
  analysis proves a narrower target is required.

## Framework

- **Framework:** Unity Mod using Harmony patches and the existing co-op Mod API
- **Game:** Escape from Duckov
- **Co-op Mod baseline:** EscapeFromDuckovCoopMod 1.3.6

## Package Manager

- **Package Manager:** dotnet CLI for build/test orchestration
- **Runtime package dependencies:** resolved from local game assemblies where
  possible
- **Third-party libraries:** avoid introducing unless the standard library and
  existing game dependencies are insufficient

## Commands

| Command | Script |
|---------|--------|
| Test | `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/test.ps1` |
| Typecheck | `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build.ps1 -WarningsAsErrors` |
| Lint | Not configured yet |
| Build | `powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build.ps1` |
| E2E | Manual in-game host/client verification |

The scripts are planned deliverables. Until they exist, build and test commands
are intentionally reported as unavailable by DevLoop verification.

## Testing

- **Unit test framework:** decision pending first implementation slice
- **Primary seam:** pure host-side session policy simulation
- **Integration seam:** co-op Mod network and death API adapters
- **Manual E2E:** two clients and one host in a real raid

## CI/CD

- **Platform:** None configured
- **Recommendation:** add Windows CI for pure session tests once the project
  builds independently of a local game installation

## Notes

- Game assemblies are read-only inputs and must not be committed.
- The build must not modify either installed game directory.
- Deployment to a Mod directory is a separate explicit script and is not part of
  normal test execution.

#!/bin/bash
# Usage: run_unity.sh <tag>  — BuildAll, EditMode, PlayMode, Windows build
U="/c/Program Files/Unity/Hub/Editor/6000.6.0f1/Editor/Unity.exe"
ROOT="$(cd "$(dirname "$0")/../.." && pwd -W 2>/dev/null || cd "$(dirname "$0")/../.." && pwd)"
P="$ROOT/AniBato"; T="$1"
"$U" -batchmode -quit -projectPath "$P" -executeMethod AniBato.EditorTools.ProjectBuilder.BuildAll -logFile "$ROOT/TestResults/${T}_buildall.log"; echo "buildall=$?"
grep -E "error CS" "$ROOT/TestResults/${T}_buildall.log" | sort -u | head -20
for plat in EditMode PlayMode; do
  "$U" -batchmode -projectPath "$P" -runTests -testPlatform $plat -testResults "$ROOT/TestResults/${T}_$plat.xml" -logFile "$ROOT/TestResults/${T}_$plat.log"; echo "$plat=$?"
done
"$U" -batchmode -quit -projectPath "$P" -buildWindows64Player "$ROOT/Build/$T/AniBato.exe" -logFile "$ROOT/TestResults/${T}_build.log"; echo "winbuild=$?"
grep -E "Build Finished" "$ROOT/TestResults/${T}_build.log"
# Retention: keep only the newest 2 Windows builds (each ~100-160 MB; every version is recoverable from git).
ls -1dt "$ROOT"/Build/*/ 2>/dev/null | tail -n +3 | while read -r d; do [ -f "$d/AniBato.exe" ] && echo "removing old build: $d" && rm -rf "$d"; done

#!/usr/bin/env bash
# Copyright (c) Cratis. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

set -euo pipefail

root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
if [[ ${1:-} == --self-test ]]; then
    exec node --test "$root/.github/scripts/verify-native-specifications.test.mjs"
fi
if [[ $# -ne 1 ]]; then
    echo "Usage: $0 <rid>|--self-test" >&2
    exit 2
fi
case "$1" in
    linux-x64|osx-arm64) rid=$1 ;;
    *) echo "Unsupported native smoke RID: $1" >&2; exit 2 ;;
esac

cd "$root"
output="$root/TestResults/native-aot"
mkdir -p "$output"

dotnet publish Samples/TypeDiscovery/TypeDiscovery.csproj -c Release -f net10.0 -r "$rid" \
    -p:TreatWarningsAsErrors=true -o "$output/sample"
status=0
"$output/sample/TypeDiscovery" > "$output/sample.log" 2>&1 || status=$?
cat "$output/sample.log"
if [[ $status -ne 0 ]]; then
    echo "::error::Native type-discovery sample exited with $status" >&2
    exit "$status"
fi
for expected in 'Convention binding resolved:' 'Self binding resolved:' \
    'Discovery-only convention binding resolved: Registered through AddBindingsByConvention' \
    'Discovery-only self binding resolved: OnlySelfBound' 'Demo complete.'; do
    if ! grep -Fq "$expected" "$output/sample.log"; then
        echo "::error::Native type-discovery sample did not report: $expected" >&2
        exit 1
    fi
done

dotnet build Source/DotNET/Fundamentals.AOT.Specs/Fundamentals.AOT.Specs.csproj -c Release -f net10.0
dotnet publish Source/DotNET/Fundamentals.AOT.Specs/Fundamentals.AOT.Specs.csproj \
    -c Release -f net10.0 -r "$rid" -p:TreatWarningsAsErrors=true -o "$output/specs-native"
node "$root/.github/scripts/verify-native-specifications.mjs" \
    --jit Source/DotNET/Fundamentals.AOT.Specs/bin/Release/net10.0/Cratis.Fundamentals.AOT.Specs \
    --native "$output/specs-native/Cratis.Fundamentals.AOT.Specs" --results "$output/specs" --label "$rid"
echo "Native type-discovery sample and AOT.Specs both passed ($rid)."

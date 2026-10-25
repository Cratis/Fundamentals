#!/usr/bin/env bash
# Copyright (c) Cratis. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

set -euo pipefail

if [[ $# -ne 1 ]]; then
    echo "Usage: $0 <rid>" >&2
    exit 2
fi
case "$1" in
    linux-x64|osx-arm64) rid=$1 ;;
    *) echo "Unsupported native smoke RID: $1" >&2; exit 2 ;;
esac

root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
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

# Native AOT.Specs is blocked on the current xUnit stack's trim/AOT analysis errors.
# Keep this gate sample-only until that runner can publish without suppressions.
echo "Native type-discovery sample passed ($rid). Native AOT.Specs remains blocked on xUnit."

#!/usr/bin/env bash
# Copyright (c) Cratis. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

set -uo pipefail

root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd) || exit 2
if [[ ${1:-} == --self-test ]]; then
    exec node --test "$root/.github/scripts/run-specifications.test.mjs"
fi
configuration=${1:-Release}
expected=${EXPECTED_SPECIFICATION_PROJECTS:-5}
shopt -s nullglob
projects=("$root"/Source/DotNET/*.Specs/*.Specs.csproj)
if [[ ${#projects[@]} -ne $expected ]]; then
    echo "::error::Expected $expected specification projects, found ${#projects[@]}"
    exit 2
fi

suffix=
case ${OS:-$(uname -s)} in
    Windows_NT|MINGW*|MSYS*|CYGWIN*) suffix=.exe ;;
esac
names=()
executables=()
for project in "${projects[@]}"; do
    assembly=$(sed -n 's/.*<AssemblyName>\([^<]*\)<\/AssemblyName>.*/\1/p' "$project")
    if [[ -z $assembly || $assembly == *$'\n'* ]]; then
        echo "::error::Expected one AssemblyName in $project"
        exit 2
    fi
    executable="${project%/*}/bin/$configuration/net10.0/$assembly$suffix"
    if [[ ! -x $executable ]]; then
        echo "::error::Missing specification executable: $executable"
        exit 2
    fi
    names+=("${project##*/}")
    executables+=("$executable")
done

mkdir -p "$root/TestResults" || exit 2
out=$(mktemp -d "$root/TestResults/run.XXXXXX") || exit 2
echo "Running ${#projects[@]} specification projects; results: $out"
pids=()
for index in "${!executables[@]}"; do
    name=${names[$index]%.csproj}
    "${executables[$index]}" -noLogo -noColor -result-trx "$out/$name.trx" > "$out/$name.log" 2>&1 &
    pids+=("$!")
done

result=0
for index in "${!pids[@]}"; do
    name=${names[$index]%.csproj}
    status=0
    wait "${pids[$index]}" || status=$?
    echo "::group::$name (exit $status)"
    cat "$out/$name.log" || { [[ $status -ne 0 ]] || status=2; }
    echo "::endgroup::"
    if [[ $status -ne 0 ]]; then
        echo "::error::$name exited with $status"
    elif ! grep -Eq '<Counters[[:space:]][^>]*total="[1-9][0-9]*"' "$out/$name.trx"; then
        echo "::error::$name produced no nonzero TRX test total"
        status=1
    fi
    if [[ $result -eq 0 && $status -ne 0 ]]; then
        result=$status
    fi
done
exit "$result"

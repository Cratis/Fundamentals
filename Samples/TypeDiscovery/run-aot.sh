#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
project_file="${script_dir}/TypeDiscovery.csproj"
configuration="Release"
framework="net10.0"

if [[ ! -f "${project_file}" ]]; then
    echo "Could not find TypeDiscovery.csproj next to this script."
    exit 1
fi

case "$(uname -s)" in
    Darwin)
        arch="$(uname -m)"
        if [[ "${arch}" == "arm64" ]]; then
            rid="osx-arm64"
        elif [[ "${arch}" == "x86_64" ]]; then
            rid="osx-x64"
        else
            echo "Unsupported macOS architecture: ${arch}"
            exit 1
        fi
        ;;
    Linux)
        arch="$(uname -m)"
        if [[ "${arch}" == "x86_64" ]]; then
            rid="linux-x64"
        elif [[ "${arch}" == "aarch64" ]]; then
            rid="linux-arm64"
        else
            echo "Unsupported Linux architecture: ${arch}"
            exit 1
        fi
        ;;
    *)
        echo "Unsupported OS: $(uname -s). This script supports macOS and Linux."
        exit 1
        ;;
esac

echo "Publishing Native AOT sample for RID: ${rid}"
dotnet publish "${project_file}" -c "${configuration}" -f "${framework}" -r "${rid}" --self-contained

publish_dir="${script_dir}/bin/${configuration}/${framework}/${rid}/publish"
output_binary="${publish_dir}/TypeDiscovery"

echo
if [[ -x "${output_binary}" ]]; then
    echo "Running native executable: ${output_binary}"
    "${output_binary}"
else
    echo "Native executable not found: ${output_binary}"
    echo "Make sure Native AOT prerequisites are installed for this platform."
    exit 1
fi

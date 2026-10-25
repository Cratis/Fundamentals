#!/usr/bin/env python3
# Copyright (c) Cratis. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.
"""Compare complete TRX case identities and outcomes, never just counts or display names."""
import argparse
from collections import Counter
import json
import re
import sys
import xml.etree.ElementTree as ET
from pathlib import Path

HERE = Path(__file__).resolve().parent


class Unreadable(Exception):
    pass


def local(element):
    return element.tag.rsplit('}', 1)[-1]


def arguments(text):
    token = re.compile(r'\s*(?:\w+\s*:\s*)?("(?:\\.|[^"\\])*"|[Tt]rue|[Ff]alse|null|-?\d+(?:\.\d+)?)(?:\s*,\s*|\s*$)')
    result, pos = [], 0
    while pos < len(text):
        match = token.match(text, pos)
        if not match:
            raise Unreadable(f'Cannot normalize theory arguments: {text!r}')
        result.append(json.loads(match[1].lower() if match[1] in ('True', 'False') else match[1]))
        pos = match.end()
    return result


def lossy(value):
    """What a runner writes to TRX for a string holding lone surrogates: UTF-8 bytes decoded with replacement."""
    if not isinstance(value, str):
        return value
    return value.encode('utf-8', 'surrogatepass').decode('utf-8', 'replace')


def recover_lossy(args, rows):
    """Match mangled (U+FFFD) arguments to exactly one known row by their lossy form; ambiguity is UNKNOWN."""
    if not any(isinstance(a, str) and '\ufffd' in a for a in args):
        return None
    matches = [row for row in rows if [lossy(v) for v in row] == args]
    return matches[0] if len(matches) == 1 else None


def signature(cls, method, args):
    return cls + '.' + method + '(' + ','.join(json.dumps(a, ensure_ascii=True, separators=(',', ':')) for a in args) + ')'


def case_identity(method, result, manifest, variant):
    cls = method.get('className', '').split(',', 1)[0].replace('+', '.')
    name = method.get('name', '')
    if not cls or not name:
        raise Unreadable('TRX TestMethod lacks className/name')
    display = result.get('testName', '')
    name = name.split('(', 1)[0]
    renames = manifest.get('variants', {}).get(variant, {}).get('renames', {})
    shown = name  # the name the runner displays; parity identity uses the baseline name
    name = renames.get(cls + '.' + name, name)
    key = cls + '.' + name
    alias = manifest.get('variants', {}).get(variant, {}).get('aliases', {}).get(key)
    if alias:
        return signature(alias.get('class', cls), alias['method'], alias['args'])
    theory = manifest.get('theories', {}).get(key)
    if theory:
        # TRX IDs and display-name formatting differ across runners; retain every row's values.
        for candidate in [display, method.get('name', '')]:
            method_pattern = '(?:' + re.escape(shown) + '|' + re.escape(shown.replace('_', ' ')) + ')'
            match = re.search(method_pattern + r'\((.*)\)$', candidate)
            if not match:
                continue
            try:
                args = arguments(match[1])
            except Unreadable:
                continue
            if args in theory['rows']:
                return signature(cls, name, args)
            row = recover_lossy(args, theory['rows'])
            if row is not None:
                return signature(cls, name, row)
        raise Unreadable(f'{key}: cannot recover a complete known theory row from {display!r}; no count-only fallback')
    # External suites remain xUnit v2 on both sides. Preserve their full row suffix verbatim.
    match = re.search(re.escape(shown) + r'(\(.*\))$', display)
    return key + (match[1] if match else '()')


def load(path, manifest, variant):
    root = ET.parse(path).getroot()
    definitions = {}
    for definition in root.iter():
        if local(definition) == 'UnitTest':
            method = next((m for m in definition if local(m) == 'TestMethod'), None)
            if method is not None:
                definitions[definition.get('id')] = method
    cases = {}
    count = 0
    for result in root.iter():
        if local(result) != 'UnitTestResult':
            continue
        count += 1
        method = definitions.get(result.get('testId'))
        if method is None:
            raise Unreadable(f'{path}: unresolved testId {result.get("testId")}')
        identity = case_identity(method, result, manifest, variant)
        outcome = result.get('outcome', '').lower()
        outcome = {'notexecuted': 'skipped'}.get(outcome, outcome)
        if outcome not in ('passed', 'failed', 'skipped'):
            raise Unreadable(f'{identity}: unknown outcome {outcome!r}')
        if identity in cases:
            raise Unreadable(f'{path}: duplicate case identity {identity}; never collapse rows')
        cases[identity] = outcome
    counters = [e for e in root.iter() if local(e) == 'Counters']
    if not count or len(counters) != 1 or int(counters[0].get('total', '-1')) != count:
        raise Unreadable(f'{path}: empty suite or TRX total does not match {count} result rows')
    return cases


def compare(base, candidate):
    missing = sorted(base.keys() - candidate.keys())
    added = sorted(candidate.keys() - base.keys())
    changed = sorted(k for k in base.keys() & candidate.keys() if base[k] != candidate[k])
    if missing or added or changed:
        print(f'PARITY FAIL: baseline={len(base)}, candidate={len(candidate)}, missing={len(missing)}, added={len(added)}, outcome changes={len(changed)}')
        for label, keys in [('missing', missing), ('added', added), ('outcome', changed)]:
            for key in keys[:10]:
                print(label + ': ' + key)
        return 1
    print(f'PARITY PASS: {len(base)} unique case identities and outcomes; {dict(Counter(candidate.values()))}')
    return 0


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('baseline', type=Path)
    parser.add_argument('candidate', type=Path)
    parser.add_argument('--variant', required=True)
    parser.add_argument('--suite', choices=['Arc', 'Chronicle'], help='Select an external suite from external.json')
    parser.add_argument('--manifest', type=Path, default=HERE / 'variants/manifest.json')
    args = parser.parse_args()
    try:
        manifest = json.loads(args.manifest.read_text())
        if args.suite:
            manifest = manifest[args.suite]
        return compare(load(args.baseline, manifest, 'V0'), load(args.candidate, manifest, args.variant))
    except (Unreadable, ET.ParseError, OSError, ValueError) as error:
        print(f'PARITY UNKNOWN: {error}')
        return 2

if __name__ == '__main__':
    sys.exit(main())

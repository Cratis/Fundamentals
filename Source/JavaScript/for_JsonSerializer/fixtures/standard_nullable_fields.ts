// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

declare const nullableFundamentals: typeof import('../../index');

class NullableLabel extends nullableFundamentals.ConceptAs<string> {}

class NullableModel {
    @nullableFundamentals.field(String)
    scalar?: string | null;

    @nullableFundamentals.field(NullableLabel)
    rich?: NullableLabel | null;

    @nullableFundamentals.field(Object)
    nested?: { label: string } | null;

    @nullableFundamentals.field(Array, { genericArguments: [String] })
    array?: string[] | null;

    @nullableFundamentals.field(String, true)
    enumerable?: string[] | null;

    @nullableFundamentals.field(nullableFundamentals.ValueMap, { genericArguments: [String, String] })
    map?: InstanceType<typeof nullableFundamentals.ValueMap<string, string>> | null;
}

void NullableModel;

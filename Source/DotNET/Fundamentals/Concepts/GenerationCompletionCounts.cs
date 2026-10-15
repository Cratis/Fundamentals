// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Concepts;

/// <summary>
/// Counts distinct completions on an acyclic DFA, conditioned on Mod11 residues when requested.
/// </summary>
/// <param name="automaton">The determinized payload automaton, reversed for Mod11.</param>
/// <param name="conditioned">Whether to track right-to-left Mod11 residue and phase.</param>
/// <param name="budget">The shared construction and counting budget.</param>
internal sealed class GenerationCompletionCounts(GenerationPatternAutomaton automaton, bool conditioned, GenerationWorkBudget budget)
{
    readonly Dictionary<(int State, int Residue, int Phase), GenerationCompletion> _completions = [];

    /// <summary>
    /// Builds the completion graph rooted at a DFA/product state.
    /// </summary>
    /// <param name="state">The DFA state, weighted residue and digit phase.</param>
    /// <returns>The fully counted completion node.</returns>
    /// <exception cref="InvalidValuePattern">A state or operation limit is exceeded.</exception>
    internal GenerationCompletion Build((int State, int Residue, int Phase) state)
    {
        if (_completions.TryGetValue(state, out var completion))
        {
            return completion;
        }

        if (_completions.Count == GenerationWorkBudget.StateLimit)
        {
            budget.Exceeded("state budget (10000 distinct DFA/product states)");
        }

        completion = new(automaton.Accepts(state.State) && (!conditioned || state.Residue != 1));
        _completions.Add(state, completion);

        // The automaton is acyclic; recursion is bounded by the 256-character limit.
        for (var character = ' '; character <= '~'; character++)
        {
            budget.Spend();
            var target = automaton.Move(state.State, character);
            if (automaton.IsEmpty(target))
            {
                continue;
            }

            var digit = conditioned && character is >= '0' and <= '9';
            var residue = digit ? (state.Residue + ((character - '0') * (2 + state.Phase))) % 11 : state.Residue;
            var phase = digit ? (state.Phase + 1) % 6 : state.Phase;
            var next = Build((target, residue, phase));
            completion.Branches.Add((character, next));
            completion.Count += next.Count;
        }

        return completion;
    }
}

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace SpanIt.Processors
{
    /*
SpanIt.Processors.Current
Iterations: 1000000
Took: 6,734 ms
Allocated: 24 kb
Peak Working Set: 16,472 kb
Gen 0 collections: 0
Gen 1 collections: 0
Gen 2 collections: 0
     */
    public class Current : IWordProcessor
    {
        private static readonly Memory<char> Separators = new Memory<char>(new []{ ' ', '\\', '/', '[', ']', '(', ')' });
        private static readonly Memory<char> Quick = new Memory<char>(new []{ 'q', 'u', 'i', 'c', 'k' });
        
        private readonly Letter _letters;

        public Current()
        {
            _letters = new Letter(Letter.NullTermination, 0);
        }
        
        public void Add(string words)
        {
            const int spaceBigEnoughForAllWords = 128;
            
            Span<char> space = stackalloc char[spaceBigEnoughForAllWords];
            
            var wordsAsSpan = words.AsSpan();
                    
            for (int start = 0, length; start < wordsAsSpan.Length; start += length + 1)
            {
                var current = wordsAsSpan.Slice(start);

                var found = current.IndexOfAny(Separators.Span);
                length = found == -1 ? current.Length : found;

                var word = current.Slice(0, length);
                var hyphen = word.LastIndexOf('-');
                
                AddTrieWord(word);

                if (hyphen == -1)
                {
                    if (EqualsIgnoringCase(word, Quick.Span))
                    {
                        AddTrieWord(word.Slice(1));
                    }
                }
                else
                {
                    var leftPart = word.Slice(0, hyphen);
                    AddTrieWord(leftPart);
                    
                    var rightPart = word.Slice(hyphen + 1, length - (hyphen + 1));
                    AddTrieWord(rightPart);
                    
                    var wordWithoutHyphenLength = leftPart.Length + rightPart.Length;

                    if (wordWithoutHyphenLength < spaceBigEnoughForAllWords)
                    {
                        leftPart.CopyTo(space.Slice(0, leftPart.Length));
                        rightPart.CopyTo(space.Slice(leftPart.Length, rightPart.Length));

                        AddTrieWord(space.Slice(0, wordWithoutHyphenLength));
                    }
                }
            }
        }

        private static bool EqualsIgnoringCase(ReadOnlySpan<char> word, ReadOnlySpan<char> quick)
        {
            if (word.Length != quick.Length)
            {
                return false;
            }

            for (var i = 0; i < word.Length; ++i)
            {
                var character = word[i];
                
                if (character - 'A' <= 'Z' - 'A')
                {
                    character = (char)(byte)( character | 0x20 );
                }

                if (character != quick[i])
                {
                    return false;
                }
            }

            return true;
        }

        private void AddTrieWord(ReadOnlySpan<char> word)
        {
            Span<char> terminatedWord = stackalloc char[word.Length + 1];
            terminatedWord[word.Length] = Letter.NullTermination;

            for (var i = 0; i < word.Length; ++i)
            {
                if (word[i] - 'A' <= 'Z' - 'A')
                {
                    terminatedWord[i] = (char)(byte)( word[i] | 0x20 );
                }
                else
                {
                    terminatedWord[i] = word[i];
                }
            }
            
            _letters.TryAdd(terminatedWord);
        }
        
        internal class Letter
        {
            public static readonly char NullTermination = '^';

            private readonly char _character;
            private readonly byte _depth;
            
            private Letter[] _expandedNodes;
            private char[] _flattenedNodes;
            private byte _position;
            

            public Letter(char character, byte depth)
            {
                _character = character;
                _depth = depth;
                _expandedNodes = null;
                _flattenedNodes = null;
            }
            
            public override string ToString()
            {
                var start = $"Word: {(char) _character}";

                if (_flattenedNodes != null)
                {
                    start += string.Concat(_flattenedNodes);
                }

                if (_expandedNodes != null)
                {
                    start += $", Children: {_position}";
                }
                
                return start;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Letter TryAdd(in ReadOnlySpan<char> word)
            {
                var depth = _depth;
                
                if (depth == word.Length)
                {
                    return this;
                }
                
                if (_flattenedNodes != null)
                {
                    byte index = 0;
                    byte split = 0;
                    
                    for (; depth < word.Length && index < _flattenedNodes.Length; )
                    {
                        if (_flattenedNodes[index] != word[depth])
                        {
                            break;
                        }

                        ++depth;
                        ++index;
                        
                        if (depth < word.Length - 1)
                        {
                            ++split;
                        }
                    }

                    if (depth == word.Length)
                    {
                        return this;
                    }

                    if (split < index)
                    {
                        depth = (byte) (depth - ( index - split ));
                        index = split;
                    }

                    var flattenedNodesAsSpan = _flattenedNodes.AsSpan();
                    
                    var flattenedNodesThatNeedToBeExpanded = flattenedNodesAsSpan
                        .Slice(index);
                    
                    var flattenedNodesThatNeedToReplaceTheCurrentFlattenedNodes = flattenedNodesAsSpan
                        .Slice(0, index);
                    
                    if (flattenedNodesAsSpan.SequenceEqual(flattenedNodesThatNeedToReplaceTheCurrentFlattenedNodes) == false)
                    {
                        SetFlattenedNodes(flattenedNodesThatNeedToReplaceTheCurrentFlattenedNodes);
                    }
                    
                    if (flattenedNodesThatNeedToBeExpanded.Length > 0)
                    {
                        var expandedNodeThatHasComeFromTheFlattenedNodes = new Letter(flattenedNodesThatNeedToBeExpanded[0], (byte) (depth + 1));
                        expandedNodeThatHasComeFromTheFlattenedNodes.SetExpandedNodes(_expandedNodes, _position);

                        var flattenedNodesThatNeedToBeExpandedChildren = flattenedNodesThatNeedToBeExpanded.Slice(1);
                        
                        if (flattenedNodesThatNeedToBeExpandedChildren.Length > 0)
                        {
                            expandedNodeThatHasComeFromTheFlattenedNodes.SetFlattenedNodes(flattenedNodesThatNeedToBeExpandedChildren);
                        }
                        
                        SetExpandedNodes(new []
                        {
                            expandedNodeThatHasComeFromTheFlattenedNodes
                        }, 1);
                    }
                }
                
                if (_expandedNodes != null)
                {
                    for (var i = _position - 1; i >= 0; --i)
                    {
                        if (_expandedNodes[i]._character == word[depth])
                        {
                            var last1 = _expandedNodes[i].TryAdd(word);

                            return last1;
                        }
                    }
                }

                var last2 = new Letter(word[depth], (byte) (depth + 1));
                
                var remainingNodes = word.Slice(depth + 1);
                
                if (remainingNodes.Length <= 1)
                {
                    last2.SetFlattenedNodes(remainingNodes);
                }
                else
                {
                    var remainingNodesForFlattened = remainingNodes.Slice(0, remainingNodes.Length - 2);
                    var endingNodesForExpanded = remainingNodes.Slice(remainingNodes.Length - 2);
                    
                    depth = (byte) (depth + remainingNodesForFlattened.Length);

                    if (remainingNodesForFlattened.Length > 0)
                    {
                        last2.SetFlattenedNodes(remainingNodesForFlattened);
                    }
                
                    if (endingNodesForExpanded.Length > 0)
                    {
                        var last3 = new Letter(word[depth + 1], (byte) (depth + 2));
                        last3.SetFlattenedNodes(endingNodesForExpanded.Slice(1));
                    
                        last2.AddExpandedNode(last3);
                    }
                }
                
                AddExpandedNode(last2);

                return last2;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void SetFlattenedNodes(ReadOnlySpan<char> nodes)
            {
                if (nodes == null || nodes.Length == 0)
                {
                    _flattenedNodes = null;
                }
                else
                {
                    _flattenedNodes = nodes.ToArray();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void SetExpandedNodes(Letter[] nodes, byte position)
            {
                if (nodes == null || nodes.Length == 0)
                {
                    _position = 0;
                    _expandedNodes = null;
                }
                else
                {
                    _position = position;
                    _expandedNodes = nodes;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void AddExpandedNode(Letter node)
            {
                if (_expandedNodes == null)
                {
                    _expandedNodes = new Letter[HashHelpers.PrimeLargerThan(_position)];
                    _expandedNodes[0] = node;
                    _position = 0;
                }
                else if (_position >= _expandedNodes.Length)
                {
                    var nodes = new Letter[HashHelpers.PrimeLargerThan(_position)];

                    Array.Copy(_expandedNodes, 0, nodes, 0, _expandedNodes.Length);

                    nodes[_position] = node;
                    _expandedNodes = nodes;
                }
                else
                {
                    _expandedNodes[_position] = node;
                }
                
                ++_position;
            }

            private static class HashHelpers
            {
                private static readonly int[] Primes =
                {
                    3,
                    7,
                    11,
                    17,
                    23,
                    29,
                    37,
                    47,
                    59,
                    71,
                    89,
                    107,
                    131,
                    163,
                    197,
                    239,
                    293,
                    353,
                    431,
                    521,
                    631,
                    761,
                    919,
                    1103,
                    1327,
                    1597,
                    1931,
                    2333,
                    2801,
                    3371,
                    4049,
                    4861,
                    5839,
                    7013,
                    8419,
                    10103,
                    12143,
                    14591,
                    17519,
                    21023,
                    25229,
                    30293,
                    36353,
                    43627,
                    52361,
                    62851,
                    75431,
                    90523,
                    108631,
                    130363,
                    156437,
                    187751,
                    225307,
                    270371,
                    324449,
                    389357,
                    467237,
                    560689,
                    672827,
                    807403,
                    968897,
                    1162687,
                    1395263,
                    1674319,
                    2009191,
                    2411033,
                    2893249,
                    3471899,
                    4166287,
                    4999559,
                    5999471,
                    7199369
                };

                [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
                internal static int PrimeLargerThan(int min)
                {
                    for (var index = 0; index < Primes.Length; ++index)
                    {
                        var num = Primes[index];
                        if (num > min)
                            return num;
                    }
                    
                    return min;
                }
            }
        }
    }
}
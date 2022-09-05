using System;
using System.Runtime.CompilerServices;

namespace SpanIt.Processors
{
    /*
SpanIt.Processors.ProcessorV3
Iterations: 1000000
Took: 1,328 ms
Allocated: 16 kb
Peak Working Set: 14,716 kb
Gen 0 collections: 0
Gen 1 collections: 0
Gen 2 collections: 0
     */
    public class ProcessorV3 : IWordProcessor
    {
        private static readonly char CompleteWordIndicator = '^';
            
        private static readonly Memory<char> Quick = new Memory<char>(new []{ 'q', 'u', 'i', 'c', 'k' });
        
        public void Add(string words)
        {
            const int spaceBigEnoughForAllWords = 128;
            
            Span<char> space = stackalloc char[spaceBigEnoughForAllWords];
            
            var wordsAsSpan = words.AsSpan();
            
            for (var i = 0; i < wordsAsSpan.Length; ++i)
            {
                if (IsSeparator(wordsAsSpan[i]))
                {
                    continue;
                }

                var wordStartIndex = i;
                var hyphenIndex = -1;
                
                while (i + 1 < wordsAsSpan.Length && IsSeparator(wordsAsSpan[i + 1]) == false)
                {
                    if (IsHyphen(wordsAsSpan[i]))
                    {
                        hyphenIndex = i - wordStartIndex;
                    }

                    ++i;
                }

                var length = i - wordStartIndex + 1;
                
                var word = wordsAsSpan.Slice(wordStartIndex, length);
                
                AddTrieWord(word);
                
                if (hyphenIndex == -1)
                {
                    if (EqualsIgnoringCase(word, Quick.Span))
                    {
                        AddTrieWord(word.Slice(1));
                    }
                }
                else
                {
                    var leftPart = word.Slice(0, hyphenIndex);
                    AddTrieWord(leftPart);
                    
                    var rightPart = word.Slice(hyphenIndex + 1, length - (hyphenIndex + 1));
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsHyphen(char character)
        {
            return character == '-';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsSeparator(char character)
        {
            return character == ' ' || character == '\\' || character == '/' || character == '[' || character == '(' || character == ']' || character == ')';
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddTrieWord(ReadOnlySpan<char> word)
        {
            Span<char> lower = stackalloc char[word.Length + 1];
            lower[word.Length] = CompleteWordIndicator;

            for (var i = 0; i < word.Length; ++i)
            {
                if (word[i] - 'A' <= 'Z' - 'A')
                {
                    lower[i] = (char)(byte)( word[i] | 0x20 );
                }
                else
                {
                    lower[i] = word[i];
                }
            }
            
            // Add to index here.
        }
    }

}
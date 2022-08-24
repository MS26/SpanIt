using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;

namespace SpanIt.Processors
{
    /*
SpanIt.Processors.Current
Iterations: 1000000
Took: 2,750 ms
Allocated: 16 kb
Peak Working Set: 14,816 kb
Gen 0 collections: 0
Gen 1 collections: 0
Gen 2 collections: 0
     */
    public class Current : IWordProcessor
    {
        private static readonly char CompleteWordIndicator = '^';
            
        private static readonly Memory<char> Separators = new Memory<char>(new []{ ' ', '\\', '/', '[', ']', '(', ')' });
        private static readonly Memory<char> Quick = new Memory<char>(new []{ 'q', 'u', 'i', 'c', 'k' });
        
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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SpanIt.Processors
{
    /*
SpanIt.Processors.Original
Iterations: 1000000
Took: 23,953 ms
Allocated: 15,984,446 kb
Peak Working Set: 20,776 kb
Gen 0 collections: 2601
Gen 1 collections: 2
Gen 2 collections: 0
     */
    public class Original : IWordProcessor
    {
        private readonly IDictionary<string, byte> _index;
        private readonly IDictionary<string, byte> _fullWordIndex;

        internal Original()
        {
            _index = new ConcurrentDictionary<string, byte>();
            _fullWordIndex = new ConcurrentDictionary<string, byte>();
        }
        
        public void Add(string words)
        {
            for (var i = 0; i < words.Length; ++i)
            {
                if (words[i] == ' ' || words[i] == '\\' || words[i] == '/' || words[i] == '[' || words[i] == '(' || words[i] == ']' || words[i] == ')')
                {
                    continue;
                }

                var wordStartIndex = i;
                var hyphenIndex = -1;
                
                while (i + 1 < words.Length && words[i + 1] != ' ' && words[i + 1] != '\\' && words[i + 1] != '/' && words[i + 1] != '[' && words[i + 1] != '(' && words[i + 1] != ']' && words[i + 1] != ')')
                {
                    if (words[i] == '-')
                    {
                        hyphenIndex = i;
                    }

                    ++i;
                }
                
                if (hyphenIndex == -1)
                {
                    // Word Exact
                    var word = words.Substring(wordStartIndex, i - wordStartIndex + 1);
                    AddToDictionary(word);
                    AddToFullWordIndex(word);
                    
                    // Word
                    for (var x = 1; x < word.Length; ++x)
                    {
                        AddToDictionary(word.Substring(0, x));
                    }

                    // Index XDrive Against Drive
                    if (word.Equals("quick", StringComparison.OrdinalIgnoreCase))
                    {
                        for (var x = 1; x < word.Length; ++x)
                        {
                            AddToDictionary(word.Substring(1, x));
                        }
                    }
                }
                else
                {
                    // Left Part Exact
                    var leftPart = words.Substring(wordStartIndex, hyphenIndex - wordStartIndex);
                    AddToDictionary(leftPart);
                    
                    // Right Part Exact
                    var rightPart = words.Substring(hyphenIndex + 1, i - hyphenIndex);
                    AddToDictionary(rightPart);
                    
                    // Right Part 
                    for (var x = 1; x < rightPart.Length; ++x)
                    {
                        AddToDictionary(rightPart.Substring(0, x));
                    }
                    
                    // Word Hyphen
                    var wordIncludingHyphen = words.Substring(wordStartIndex, i - wordStartIndex + 1);
                    AddToFullWordIndex(wordIncludingHyphen);

                    // Word No Hyphen
                    var word = leftPart + rightPart;
                    AddToDictionary(word);
                    
                    for (var x = 2; x < word.Length; ++x)
                    {
                        AddToDictionary(word.Substring(0, x));
                    }
                }
            }
        }

        private void AddToFullWordIndex(string word)
        {
            if (!_fullWordIndex.ContainsKey(word.ToLower()))
            {
                _fullWordIndex[word.ToLower()] = 1;
            }
        }

        private void AddToDictionary(string word)
        {
            if (!_index.ContainsKey(word.ToLower()))
            {
                _index[word.ToLower()] = 1;
            }
        }
    }
}
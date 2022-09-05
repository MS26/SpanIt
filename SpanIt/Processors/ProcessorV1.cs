using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SpanIt.Processors
{
    /*
SpanIt.Processors.ProcessorV1
Iterations: 1000000
Took: 18,297 ms
Allocated: 15,984,447 kb
Peak Working Set: 20,420 kb
Gen 0 collections: 2601
Gen 1 collections: 1
Gen 2 collections: 0
     */
    public class ProcessorV1 : IWordProcessor
    {
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
            var lower = word.ToLower();

            // Add to index here.
        }

        private void AddToDictionary(string word)
        {
            var lower = word.ToLower();
            
            // Add to index here.
        }
    }
}
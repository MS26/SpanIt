# Span It

Attempting to reduce memory allocations during word processing. 

## Development

* Each word processor is provided a string. It needs to be able to process this string into individual words and index these, complying to the requirements outlined below.

## Requirements

* The provided word should be split using these characters:
    * ' '
    * '\\'
    * '/'
    * '['
    * ']'
    * '('
    * ')'
    
* If there is a hyphen in the word:
  * Both the left and right side of the hyphen should be processed as seperate words.
  * The word should be processed excluding the hyphen.
  * The word should be processed including the hyphen.
* If there is no hyphen in the word:
  * 'quick' should be processed as both 'quick' and 'uick'.
  
* Every word should be able to be retrieved as an exact match or a partial match.
* Every word should be able to be retireved with no dependency on case.x

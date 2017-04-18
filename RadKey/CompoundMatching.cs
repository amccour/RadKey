using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RadKey
{
    partial class RadKey
    {
        // Build wordlists in cases where only kana was used as input.
        // Matches any words with reading starting with the provided input.
        private List<Compound> buildWordListByReading(string matchOn)
        {
            List<Compound> matchList = new List<Compound>();

            foreach (Compound word in compounds)
            {
                if (word.hasReadingStartingWith(matchOn))
                {
                    matchList.Add(word);
                }
            }

            return matchList;
        }

        // Build wordlists.
        private List<Compound> buildMatchingWordList(string matchOn, List<Compound> lastMatches, int pos, bool isBracketed)
        {
            List<Compound> matchList = new List<Compound>();

            // See if the string is all hiragana.
            bool isHiraganaOnly = true;
            foreach (char letter in matchOn)
            {
                isHiraganaOnly = isHiraganaOnly && new Regex("[\\p{IsHiragana}]").IsMatch(letter.ToString());
            }

            // Iterate over the radical input text string, and get all kanji matching any of those radicals.
            foreach (Compound word in lastMatches)
            {
                // Case 0: Throw out words shorter than the input string. 
                if (word.ToString().Length >= pos + 1) // Adding 1 to pos because pos is the array position offset from zero.
                {
                    // Case 1: See if the search string matches the word outright.
                    if (word.ToString()[pos].ToString() == matchOn)
                    {
                        matchList.Add(word);
                    }
                    // Case 2: Bracketed text. Either radicals or hiragana.
                    else if (isBracketed)
                    {
                        bool wordMatches = true;
                        // Case 2a: String only contains hiragana. 
                        if (isHiraganaOnly)
                        {
                            wordMatches = wordMatches && word.hasReadingContaining(matchOn);
                        }
                        // Case 2b: Assume it's radicals (if it's a mix of radicals, kanji, and kana, the match'll fail safely).
                        else
                        {
                            foreach (char rad in matchOn)
                            {
                                // Checking the kanjiToRad dictionary is needed to stop the system from trying to look up a kana or symbol.
                                if (kanjiToRad.ContainsKey(word.ToString()[pos].ToString()))
                                {
                                    wordMatches = wordMatches && kanjiToRad[word.ToString()[pos].ToString()].Contains(rad);
                                }
                                // Word is not a kanji at this position. Does not match.
                                else
                                {
                                    wordMatches = false;
                                }
                            }
                        }

                        if (wordMatches)
                        {
                            matchList.Add(word);
                        }
                    }
                    // Case 3: Wildcard.
                    else if (matchOn == "*")
                    {
                        matchList.Add(word);
                    }

                    // Case 4: character is a loose radical. See if word[pos] contains that radical.
                    // Checking the kanjiToRad dictionary is needed to stop the system from trying to look up a kana or symbol.
                    else if (kanjiToRad.ContainsKey(word.ToString()[pos].ToString()))
                    {
                        if (kanjiToRad[word.ToString()[pos].ToString()].Contains(matchOn))
                        {
                            matchList.Add(word);
                        }
                    }
                }
            }

            return matchList;
        }

        private List<Compound> matchCompound(string input)
        {
            List<Compound> wordList = new List<Compound>();

            // Ignore blank input.
            if (input == "")
            {
                return new List<Compound> { };
            }
            // Special case for searching by reading.
            if (new Regex("[\\p{IsHiragana}]").IsMatch(input) &&
                !new Regex("[\\p{IsCJKUnifiedIdeographs}\\p{IsKatakana}\\p{IsBasicLatin}]").IsMatch(input))
            {
                wordList = buildWordListByReading(input);
            }
            else
            {
                // Standard multi-radical/multi-kanji wordlist generation
                wordList = compounds;

                // Position of a given character in a compound. This CAN be different from x, in the for loop below.
                int charPos = 0;

                for (int x = 0; x < input.Length; x++) // input must have more than two characters. This will be checked elsewhere.
                {
                    // Bracketed text found. Get a substring.
                    if (input[x] == '[')
                    {
                        // Check that the string doesn't end in a opening bracket.
                        if (x + 1 != input.Length)
                        {
                            int closingBracket = input.IndexOf(']', x);

                            // If a closing bracket wasn't provided, just go to the end of the line.
                            if (closingBracket < 0)
                            {
                                closingBracket = input.Length;
                            }

                            wordList = buildMatchingWordList(input.Substring(x + 1, closingBracket - (x + 1)), wordList, charPos, true);
                            x = closingBracket;
                            charPos++;
                        }
                    }
                    else
                    {
                        wordList = buildMatchingWordList(input[x].ToString(), wordList, charPos, false);
                        charPos++;
                    }
                }
            }

            wordList.Sort();
            return wordList;
        }
    }
}

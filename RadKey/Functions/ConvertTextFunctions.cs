using System;
using System.Text.RegularExpressions;

namespace RadKey
{
    partial class RadKey
    {
        // Given a string containing radicals and English text, find the first occurence of English text, 
        // look up a radical associated with it, and replace the English text with the radical.
        private Tuple<string, int> getRadical(string input, int originalStart, out bool success)
        {
            // If the input is empty, return it as is.
            if (input == "")
            {
                success = false;
                return Tuple.Create(input, 0);
            }
            // Parse the input string to find the first occurence of English text, where English text is
            // defined as text NOT in the dictionary.
            int start = -1;
            int end = -1;

            // First, get the start position.
            for (int x = 0; x < input.Length; x++)
            {
                // See if the character at the current position is English text or *.
                if (!Regex.IsMatch(input[x].ToString(), isSpecialPattern))
                {
                    if (Regex.IsMatch(input[x].ToString(), isLatinPattern))
                    {
                        start = x;
                        end = x; // End is set here too. This will stop one-character radical names from being ignored.
                        break;
                    }
                }
            }

            // If the user only sent radicals/reserved characters, start will still be -1.
            // Return the original start position.
            if (start == -1)
            {
                success = false;
                return Tuple.Create(input, originalStart);
            }

            // Next, get the end position.
            // NOTE TO SELF: Is there a more elegant way to do this?
            for (end = start; end < input.Length; end++)
            {

                if (Regex.IsMatch(input[end].ToString(), isNotLatinPattern))
                {
                    // The charcter at position 'end' is in the dictionary. Return one past the last valid English letter.
                    break;
                }

                else if (Regex.IsMatch(input[end].ToString(), isSpecialPattern))
                {
                    break;
                }

            }

            // Store the substring we want to use to look up the radical.
            // Need to add 1 here to get the correct length.
            string radicalLookup = input.Substring(start, end - start);

            // Initialize the found radical to nothing.
            string radical = "";

            // First, make sure it's a valid lookup string.
            if (radCodes.ContainsKey(radicalLookup))
            {
                radical = radCodes[radicalLookup];
                success = true;
            }
            else
            {
                success = false;
            }


            // Then, insert it into original string and return that.
            // If a radical was not found, this just removes the input text.
            string temp = string.Concat(input.Substring(0, start), radical);

            // If end = the last character of input, we need to handle it differently.
            int newSelectionStart;

            // If the cursor was before the characters being replaced, no need to change it.
            if (originalStart < start)
            {
                newSelectionStart = originalStart;
            }
            else
            {
                newSelectionStart = start + radical.Length;
            }

            if (end == input.Length)
            {
                return Tuple.Create(temp, newSelectionStart);
            }
            else
            {
                return Tuple.Create(string.Concat(temp, input.Substring(end, input.Length - end)), newSelectionStart);
            }

        }

        // Overloaded variant that doesn't need to track success/failure.
        private Tuple<string, int> getRadical(string input, int originalStart)
        {
            bool ignore;
            return getRadical(input, originalStart, out ignore);
        }
    }
}

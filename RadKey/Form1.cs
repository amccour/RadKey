using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;



namespace RadKey
{    
    public partial class RadKey : Form
    {

        // Hotkey Registration Section
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Hotkey Registration Section
        enum KeyModifier
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8
        }

        // Hotkey Registration Section
        protected override void WndProc(ref Message m)
        {
            RegisterHotKey(this.Handle, 0, (int)KeyModifier.Shift + (int)KeyModifier.Control, Keys.Back.GetHashCode());
            
            base.WndProc(ref m);

            if (m.Msg == 0x0312)
            {                                   // The id of the hotkey that was pressed.
                if (this.WindowState == FormWindowState.Normal)
                {
                    this.WindowState = FormWindowState.Minimized;
                    this.ShowInTaskbar = false;
                    RadKeyNotifyIcon.Visible = true;
                }
                else if (this.WindowState == FormWindowState.Minimized)
                {
                    this.WindowState = FormWindowState.Normal;
                    this.ShowInTaskbar = true;
                    RadKeyNotifyIcon.Visible = false;
                }
            }
        }


        // The following dictionaries/list contain the dictionary and graphical decomposition data from JMDict/Edict, 
        // as well as the user-defined radical codes.
        Dictionary<string, string> kanjiToRad = new Dictionary<string, string>();

        Dictionary<string, string> radToKanji = new Dictionary<string, string>();
        
        static Dictionary<string, KanjiData> kanjiDataDictionary = new Dictionary<string,KanjiData>();

        Dictionary<string, string> radCodes = new Dictionary<string, string>();

        List<Compound> compounds = new List<Compound>();



        // Variables used to track previous focus/results selection to make navigation easier.
        int lastResultFocus = -1;
        int lastCompoundResultFocus = -1;
        string lastRadString = "";
        String lastSubmittedCompoundString = "";
        int lastStroke = -1;

        // Flag for filtering out low-frequency Kanji. Off by default.
        public bool noLowFreq = true;

        // Flag for sorting by frequency only. Off by defaul.
        private bool sortByFreq = false;

        // Flag for supressing certain behavior when nothing was found last time.
        private bool found = false;

        // Regex Patterns.
        const string isLatinPattern = "\\p{IsBasicLatin}";
        const string isNotLatinPattern = "\\P{IsBasicLatin}";
        const string isSpecialPattern = "[\\*\\@\\[\\]]";
        const string mainKanaConversioPattern = "[\\@\\[\\]\\*aeiounAEIOUN\\P{IsBasicLatin}]";

        public RadKey()
        {
            InitializeComponent();         
            loadRadkfile();
            loadKanjiDic();
            loadKradfile();
            loadRadCodes();
            loadJMDict();

        }

        private void toggleSortByFreq()
        {
            // Set lastRadString/lastCompoundString to nothing, so that you can regenerate results easily.
            lastRadString = "";
            lastSubmittedCompoundString = "";

            if (sortByFreq == false)
            {
                sortByFreq = true;
                messageBox.Text = "Now sorting by frequency only.";
                frequencySortCB.Checked = true;
            }
            else
            {
                sortByFreq = false;
                messageBox.Text = "Now sorting by stroke count and frequency.";
                frequencySortCB.Checked = false;
            }
        }

        private void toggleNoLowFreq()
        {
            // Set lastRadString to nothing, so that you can regenerate results easily.
            lastRadString = "";

            if (noLowFreq == false)
            {
                noLowFreq = true;
                messageBox.Text = "Now filtering out low frequency kanji.";
                includeLowFreqCB.Checked = false;
            }
            else
            {
                noLowFreq = false;
                messageBox.Text = "Now including low frequency kanji.";
                includeLowFreqCB.Checked = true;
            }
        }         
                
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
            string radical= "";

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

            /*if (radical == "")
            {
                // If invalid text was entered, move the start position back by one. This is used to reset the cursor to the right place.
                // Note that this MUST be done after the first part of the result string is built.
                // Don't do this if start is at 0.
                if (start > 0)
                {
                    start--;
                }
            }*/

            // If end = the last character of input, we need to handle it differently.
            int newSelectionStart;

            // If the cursor was before the characters being replaced, no need to change it.
            if(originalStart < start)
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
       
        // Search for kanji based on radicals/strokes.
        private void searchKanji()
        {
            // If the input criteria didn't change, just move the cursor back to the box.
            int newStroke;
            // StrokeBox content needs to be handled separately.
            if (strokeBox.Text == "")
            {
                newStroke = -1;
            }
            else
            {
                newStroke = Int32.Parse(strokeBox.Text);
            }
            if ((entryField.Text == lastRadString) && (lastRadString != "") && newStroke == lastStroke && found == true)
            {
                resultList.Focus();
                resultList.SelectedIndex = lastResultFocus;
            }
            else
            {
                // Get the kanji results and note the last input criteria.
                lastRadString = entryField.Text;
                if (strokeBox.Text != "")
                {
                    lastStroke = Int32.Parse(strokeBox.Text);
                }
                else
                {
                    lastStroke = -1;
                }
                getKanjiList();
            }
        }
        
        // Shows info about the selected kanji in the message box.
        private void showKanjiSelectionInfo()
        {
            // This also changes the font sizes in addition to updating the text.
            meaningBox.Font = new Font(meaningBox.Font.FontFamily, (float)8.25);
            readingBox.Font = new Font(readingBox.Font.FontFamily, (float)11.25);
            
            KanjiData selected = kanjiDataDictionary[resultList.Text];
            messageBox.Text = string.Concat("Strokes: ", selected.strokes.ToString(), " :: Freq: ", selected.freq.ToString(),
                        " :: Rads: ", kanjiToRad[resultList.Text]);
            readingBox.Text = string.Concat("On: ", selected.onReads, " :: Kun: ", selected.kunReads);
            meaningBox.Text = selected.meanings;
        }

        // Get the first past list of all kanji matching any of the input radicals.
        private List<string> buildMatchingKanjiList(string radInput)
        {
            List<string> matchList = new List<string>();
            
            // Iterate over the radical input text string, and get all kanji matching any of those radicals.
            for (int x = 0; x < radInput.Length; x++)
            {
                // Make sure the input character exists in the radToKanji map.
                if (radToKanji.ContainsKey(radInput[x].ToString()))
                {
                    matchList.Add(radToKanji[radInput[x].ToString()]);
                }
            }

            return matchList;
        }

        // Filter the potentially matching kanji to only include those matching ALL of the radicals.
        private string filterKanji(List<string> matchList)
        {
            string filteredResults = matchList.First();

            // If we had more than one radical provided, do intersects on all of the matching kanji sets.
            if (matchList.Count > 1)
            {
                foreach (string element in matchList)
                {
                    filteredResults = new string(filteredResults.Intersect(element).ToArray());
                }
            }

            return filteredResults;
        }
        
        // Convert the list of kanji characters into sortable/filterable kanji data.
        private List<KanjiData> buildKanjiDataList(string filteredResults)
        {
            List<KanjiData> kanjiDataList = new List<KanjiData>();
            KanjiData tKanjiData;
            bool addFlag = true;
            
            // Was a stroke filter given?
            int strokes = -1;
            if(strokeBox.Text != "")
            {
                strokes = Int32.Parse(strokeBox.Text);
            }

            for (int x = 0; x < filteredResults.Length; x++)
            {
                // Get a KanjiData object from the dictionary, based on the input text.
                tKanjiData = kanjiDataDictionary[filteredResults[x].ToString()];
                // Is a stroke filter given? Don't add kanji that don't match.
                if (strokes > 0)
                {
                    if (tKanjiData.strokes == strokes) 
                    {
                        addFlag = true;
                    }
                    else
                    {
                        addFlag = false;
                    }
                }
                // Are non-frequent kanji filtered out?
                if (noLowFreq == true)
                {
                    if ((tKanjiData.freq == 9999) && (tKanjiData.grade >= 9))
                    {
                        // And this to make sure it was previously set to true.
                        addFlag = false; 
                    }
                    else
                    {
                        addFlag = addFlag && true;
                    }
                }
                // Otherwise add everything.
                if(addFlag == true) 
                {
                    kanjiDataList.Add(tKanjiData);
                }

                // Reset addFlag to true, as this is its base case.
                addFlag = true;
                
            }

            return kanjiDataList;
        }
        
        // Populates the kanji result list. Possibly clean this up in the future if I really need to.
        private void getKanjiList() 
        {
            // Get a list of all kanji matching all of the input radicals.
            List<string> matchList = buildMatchingKanjiList(entryField.Text);

            if (matchList.Count == 0)
            {
                // Box isn't empty; treat this like quick search.   
                if (lastResultFocus > -1)
                {
                    resultList.Focus();
                    resultList.SelectedIndex = lastResultFocus;
                    return;
                }

                messageBox.Text = "No searchable radicals.";
                return;
            }       
            else
            {
                string filteredResults = filterKanji(matchList);

                // Convert result string to a list of kanji.
                // First, make sure we actually HAVE matching kanji.
                if (filteredResults == "")
                {
                    found = false;
                    messageBox.Text = "No matching kanji.";
                    // Don't need to change the last focus or last search set here. The results aren't getting cleared or changed.
                    entryField.Focus();
                }
                else {
                    List<KanjiData> kanjiDataList = buildKanjiDataList(filteredResults);

                    // If a stroke count was given, filter by that.
                        // Make sure there are still kanji left.
                    if (kanjiDataList.Count == 0)
                    {
                        found = false;
                        messageBox.Text = "No matching kanji.";
                        // Don't need to change the last focus or last search set here. The results aren't getting cleared or changed.
                        entryField.Focus();
                        return;
                    }

                    kanjiDataList.Sort();
                    resultList.Items.Clear();
                    resultList.Items.AddRange(kanjiDataList.ToArray());
                    resultList.Focus();
                    resultList.SelectedIndex = 0;
                    showKanjiSelectionInfo();

                    found = true;

                    // Text changed; get rid of the last focused on index for the result list.
                    lastResultFocus = 0;
                }
            }
        }


        // Build wordlists in cases where only kana was used as input.
        // Matches any words with reading starting with the provided input.
        private List<Compound> buildWordListByReading(string matchOn)
        {
            List<Compound> matchList = new List<Compound>();
            
            foreach (Compound word in compounds)
            {
                if(word.hasReadingStartingWith(matchOn))
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
            foreach(char letter in matchOn)
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
            if(input == "")
            {
                return new List<Compound> {};
            }
            // Special case for searching by reading.
            if(new Regex("[\\p{IsHiragana}]").IsMatch(input) &&
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

                for(int x = 0; x < input.Length; x++) // input must have more than two characters. This will be checked elsewhere.
                {
                    // Bracketed text found. Get a substring.
                    if(input[x] == '[')
                    {
                        // Check that the string doesn't end in a opening bracket.
                        if(x+1 != input.Length)
                        {
                            int closingBracket = input.IndexOf(']', x);

                            // If a closing bracket wasn't provided, just go to the end of the line.
                            if(closingBracket < 0)
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

        private void RadKey_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+F? Sort by freq/strokes.
            if (e.Control && e.KeyCode == Keys.F)
            {
                toggleSortByFreq();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Ctrl+J: Hide/show infrequent kanji.
            else if (e.Control && e.KeyCode == Keys.J)
            {
                toggleNoLowFreq();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Ctrl+shift+C: Copy the contents of the Selected Kanji box.
            else if (e.Control && e.Shift && e.KeyCode == Keys.C)
            {
                if(selectedKanjiBox.Text.Length > 0)
                { 
                    Clipboard.SetText(convertToHiragana(selectedKanjiBox.Text));
                }
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Ctrl+shift+E: Copy the contents of the Selected Kanji box as shift-jis bytes.
            else if (e.Control && e.Shift && e.KeyCode == Keys.E)
            {
                if(selectedKanjiBox.Text.Length > 0)
                { 
                    byte[] selectedString = Encoding.GetEncoding("shift-jis").GetBytes(convertToHiragana(selectedKanjiBox.Text));
                
                    Clipboard.SetText(BitConverter.ToString(selectedString).Replace("-", ""));
                }
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Ctrl+Shift+V: Append the contents of the buffer onto the Selected Kanji box.
            else if (e.Control && e.Shift && e.KeyCode == Keys.V)
            {
                selectedKanjiBox.Text = string.Concat(selectedKanjiBox.Text, Clipboard.GetText());
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // +W? Give focus to the stroke count box.
            else if (e.Control && e.KeyCode == Keys.W)
            {
                strokeBox.Focus();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Pin the application.
            else if(e.Control && e.KeyCode == Keys.P)
            {
                this.TopMost = this.TopMost == false;
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // F8 -- Open radicalNames.txt for reference/editing.
            else if (e.KeyCode == Keys.F8)
            {
                System.Diagnostics.Process.Start(@"radicalNames.txt");
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // F9 -- Reload radicalNames.txt
            else if (e.KeyCode == Keys.F9)
            {
                radCodes.Clear();
                if(loadRadCodes() == 0)
                {
                    messageBox.Text = "Radical names reloaded successfully.";
                }
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }
        
        private void entryField_KeyDown(object sender, KeyEventArgs e)
        {
            // Did the user hit space?
            if (e.KeyCode == Keys.Space)
            {
                Tuple<string, int> result = getRadical(entryField.Text, entryField.SelectionStart);

                // Look up the radical based on the input
                entryField.Text = result.Item1;

                // Put the cursor in the right place.
                entryField.SelectionStart = result.Item2;

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Did the user hit enter?
            else if (e.KeyCode == Keys.Enter)
            {
                // Replace any remaining things.
                Tuple<string, int> result = getRadical(entryField.Text, entryField.SelectionStart);

                // Look up the radical based on the input
                entryField.Text = result.Item1;

                // Put the cursor in the right place.
                entryField.SelectionStart = result.Item2;                               

                // The actual focus updating for this is handled in searcKanji. Consider changing that.
                searchKanji();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }

            // Decompose kanji into radicals.
            else if (e.Control && e.KeyCode == Keys.D)
            {
                // Limit this to single kanji decomposition.
                if(entryField.Text.Length == 1)
                {
                    // If the character is a radical, ignore it.
                    if(!radToKanji.ContainsKey(entryField.Text))
                    {
                        // Make sure it's a kanji.
                        if(kanjiToRad.ContainsKey(entryField.Text))
                        {
                            entryField.Text = kanjiToRad[entryField.Text];
                        }
                        else
                        {
                            messageBox.Text = "Unable to decompose this character.";
                        }
                    }
                }
                else
                {
                    messageBox.Text = "A single kanji needs to be in the Entry Field for this to work.";
                }

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Did the user hit down?
            else if (e.KeyCode == Keys.Down)
            {
                Tuple<string, int> result = getRadical(entryField.Text, entryField.SelectionStart);

                // Look up the radical based on the input
                entryField.Text = result.Item1;

                // Put the cursor in the right place.
                entryField.SelectionStart = result.Item2;                
                
                selectedKanjiBox.Focus();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Up: Go to Compound Entry.
            else if (e.KeyCode == Keys.Up)
            {
                Tuple<string, int> result = getRadical(entryField.Text, entryField.SelectionStart);

                // Look up the radical based on the input
                entryField.Text = result.Item1;

                // Put the cursor in the right place.
                entryField.SelectionStart = result.Item2;
                
                compoundEntry.Focus();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }

        }

        private void resultList_KeyDown(object sender, KeyEventArgs e)
        {
            // Shift enter to copy to compound box.
            if (e.Shift && e.KeyCode == Keys.Enter)
            {
                int atPos = compoundEntry.Text.IndexOf("@");
                if (atPos > -1)
                {
                    string tText = compoundEntry.Text;
                    compoundEntry.Text = tText.Substring(0, atPos) + resultList.Text + tText.Substring(atPos + 1, tText.Length - atPos - 1);
                    
                    // Move the cursor to one past where the kanji was inserted.
                    compoundEntry.SelectionStart = atPos + 1;

                    // See if there are other @s.
                    atPos = compoundEntry.Text.IndexOf("@");
                }
                else
                {
                    compoundEntry.AppendText(resultList.Text);
                    compoundEntry.SelectionStart = selectedKanjiBox.Text.Length;
                }

                lastResultFocus = resultList.SelectedIndex;

                // If there are more @s, give focus back to entry box instead.
                if (atPos > -1)
                {
                    entryField.Clear();
                    strokeBox.Clear();
                    entryField.Focus();
                }
                else
                {
                    entryField.Clear();
                    strokeBox.Clear();
                    compoundEntry.Focus();
                }                              
                
                e.SuppressKeyPress = true;
                e.Handled = true;
            }            
            // User hit enter?
            else if (e.KeyCode == Keys.Enter)
            {
                // Grab the selected kanji and store the last result focus.
                // Also we want to try to replace any @ characters first.
                int atPos = selectedKanjiBox.Text.IndexOf("@");
                if(atPos > -1)
                {
                    string tText = selectedKanjiBox.Text;
                    selectedKanjiBox.Text = tText.Substring(0, atPos) + resultList.Text + tText.Substring(atPos+1, tText.Length - atPos-1);

                    // Move the cursor to one past where the kanji was inserted.
                    selectedKanjiBox.SelectionStart = atPos + 1;
                }
                else 
                {
                    selectedKanjiBox.AppendText(resultList.Text);
                    selectedKanjiBox.SelectionStart = selectedKanjiBox.Text.Length;
                }
                lastResultFocus = resultList.SelectedIndex;
                // Clear out the radical entry and give focus to it.
                entryField.Clear();
                strokeBox.Clear();
                entryField.Focus();
            }
            // User hit space?
            else if (e.KeyCode == Keys.Space)
            {
                // STore the last selected item and give focus back to the radical entry box.
                lastResultFocus = resultList.SelectedIndex;
                entryField.Focus();
            }

            // Decompose kanji into radicals.
            // Give focus to the stroke count box.
            else if (e.Control && e.KeyCode == Keys.D)
            {
                    entryField.Text = kanjiToRad[resultList.Text];
                    entryField.Focus();
                    entryField.SelectionStart = entryField.Text.Length;
                    lastResultFocus = resultList.SelectedIndex;
                    e.SuppressKeyPress = true;
                    e.Handled = true;
            }
            // Shift+Up goes back to Selected Kanji.
            else if (e.Shift && e.KeyCode == Keys.Up)
            {
                lastResultFocus = resultList.SelectedIndex;
                selectedKanjiBox.Focus();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }

        }

        private void strokeBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Space)
            {
                entryField.Focus();
                e.Handled = true;
            }
            // Did the user hit space? Populate the kanji.
            else if (e.KeyChar == (char)Keys.Enter)
            {
                // Replace any remaining things.
                Tuple<string, int> result = getRadical(entryField.Text, entryField.SelectionStart);

                // Look up the radical based on the input
                entryField.Text = result.Item1;

                // Put the cursor in the right place.
                entryField.SelectionStart = result.Item2;                    
                searchKanji();
                e.Handled = true;
            }
            else if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void resultList_SelectedIndexChanged(object sender, EventArgs e)
        {
            showKanjiSelectionInfo();
        }


        
        // Minimize to system tray.
        private void RadKey_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                RadKeyNotifyIcon.Visible = true;
                this.ShowInTaskbar = false;
            }

            else if (FormWindowState.Normal == this.WindowState)
            {
                RadKeyNotifyIcon.Visible = false;
            }
        }


        private void RadKeyNotifyIcon_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            RadKeyNotifyIcon.Visible = false;
        }

        // Unregister the hotkey.
        private void RadKey_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterHotKey(this.Handle, 0); 
        }


        
        

        private void showCompoundSelectionInfo(Compound toDisplay)
        {
            // This also changes the font sizes in addition to updating the text.
            meaningBox.Font = new Font(meaningBox.Font.FontFamily, (float)11.25);
            readingBox.Font = new Font(readingBox.Font.FontFamily, (float)9.5);

            meaningBox.Text = toDisplay.pronunciation();
            readingBox.Text = toDisplay.definition();

        }

        private bool inBrackets(string text, int pos)
        {
            int nextClosed = text.IndexOf(']', pos);
            int nextOpen = text.IndexOf('[', pos);

            // Supress this if there's an unmatched closed bracket to the right of SelectionStart.

            // Special case where string is a single ].
            if(text == "]" && pos == 0)
            {
                return true;
            }
            else if ((nextClosed > 0 && nextOpen < 0)
                || (nextClosed > 0 && nextOpen > 0 && nextOpen > nextClosed))
            {
                return true;
            }
            return false;
        }
        
        private Tuple<string, int> radOrKanaConversion(string input, string kanaPattern, int pos)
        {
            // input: String that radical or kana conversions are being done on.
            // kanaPattern: Kana conversion regex.
            // pos: SelectionStart.
            
            // Step 1: Attempt radical conversion.
            bool updatedRadical;
            Tuple<string, int> result = getRadical(input, pos, out updatedRadical);
            if (!updatedRadical)
            {
                // Step 2: Radical not found; do kana conversion.
                result = convertToKanaWithCursorPos(input,
                    kanaPattern,
                    pos);
            }

            return result;
        }
        
        private void compoundEntry_KeyDown(object sender, KeyEventArgs e)
        {
            // Shift + Enter - Conver to hiragana without submitting.
            if (e.Shift && e.KeyCode == Keys.Enter)
            {
                // Convert text to hiragana.
                Tuple<string, int> conversionResults =
                    convertToKanaWithCursorPos(compoundEntry.Text,
                    "[\\@\\[\\]\\*aeiounAEIOUN\\P{IsBasicLatin}]",
                    compoundEntry.SelectionStart);

                compoundEntry.Text = conversionResults.Item1;
                compoundEntry.SelectionStart = conversionResults.Item2;

                e.SuppressKeyPress = true;
                e.Handled = true;
            }

            // Shift + Space  - Also convert to hiragana without submitting.
            else if (e.Shift && e.KeyCode == Keys.Space)
            {
                // Convert text to hiragana.
                Tuple<string, int> conversionResults =
                    convertToKanaWithCursorPos(compoundEntry.Text,
                    "[\\@\\[\\]\\*aeiounAEIOUN\\P{IsBasicLatin}]",
                    compoundEntry.SelectionStart);

                compoundEntry.Text = conversionResults.Item1;
                compoundEntry.SelectionStart = conversionResults.Item2;

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            
            
            // User hit space: Insert brackets, surround text in brackets, and also do radical conversion.
            // Space is not allowed to do kana conversions.
            else if (e.KeyCode == Keys.Space)
            {
                // If only some text is highlighted, surround it in brackets.
                if(compoundEntry.SelectionLength > 0)
                {
                    int newSelectionStart = compoundEntry.SelectionStart + compoundEntry.SelectedText.Length + 1;
                    compoundEntry.SelectedText = "[" + compoundEntry.SelectedText + "]";
                    compoundEntry.SelectionStart = newSelectionStart;
                }
                else
                { 
                    Tuple<string, int> result = getRadical(compoundEntry.Text, compoundEntry.SelectionStart);

                    // If the text didn't change and the cursor is at a ], space past the ].
                    if (compoundEntry.Text == result.Item1)
                    {
                        if(compoundEntry.SelectionStart == compoundEntry.Text.Length)
                        {
                            compoundEntry.Text = compoundEntry.Text + "[]";
                            compoundEntry.SelectionStart = compoundEntry.Text.Length-1;
                        }

                        // Space pressed at end of entry box; add a [].
                        else if (compoundEntry.Text[compoundEntry.SelectionStart] == ']')
                        {
                            compoundEntry.SelectionStart = compoundEntry.SelectionStart + 1;
                        }
                    }

                    // Otherwise do normal radical conversion.
                    else
                    {
                        string originalText = compoundEntry.Text;
                        int originalStart = compoundEntry.SelectionStart;

                        compoundEntry.Text = result.Item1;

                        compoundEntry.SelectionStart = result.Item2;
                        // See if the original SelectionStart was in brackets.
                        if (inBrackets(originalText, originalStart))
                        {
                            // If it WAS originally within brackets, the radical search moved it past the start of the brackets.
                            // Put it back in brackets.
                            if (compoundEntry.Text[compoundEntry.SelectionStart] == '[')
                            {
                                compoundEntry.SelectionStart = compoundEntry.SelectionStart + 1;
                            }
                        }

                    }
                }

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Handle input of { and }.
            else if ((e.Shift && e.KeyCode == Keys.OemOpenBrackets)
            || e.Shift && e.KeyCode == Keys.OemCloseBrackets)
            {
                e.Handled = true;
            }
            // Did the user enter an opening bracket? Add the closing bracket.
            else if(e.KeyCode == Keys.OemOpenBrackets)
            {
                // Track the initial SelectionStart.
                // Needs to be taken first -- Insert makes you lose the selection start I guess.
                int initialSelectionStart = compoundEntry.SelectionStart;

                // Clear out any highlighted text -- new input should overwrite it.
                compoundEntry.SelectedText = "";

                // The general purpose of this is to add a matching closed bracket when the user types an open bracket.
                // If there's already a closed bracket to the left ot the cursor, don't add another closed bracket.
                //if (!in_brackets(compoundEntry.Text, compoundEntry.SelectionStart))
                //{                                                                                
                //    // If the selection start is at position zero, append to the front of the text.
                //    compoundEntry.Text = compoundEntry.Text.Insert(initialSelectionStart, "]");                    
                //}

                if (!inBrackets(compoundEntry.Text, compoundEntry.SelectionStart))
                {
                    // Trying something different: only adding a closing bracket if the next character is a kana, a [, or the end of the textbox.
                    if (compoundEntry.SelectionStart == compoundEntry.Text.Length)
                    {
                        compoundEntry.Text = compoundEntry.Text.Insert(initialSelectionStart, "]");
                    }

                    else if (Regex.IsMatch(compoundEntry.Text[compoundEntry.SelectionStart].ToString(), "[\\p{IsHiragana}\\p{IsKatakana}\\[]"))
                    {
                        compoundEntry.Text = compoundEntry.Text.Insert(initialSelectionStart, "]");
                    }
                }

                // Convert text prior to The [
                Tuple<string, int> result = radOrKanaConversion(compoundEntry.Text.Substring(0, initialSelectionStart),
                    mainKanaConversioPattern, initialSelectionStart);

                //string hPart = convertToHiragana(compoundEntry.Text.Substring(0, initialSelectionStart));
                string Remainder = compoundEntry.Text.Substring(initialSelectionStart);
                initialSelectionStart = result.Item2;
                compoundEntry.Text = result.Item1 + Remainder;

                // Put cursor between the brackets.
                compoundEntry.SelectionStart = initialSelectionStart;
                
                e.Handled = true;
            }

            // Did the user enter a closing bracket?
            else if (e.KeyCode == Keys.OemCloseBrackets)
            {
                // Clear out any highlighted text -- new input should overwrite it.
                compoundEntry.SelectedText = "";                

                // Look up the radicals.
                //Tuple<string, int> result = getRadical(compoundEntry.Text, compoundEntry.SelectionStart);
                Tuple<string, int> result = radOrKanaConversion(compoundEntry.Text, 
                    "[\\@\\[\\]\\*aeiounAEIOUN\\P{IsBasicLatin}]", compoundEntry.SelectionStart);

                // Only add another ] if there wasn't already one present.
                // Also some special handling if trying to enter a ] as the first character/last character.
                if (compoundEntry.SelectionStart == compoundEntry.Text.Length)
                {
                    compoundEntry.Text = result.Item1 + "]";
                }
                else if (compoundEntry.Text[compoundEntry.SelectionStart] != ']')
                {
                    compoundEntry.Text = result.Item1.Insert(result.Item2, "]");
                }
                // Otherwise assume a ] was present and update the text.
                else 
                {
                    compoundEntry.Text = result.Item1;
                }

                // In all cases add 1 to Item2's value since a ] was added that needs to be moved past.
                compoundEntry.SelectionStart = result.Item2 + 1;

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Did the user hit enter?
            else if (e.KeyCode == Keys.Enter)
            {
                // Replace any Japanese punctuation with English ones.
                compoundEntry.Text = compoundEntry.Text.Replace('＊', '*');
                compoundEntry.Text = compoundEntry.Text.Replace('「', '[');
                compoundEntry.Text = compoundEntry.Text.Replace('」', ']');

                // Also handle different wildcard configurations.
                compoundEntry.Text = compoundEntry.Text.Replace("[]", "*");
                compoundEntry.Text = compoundEntry.Text.Replace("[*]", "*");

                // Do kana or radical conversion.
                // If only romaji were entered, this should just do a straight kana conversion anyway, so
                // no need to do the special checks to see if only romaji were present.
                Tuple<string, int> results = radOrKanaConversion(compoundEntry.Text, mainKanaConversioPattern, 
                    compoundEntry.SelectionStart);

                compoundEntry.Text = results.Item1;
                compoundEntry.SelectionStart = results.Item2;

                // Following section was some old matching logic. Shouldn't be needed anymore.

                /*// Do hiragana replacement on any text prior to the first [ and after the last ].
                if (compoundEntry.Text.IndexOf("[") > 0)
                {
                    compoundEntry.Text =
                        convertToHiragana(compoundEntry.Text.Substring(0, compoundEntry.Text.IndexOf("[")))
                        + compoundEntry.Text.Substring(compoundEntry.Text.IndexOf("["));
                }
                if (compoundEntry.Text.LastIndexOf("]") > 0 && compoundEntry.Text.LastIndexOf("]") < compoundEntry.Text.Length - 1)
                {
                    compoundEntry.Text =
                        compoundEntry.Text.Substring(0, compoundEntry.Text.LastIndexOf("]"))
                        + convertToHiragana(compoundEntry.Text.Substring(compoundEntry.Text.LastIndexOf("]")));
                }

                // Replace any radicals if present.
                Tuple<string, int> radChanges = getRadical(compoundEntry.Text, compoundEntry.SelectionStart);

                // Look up the radical based on the input
                compoundEntry.Text = radChanges.Item1;

                // Put the cursor in the right place.
                compoundEntry.SelectionStart = radChanges.Item2;*/


                // Find the matches.
                if (lastSubmittedCompoundString != compoundEntry.Text)
                {                    
                    List<Compound> matchResults = matchCompound(compoundEntry.Text);

                    if (matchResults.Count() > 0)
                    {
                        // Don't update this until we get results -- this should really store the last GOOD string.
                        lastSubmittedCompoundString = compoundEntry.Text;
                        compoundsResults.Items.Clear();
                        compoundsResults.Items.AddRange(matchResults.ToArray());
                        compoundsResults.Focus();
                        compoundsResults.SelectedIndex = 0;
                        showCompoundSelectionInfo((Compound)compoundsResults.SelectedItem);

                        // Clear out old error if present.
                        messageBox.Text = "";

                        // Add the search string to history.
                        // Doing this here to stop adding the same search string multiple times/remove failed searches.
                        SearchHistory.AddHistory(compoundEntry.Text);
                        
                    }
                    else
                    {
                        messageBox.Text = "No matching compounds.";
                    }


                }
                else
                {
                    // Don't change focus if the box is blank.
                    if(compoundsResults.Items.Count > 0)
                    { 
                        compoundsResults.Focus();
                        compoundsResults.SelectedIndex = lastCompoundResultFocus;
                        messageBox.Text = "";

                    }
                }

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Shift+Down goes back to results.
            else if (e.Shift && e.KeyCode == Keys.Down)
            {
                if (compoundsResults.Items.Count > 0)
                {
                    compoundsResults.SelectedIndex = lastCompoundResultFocus;
                    compoundsResults.Focus();
                }
                e.SuppressKeyPress = true;
                e.Handled = true;
            }            
            // Did the user hit down?
            else if (e.KeyCode == Keys.Down)
            {                
                entryField.Focus();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Did the user hit up?
            else if (e.KeyCode == Keys.Up)
            {
                selectedKanjiBox.Focus();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Did the user hit Ctrl+H?
            else if (e.Control && e.KeyCode == Keys.H)
            {
                // Convert text to hiragana.
                Tuple<string, int> conversionResults =
                    convertToKanaWithCursorPos(compoundEntry.Text,
                    "[\\@\\[\\]\\*aeiounAEIOUN\\P{IsBasicLatin}]",
                    compoundEntry.SelectionStart);

                compoundEntry.Text = conversionResults.Item1;
                compoundEntry.SelectionStart = conversionResults.Item2;

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // History keys.
            else if(e.KeyCode == Keys.PageDown)
            {
                compoundEntry.Text = SearchHistory.GetNextFromHistory();
            }
            else if(e.KeyCode == Keys.PageUp)
            {
                compoundEntry.Text = SearchHistory.GetLastFromHistory(compoundEntry.Text);
            }

        }

        private void compoundResults_KeyDown(object sender, KeyEventArgs e)
        {
            // Did the user hit space?
            if (e.KeyCode == Keys.Space)
            {
                lastCompoundResultFocus = compoundsResults.SelectedIndex; // Is this redundant now?
                compoundEntry.Focus();

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Did the user hit enter?
            else if (e.KeyCode == Keys.Enter)
            {
                int atPos = selectedKanjiBox.Text.IndexOf("@");
                if (atPos > -1)
                {
                    string tText = selectedKanjiBox.Text;
                    selectedKanjiBox.Text = tText.Substring(0, atPos) + compoundsResults.Text + tText.Substring(atPos + 1, tText.Length - atPos - 1);

                    // Set The cursor in selectedKanjiBox on past the text that was added.
                    selectedKanjiBox.SelectionStart = atPos + compoundsResults.Text.Length;

                    // See if there are other @s.
                    atPos = selectedKanjiBox.Text.IndexOf("@");           
                }
                else
                {
                    selectedKanjiBox.AppendText(compoundsResults.Text);
                    selectedKanjiBox.SelectionStart = selectedKanjiBox.Text.Length;
                }

                lastCompoundResultFocus = compoundsResults.SelectedIndex;

                // If there are more @s, give focus back to entry box instead.
                if (atPos > -1)
                {
                    // Clear out the compound field entry and give focus to it.
                    compoundEntry.Clear();
                    compoundEntry.Focus();
                }
                else
                {
                    compoundEntry.Clear();
                    selectedKanjiBox.Focus();
                }                

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Shift+Up goes back to Selected Kanji.
            else if (e.Shift && e.KeyCode == Keys.Up)
            {
                lastCompoundResultFocus = compoundsResults.SelectedIndex;
                compoundEntry.Focus();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void compoundsResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            showCompoundSelectionInfo((Compound)compoundsResults.SelectedItem);
        }

        // Store focus in case the user clicks out of a certain field.
        private void compoundsResults_Leave(object sender, EventArgs e)
        {
            lastCompoundResultFocus = compoundsResults.SelectedIndex;
        }

        private void resultList_Leave(object sender, EventArgs e)
        {
            lastResultFocus = resultList.SelectedIndex;                        
        }
    
        private void entryField_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Tab))
            {
                // Replace any remaining things.
                Tuple<string, int> result = getRadical(entryField.Text, entryField.SelectionStart);

                // Look up the radical based on the input
                entryField.Text = result.Item1;

                // Put the cursor in the right place.
                entryField.SelectionStart = result.Item2;    
            }
        }

        

        // These next parts convert Japanese special characterss to English ones.
        private void compoundEntry_Leave(object sender, EventArgs e)
        {
            int currentSelection = compoundEntry.SelectionStart;
            compoundEntry.Text = compoundEntry.Text.Replace('＠', '@');
            compoundEntry.Text = compoundEntry.Text.Replace('＊', '*');
            compoundEntry.Text = compoundEntry.Text.Replace('「', '[');
            compoundEntry.Text = compoundEntry.Text.Replace('」', ']');

            // Length can't change so this is safe.
            compoundEntry.SelectionStart = currentSelection;
        }

        

        private void includeLowFreqCB_Click(object sender, EventArgs e)
        {
            toggleNoLowFreq();
            entryField.Focus();
        }

        private void frequencySortCB_Click(object sender, EventArgs e)
        {
            toggleSortByFreq();
            entryField.Focus();
        }


    }
}


/*
          {"one","一"},
            {"line","｜"},
            {"dot","丶"},
            {"no","ノ"},
            {"z","乙"},
            {"hook","亅"},
            {"ni","二"},
            {"lid","亠"},
            {"hito","人"},
            {"lhto","化"},
            {"thto","个"},
            {"legs","儿"},
            {"enter","入"},
            {"eight","ハ"},
            {"top8","并"},
            {"ubox","冂"},
            {"crown","冖"},
            {"ice","冫"},
            {"desk","几"},
            {"cont","凵"},
            {"sword","刀"},
            {"ri","刈"},
            {"ka","力"},
            {"wrap","勹"},
            {"hi","匕"},
            {"box","匚"},
            {"ten","十"},
            {"to","卜"},
            {"seal","卩"},
            {"cliff","厂"},
            {"mu","厶"},
            {"and","又"},
            {"ma","マ"},
            {"nine","九"},
            {"yu","ユ"},
            {"from","乃"},
            {"walk","込"},
            {"mouth","口"},
            {"encl","囗"},{"enclose","囗"},
            {"earth","土"},
            {"man","士"},
            {"winter","夂"},
            {"twil","夕"}, {"twilight","夕"}, {"sunset","夕"}, {"evening","夕"},
            {"big","大"},
            {"woman","女"},
            {"child","子"},
            {"roof","宀"},
            {"inch","寸"},
            {"small","小"},
            {"3dot","尚"},
            {"lame","尢"},
            {"flag","尸"},
            {"sprout","屮"},
            {"mount","山"},
            {"river","川"},
            {"lrvr","巛"},{"lriver","巛"},
            {"e","工"},
            {"self","已"},
            {"cloth","巾"},
            {"dry","干"},
            {"thread","幺"},
            {"dcliff","广"},
            {"stride","廴"},
            {"H","廾"},
            {"crmny","弋"}, {"ceremony","弋"}, 
            {"bow","弓"},
            {"yo","ヨ"},
            {"phed","彑"},{"pighead","彑"},
            {"three","彡"},
            {"step","彳"},
            {"lhrt","忙"},{"lheart","忙"},
            {"lhnd","扎"},{"lhand","扎"},
            {"lwtr","汁"},{"lwater","汁"},
            {"ldog","犯"},
            {"grass","艾"},
            {"rb","邦"},
            {"lb","阡"},
            {"G","也"},
            {"dead","亡"},
            {"reach","及"},
            {"while","久"},
            {"old","老"},
            {"heart","心"},
            {"spear","戈"},
            {"door","戸"},
            {"hand","手"},
            {"branch","支"},
            {"whip","攵"},
            {"lit","文"}, {"literature","文"},
            {"dip","斗"},
            {"axe","斤"},
            {"kata","方"},
            {"nai1","无"},
            {"sun","日"},
            {"say","曰"},
            {"moon","月"},
            {"tree","木"},
            {"yawn","欠"},
            {"stop","止"},
            {"decay","歹"},
            {"wepn","殳"}, {"weapon","殳"}, {"deskand","殳"},
            {"comp","比"}, {"compare","比"},
            {"fur","毛"},
            {"clan","氏"},
            {"steam","气"},
            {"water","水"},
            {"fire","火"},
            {"4dot","杰"},
            {"claw","爪"},
            {"father","父"},
            {"mix","爻"},
            {"wood","爿"},
            {"slice","片"},
            {"cow","牛"},
            {"dog","犬"},
            {"ne","礼"},
            {"king","王"},
            {"base","元"},
            {"well","井"},
            {"mono","勿"},
            {"superb","尤"},
            {"five","五"},
            {"ton","屯"},
            {"tomoe","巴"},
            {"must","毋"},
            {"dark","玄"},
            {"title","瓦"},
            {"sweet","甘"},
            {"life","生"},
            {"use","用"},
            {"field","田"},
            {"bolt","疋"},
            {"sick","疔"},
            {"tent","癶"},
            {"white","白"},
            {"skin","皮"},
            {"dish","皿"},
            {"eye","目"},
            {"pike","矛"},
            {"arrow","矢"},
            {"rock","石"}, {"stone","石"},
            {"show","示"},
            {"ytg","禹"},
            {"2tree","禾"},
            {"hole","穴"},
            {"stand","立"},
            {"new","初"},
            {"age","世"},
            {"huge","巨"},
            {"book","冊"},
            {"mom","母"},
            {"net","買"},
            {"fang","牙"},
            {"melon","瓜"},
            {"bamboo","竹"}, {"bb","竹"},
            {"light","米"},
            {"yarn","糸"},
            {"jar","缶"},
            {"sheep","羊"},
            {"wing","羽"},
            {"rake","而"},
            {"plow","耒"},
            {"ear","耳"},
            {"brush","聿"},
            {"meat","肉"},
            {"mizukara","自"},
            {"arrive","至"},
            {"mortar","臼"},
            {"tongue","舌"},
            {"boat","舟"},
            {"yoku","艮"},
            {"color","色"},
            {"tiger","虍"},
            {"bug","虫"},
            {"blood","血"},
            {"go","行"},
            {"robe","衣"},
            {"west","西"},
            {"omi","臣"},
            {"see","見"},
            {"horn","角"},
            {"iu","言"},
            {"valley","谷"},
            {"bean","豆"},
            {"pig","豕"},
            {"cat","豸"},
            {"shell","貝"},
            {"red","赤"},
            {"run","走"},
            {"foot","足"},
            {"body","身"},
            {"car","車"},
            {"spice","辛"},
            {"morning","辰"},
            {"sake","酉"},
            {"divide","釆"},
            {"village","里"},
            {"dance","舛"},
            {"wheat","麦"},
            {"gold","金"},
            {"long","長"},
            {"gate","門"},
            {"slave","隶"},
            {"dare","隹"},
            {"rain","雨"},
            {"blue","青"},
            {"wrong","非"},
            {"cover","奄"},
            {"hill","岡"},
            {"excuse","免"},
            {"qi","斉"},
            {"face","面"},
            {"hide","革"},
            {"leek","韭"},
            {"sound","音"},
            {"page","頁"},
            {"wind","風"},
            {"fly","飛"}, 
            {"eat","食"}, {"food","食"},
            {"neck","首"},
            {"smell","香"},
            {"goods","品"},
            {"horse","馬"},
            {"bone","骨"},
            {"tall","高"},
            {"hair","髟"},
            {"fight","鬥"},
            {"herb","鬯"},
            {"tripod","鬲"},
            {"demon","鬼"},
            {"dragon","竜"},
            {"tanned","韋"},
            {"fish","魚"},
            {"bird","鳥"},
            {"salt","鹵"},
            {"deer","鹿"},
            {"hemp","麻"},
            {"turtle","亀"},
            {"drop","滴"},
            {"yellow","黄"},
            {"black","黒"},
            {"millet","黍"},
            {"embr","黹"},
            {"nai2","無"},
            {"tooth","歯"},
            {"frog","黽"},
            {"ding","鼎"},
            {"drum","鼓"},
            {"rat","鼠"},
            {"nose","鼻"},
            {"even","齊"},
            {"flute","龠"},
            {"8","ハ"},{"entr","入"}, {"5","五"}, {"3white","自"}, 
            {"2","二"}, {"two","二"}, {"9","九"}, {"3","彡"}, {"comma","巴"},
*/
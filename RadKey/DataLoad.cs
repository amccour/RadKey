using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace RadKey
{
    public partial class RadKey : Form
    {
        private int loadRadCodes()
        {
            string[] radicalNames = System.IO.File.ReadAllLines(@"radicalNames.txt");
            string[] tokens;
            for (int x = 0; x < radicalNames.Count(); x++)
            {
                // See if it's blank line.
                if (radicalNames[x] != "")
                {
                    // See if it's comment line.
                    if (radicalNames[x][0] != '#')
                    {
                        // Tokenize it.
                        tokens = radicalNames[x].Trim().Split(' ');
                        // Ignore empty token set.
                        if (tokens.Count() != 0)
                        {
                            // Not checking to elimintate non-radical/multichar 'radicals'. There may be reasons for allowing these.
                            // Were names given?
                            if (tokens.Count() == 1)
                            {
                                messageBox.Text = string.Concat("Un-named radical ", tokens[0]);
                                return 1;
                            }
                            else
                            {
                                // Create dictionary entries for each radical name.
                                for (int y = 1; y < tokens.Count(); y++)
                                {
                                    // See if we're trying to add the same key twice.
                                    if (radCodes.ContainsKey(tokens[y]))
                                    {
                                        messageBox.Text = string.Concat("Non-unique key: ", tokens[y]);
                                        return 1;
                                    }
                                    // Otherwise, it's safe to add.
                                    radCodes.Add(tokens[y], tokens[0]);
                                }
                            }
                        }
                    }
                }
            }

            return 0;
        }

        private void loadKanjiDic()
        {
            string[] kanjidicLines = System.IO.File.ReadAllLines(@"kanjidic_utf8.txt");

            string buffer = "";
            string tKanji = "";
            string[] tokens;

            int meaningStart = -1;
            string onReads;
            string kunReads;

            int tStrokes = 0;
            int tFreq = 0;
            int tGrade = 0;

            // Parse the file, starting at line 2. Line 1 is a header.
            for (int x = 1; x < 6356; x++)
            {
                // Load and tokenize the current string.
                onReads = "";
                kunReads = "";
                buffer = kanjidicLines[x].Trim();
                tokens = buffer.Split(' ');
                // Get the kanji.
                tKanji = tokens[0];
                // Ideally, Freq always comes after Stroke in kanjidic. However, I can't actually prove this.
                // So, I need to search for them separately.
                // Set tStrokes to 99 in case the stroke count is undefined.
                tStrokes = 99;
                // I think it is safe to assume that strokes will be within the first ten tokens. Same for frequency. 
                for (int y = 1; y < 11; y++)
                {
                    // Found the stroke token (the stroken).
                    if (tokens[y][0] == 'S')
                    {
                        tStrokes = Int32.Parse(tokens[y].Substring(1));
                        break;
                    }
                }
                // Same approach for frequency.
                tFreq = 9999;
                for (int y = 1; y < tokens.Count(); y++)
                {
                    // Found the stroke token (the stroken).
                    if (tokens[y][0] == 'F')
                    {
                        tFreq = Int32.Parse(tokens[y].Substring(1));
                        break;
                    }
                }

                // Same approach for grade.
                tGrade = 99;
                for (int y = 1; y < tokens.Count(); y++)
                {
                    // Found the stroke token (the stroken).
                    if (tokens[y][0] == 'G')
                    {
                        tGrade = Int32.Parse(tokens[y].Substring(1));
                        break;
                    }
                }

                // Find the ON- or KUN- readings in the tokens.
                foreach (string element in tokens)
                {
                    // See if the strting chracter is in the katakana range. If so, it's on.
                    if ((element[0] >= 0x30A0) && (element[0] <= 0x30FF))
                    {
                        onReads = string.Concat(element, " / ", onReads);
                    }
                    // See if the strting chracter is in the hiragana range. If so, it's kun.
                    else if ((element[0] >= 0x3040) && (element[0] <= 0x309F))
                    {
                        kunReads = string.Concat(element, " / ", kunReads);
                    }
                }

                // Find the meanings start position.
                meaningStart = buffer.IndexOf('{');

                // Add the kanji info to the dictionary.
                kanjiDataDictionary.Add(tKanji, new KanjiData(tKanji, tStrokes, tFreq, tGrade, onReads, kunReads, buffer.Substring(meaningStart, buffer.Length - meaningStart), this));
            }
        }

        private void loadKradfile()
        {
            string[] kradfileLines = System.IO.File.ReadAllLines(@"kradfile_utf8.txt");

            int start = 99;     // Starting line in kradfile. 99 maps to 100 in the textfile.
            int size = 6454;    // Current length of kradfile.

            string tKanji = "";
            string tRads = "";

            // Parse the file, starting at line 100. Line 1 is a header.
            for (int x = start; x < size; x++)
            {
                // Get the kanji.
                tKanji = kradfileLines[x][0].ToString();
                // Get the radicals. These start at position 4.
                tRads = kradfileLines[x].Substring(4);
                // Remove the spaces.
                tRads = tRads.Replace(" ", "");

                // Add the kanji info to the dictionary.
                kanjiToRad.Add(tKanji, tRads);
            }
        }

        private void loadRadkfile()
        {
            string[] radkfileLines = System.IO.File.ReadAllLines(@"radkfile_utf8.txt");

            // REMOVE THE HARDCODING ON THESE AT SOME POINT.
            int start = 45;     // Stratig lne in radkfile.
            int size = 1141;    // Current length of radkfile.
            string buffer = "";
            string currentRad = "";
            string kanjiSet = "";

            for (int x = start - 1; x < size; x++)
            {
                buffer = radkfileLines[x];
                // If the line starts with $, we have a new radical.
                if (buffer.Substring(0, 1) == "$")
                {
                    // Write the old radical and kanji set to the dictionary.
                    radToKanji.Add(currentRad, kanjiSet);
                    // Store the new radical.
                    currentRad = buffer.Substring(2, 1);
                    // Clear the kanji set.
                    kanjiSet = "";
                }
                // Line of kanji.
                else
                {
                    // Append it to the current kanjiset.
                    kanjiSet = string.Concat(kanjiSet, buffer);
                }
            }
            // Add the last line, which isn't captured by the loop.
            radToKanji.Add(currentRad, kanjiSet);
        }

        private void loadJMDict()
        {
            // Store temporary data for the next compound to load.
            List<string> nextKeb = new List<string>();
            List<string> nextReb = new List<string>();
            List<string> nextGloss = new List<string>();

            bool uk = false;
            string miscTag = "";

            XmlTextReader JMDict = new XmlTextReader("JMdict_e");
            while (JMDict.Read())
            {

                switch (JMDict.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (JMDict.Name)
                        {
                            case "keb":
                                // Read again to get the value.
                                JMDict.Read();
                                nextKeb.Add(JMDict.Value);
                                break;
                            case "reb":
                                JMDict.Read();
                                nextReb.Add(JMDict.Value);
                                break;
                            case "gloss":
                                JMDict.Read();

                                if (miscTag != "")
                                {
                                    nextGloss.Add(JMDict.Value + " " + miscTag);
                                    miscTag = "";
                                }
                                else
                                {
                                    nextGloss.Add(JMDict.Value);
                                }

                                /*if (nextGloss == "")
                                {
                                    nextGloss = JMDict.Value;
                                }
                                else
                                {
                                    // nextGloss appends, as there could be multiple glosses?
                                    // Don't do this if it's The first gloss though.
                                    nextGloss = nextGloss + "; " + JMDict.Value;
                                }

                                if (miscTag != "")
                                {
                                    nextGloss = nextGloss + miscTag;
                                    miscTag = "";
                                }*/
                                break;
                            case "misc":
                                JMDict.Read();

                                if(JMDict.Name == "uk")
                                {
                                    uk = true;
                                }

                                miscTag = miscTag + " (" + JMDict.Name + ")";
                             
                                break;
                        }
                        break;
                    case XmlNodeType.Text:
                        break;
                    case XmlNodeType.EndElement:
                        // Entry ended. Add compound and clear out the next___ stuff.
                        if (JMDict.Name == "entry")
                        {
                            // Add every compound entry.
                            if (nextKeb.Count > 0)
                            {
                                // Word is usually written in kana. Add redundant kana entries, and tag as such.
                                // Technically UK is on a per-definition basis but I don't really have that level of granularity
                                // and it might be misleading anyway as that could hide definitions.
                                if(uk == true)
                                {
                                    foreach (string rebOnly in nextReb)
                                    {
                                        compounds.Add(new Compound(rebOnly, nextGloss, nextReb));
                                    }
                                }                                
                                foreach (string compound in nextKeb)
                                {
                                    compounds.Add(new Compound(compound, nextGloss, nextReb));
                                }
                            }
                            // Word is kana only. Add as such.
                            else
                            {
                                foreach (string rebOnly in nextReb)
                                {
                                    compounds.Add(new Compound(rebOnly, nextGloss, nextReb));
                                }
                            }

                            nextKeb = new List<string>();
                            nextReb = new List<string>();
                            nextGloss = new List<string>();
                            uk = false;
                            miscTag = "";

                        }
                        break;
                }
            }
        }
    }
}
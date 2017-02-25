using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RadKey
{
    public partial class RadKey : Form
    {
        public class KanjiData : IComparable
        {
            // The actual kanji name is stored in the dictionary's key.
            public readonly string kString;
            public readonly int strokes;
            public readonly int freq;
            public readonly int grade;
            public string onReads;
            public string kunReads;
            public string meanings;
            private RadKey parent;

            public KanjiData(string _kString, int _strokes, int _freq, int _grade, string _onReads, string _kunReads, string _meanings, RadKey _parent)
            {
                kString = _kString;
                strokes = _strokes;
                freq = _freq;
                grade = _grade;
                onReads = _onReads;
                kunReads = _kunReads;
                meanings = _meanings;
                parent = _parent;
            }

            public override string ToString()
            {
                return kString;
            }

            public int CompareTo(object obj)
            {
                if (obj == null) return 1;

                if (obj is KanjiData)
                {
                    KanjiData otherKanjiData = obj as KanjiData;

                    // Sort differently depending on what sortByFreq is set to.
                    if (parent.sortByFreq == false)
                    {

                        // If the stroke counts are different, compare on that; otherwise, if the stroke counts are the same, compare on frequency.
                        if (this.strokes.CompareTo(otherKanjiData.strokes) == 0)
                        {
                            return this.freq.CompareTo(otherKanjiData.freq);
                        }
                        return this.strokes.CompareTo(otherKanjiData.strokes);
                    }
                    else
                    {
                        // Sort by hex value if freq == 9999. 
                        if(this.freq == 9999 && otherKanjiData.freq == 9999)
                        {
                            return string.Compare(this.ToString(), otherKanjiData.ToString());
                        }
                        else
                        { 
                            return this.freq.CompareTo(otherKanjiData.freq);
                        }
                    }
                }
                throw new ArgumentException("Object is not a User");
            }
        }


        public class Compound : IComparable
        {
            private string keb;
            private string gloss;
            private List<string> reb;

            public Compound(string _keb, string _gloss, List<string> _reb)
            {
                keb = _keb;
                gloss = _gloss;
                reb = _reb;
            }

            public override string ToString()
            {
                return keb;
            }

            public string definition()
            {
                return gloss;
            }

            public string pronunciation()
            {
                string rebString;
                if (reb.Count == 1)
                {
                    rebString = reb[0];
                }
                else
                {
                    rebString = reb[0];
                    for (int x = 1; x < reb.Count; x++)
                    {
                        rebString = rebString + "; " + reb[x];
                    }
                }
                return rebString;
            }

            public void addDefinition(string newDef)
            {
                gloss = gloss + "; " + newDef;
            }

            public int CompareTo(object obj)
            {
                if (obj == null) return 1;

                if (obj is Compound)
                {
                    Compound otherCompound = obj as Compound;

                    string thisString = this.ToString();
                    string otherString = otherCompound.ToString();

                    KanjiData thisKanji = null;
                    KanjiData otherKanji = null;

                    // Check 0: Are they the same word?
                    if (thisString == otherString)
                    {
                        return string.Compare(this.ToString(), otherCompound.ToString());
                    }

                    // Check 1: Shorter strings come first.
                    if (thisString.Length != otherString.Length)
                    {
                        return thisString.Length.CompareTo(otherString.Length);
                    }
                    else
                    {
                        // Find the first character that doesn't match.
                        for (int x = 0; x < thisString.Length; x++)
                        {
                            if (thisString[x] != otherString[x])
                            {
                                // Check if it's in the kanjiDictionary. If not, assume it's a kana. If it is a kanji, do kanji comparison.                                                               
                                kanjiDataDictionary.TryGetValue(thisString[x].ToString(), out thisKanji);
                                kanjiDataDictionary.TryGetValue(otherString[x].ToString(), out otherKanji);

                                if (thisKanji != null && otherKanji != null)
                                {
                                    return thisKanji.CompareTo(otherKanji);
                                }
                                else
                                {
                                    return string.Compare(this.ToString(), otherCompound.ToString());
                                }
                            }
                        }

                        return string.Compare(this.ToString(), otherCompound.ToString());
                    }

                }
                throw new ArgumentException("Object is not a User");
            }

            public bool hasReading(string reading)
            {
                if (reb.Contains(reading))
                {
                    return true;
                }

                return false;

            }

            public bool hasReadingStartingWith(string reading)
            {
                foreach (string rebReading in reb)
                {
                    if (rebReading.StartsWith(reading))
                    {
                        return true;
                    }
                }

                return false;

            }

            public bool hasReadingContaining(string reading)
            {
                foreach (string rebReading in reb)
                {
                    if (rebReading.Contains(reading))
                    {
                        return true;
                    }
                }

                return false;

            }
        }
    }
}

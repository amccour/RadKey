using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadKey
{
    partial class RadKey
    {
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
    }
}

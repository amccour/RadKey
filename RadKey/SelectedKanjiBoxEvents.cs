using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace RadKey
{
    public partial class RadKey : Form
    {
        private void selectedKanjiBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Tuple<string, int> conversionResults =
                    convertToKanaWithCursorPos(selectedKanjiBox.Text,
                    "[\\@aeiounAEIOUN\\P{IsBasicLatin}]",
                    selectedKanjiBox.SelectionStart);

                selectedKanjiBox.Text = conversionResults.Item1;
                selectedKanjiBox.SelectionStart = conversionResults.Item2;

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            if (e.Shift && e.KeyCode == Keys.Space)
            {
                Tuple<string, int> conversionResults =
                    convertToKanaWithCursorPos(selectedKanjiBox.Text,
                    "[\\@aeiounAEIOUN\\P{IsBasicLatin}]",
                    selectedKanjiBox.SelectionStart);

                selectedKanjiBox.Text = conversionResults.Item1;
                selectedKanjiBox.SelectionStart = conversionResults.Item2;

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up)
            {
                // Convert text to hiragana first.
                Tuple<string, int> conversionResults =
                    convertToKanaWithCursorPos(selectedKanjiBox.Text,
                    "[\\@aeiounAEIOUN\\P{IsBasicLatin}]",
                    selectedKanjiBox.SelectionStart);

                selectedKanjiBox.Text = conversionResults.Item1;
                selectedKanjiBox.SelectionStart = conversionResults.Item2;

                entryField.Focus();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Did the user hit Ctrl+H?
            else if (e.Control && e.KeyCode == Keys.H)
            {
                // Convert text to hiragana.
                Tuple<string, int> conversionResults =
                    convertToKanaWithCursorPos(selectedKanjiBox.Text,
                    "[\\@aeiounAEIOUN\\P{IsBasicLatin}]",
                    selectedKanjiBox.SelectionStart);

                selectedKanjiBox.Text = conversionResults.Item1;
                selectedKanjiBox.SelectionStart = conversionResults.Item2;

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Shift+Down goes back to results.
            else if (e.Shift && e.KeyCode == Keys.Down)
            {
                if (resultList.Items.Count > 0)
                {
                    Tuple<string, int> conversionResults =
                        convertToKanaWithCursorPos(selectedKanjiBox.Text,
                        "[\\@aeiounAEIOUN\\P{IsBasicLatin}]",
                        selectedKanjiBox.SelectionStart);

                    selectedKanjiBox.Text = conversionResults.Item1;
                    selectedKanjiBox.SelectionStart = conversionResults.Item2;

                    resultList.SelectedIndex = lastResultFocus;
                    resultList.Focus();
                }
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            // Did the user hit down?
            else if (e.KeyCode == Keys.Down)
            {
                Tuple<string, int> conversionResults =
                    convertToKanaWithCursorPos(selectedKanjiBox.Text,
                    "[\\@aeiounAEIOUN\\P{IsBasicLatin}]",
                    selectedKanjiBox.SelectionStart);

                selectedKanjiBox.Text = conversionResults.Item1;
                selectedKanjiBox.SelectionStart = conversionResults.Item2;

                compoundEntry.Focus();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void selectedKanjiBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Tab))
            {
                Tuple<string, int> conversionResults =
                    convertToKanaWithCursorPos(selectedKanjiBox.Text,
                    "[\\@aeiounAEIOUN\\P{IsBasicLatin}]",
                    selectedKanjiBox.SelectionStart);

                selectedKanjiBox.Text = conversionResults.Item1;
                selectedKanjiBox.SelectionStart = conversionResults.Item2;
            }
        }

        private void selectedKanjiBox_Leave(object sender, EventArgs e)
        {
            int currentSelection = selectedKanjiBox.SelectionStart;
            selectedKanjiBox.Text = selectedKanjiBox.Text.Replace('＠', '@');
            selectedKanjiBox.SelectionStart = currentSelection;
        }
    }
}
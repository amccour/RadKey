namespace RadKey
{
    partial class RadKey
    {
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
    }
}

using System;

namespace RadKey
{
    partial class RadKey
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            
            this.entryField = new System.Windows.Forms.TextBox();
            this.radLabel = new System.Windows.Forms.Label();
            this.selectedKanjiLabel = new System.Windows.Forms.Label();
            this.selectedKanjiBox = new System.Windows.Forms.TextBox();
            this.resultList = new System.Windows.Forms.ListBox();
            this.messageBox = new System.Windows.Forms.TextBox();
            this.strokeLabel = new System.Windows.Forms.Label();
            this.strokeBox = new System.Windows.Forms.TextBox();
            this.readingBox = new System.Windows.Forms.TextBox();
            this.meaningBox = new System.Windows.Forms.TextBox();
            this.compoundEntry = new System.Windows.Forms.TextBox();
            this.ambiguousLabel = new System.Windows.Forms.Label();
            this.compoundsResults = new System.Windows.Forms.ListBox();
            this.includeLowFreqCB = new System.Windows.Forms.CheckBox();
            this.frequencySortCB = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // entryField
            // 
            this.entryField.Font = new System.Drawing.Font("MS Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.entryField.Location = new System.Drawing.Point(82, 8);
            this.entryField.Name = "entryField";
            this.entryField.Size = new System.Drawing.Size(176, 20);
            this.entryField.TabIndex = 0;
            this.entryField.KeyDown += new System.Windows.Forms.KeyEventHandler(this.entryField_KeyDown);
            this.entryField.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.entryField_PreviewKeyDown);
            // 
            // radLabel
            // 
            this.radLabel.AutoSize = true;
            this.radLabel.Location = new System.Drawing.Point(3, 9);
            this.radLabel.Name = "radLabel";
            this.radLabel.Size = new System.Drawing.Size(73, 13);
            this.radLabel.TabIndex = 1;
            this.radLabel.Text = "Radical Entry:";
            // 
            // selectedKanjiLabel
            // 
            this.selectedKanjiLabel.AutoSize = true;
            this.selectedKanjiLabel.Location = new System.Drawing.Point(3, 37);
            this.selectedKanjiLabel.Name = "selectedKanjiLabel";
            this.selectedKanjiLabel.Size = new System.Drawing.Size(78, 13);
            this.selectedKanjiLabel.TabIndex = 2;
            this.selectedKanjiLabel.Text = "Selected Kanji:";
            // 
            // selectedKanjiBox
            // 
            this.selectedKanjiBox.Font = new System.Drawing.Font("MS Gothic", 8.5F);
            this.selectedKanjiBox.ImeMode = System.Windows.Forms.ImeMode.Hiragana;
            this.selectedKanjiBox.Location = new System.Drawing.Point(82, 33);
            this.selectedKanjiBox.Name = "selectedKanjiBox";
            this.selectedKanjiBox.Size = new System.Drawing.Size(286, 19);
            this.selectedKanjiBox.TabIndex = 2;
            this.selectedKanjiBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.selectedKanjiBox_KeyDown);
            this.selectedKanjiBox.Leave += new System.EventHandler(this.selectedKanjiBox_Leave);
            this.selectedKanjiBox.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.selectedKanjiBox_PreviewKeyDown);
            // 
            // resultList
            // 
            this.resultList.ColumnWidth = 60;
            this.resultList.Font = new System.Drawing.Font("MS Gothic", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.resultList.FormattingEnabled = true;
            this.resultList.ItemHeight = 21;
            this.resultList.Location = new System.Drawing.Point(6, 177);
            this.resultList.MultiColumn = true;
            this.resultList.Name = "resultList";
            this.resultList.Size = new System.Drawing.Size(362, 172);
            this.resultList.TabIndex = 3;
            this.resultList.SelectedIndexChanged += new System.EventHandler(this.resultList_SelectedIndexChanged);
            this.resultList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.resultList_KeyDown);
            this.resultList.Leave += new System.EventHandler(this.resultList_Leave);
            // 
            // messageBox
            // 
            this.messageBox.Font = new System.Drawing.Font("MS Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.messageBox.Location = new System.Drawing.Point(6, 60);
            this.messageBox.Name = "messageBox";
            this.messageBox.ReadOnly = true;
            this.messageBox.Size = new System.Drawing.Size(362, 19);
            this.messageBox.TabIndex = 5;
            this.messageBox.TabStop = false;
            // 
            // strokeLabel
            // 
            this.strokeLabel.AutoSize = true;
            this.strokeLabel.Location = new System.Drawing.Point(283, 9);
            this.strokeLabel.Name = "strokeLabel";
            this.strokeLabel.Size = new System.Drawing.Size(46, 13);
            this.strokeLabel.TabIndex = 6;
            this.strokeLabel.Text = "Strokes:";
            // 
            // strokeBox
            // 
            this.strokeBox.Location = new System.Drawing.Point(335, 6);
            this.strokeBox.MaxLength = 4;
            this.strokeBox.Name = "strokeBox";
            this.strokeBox.Size = new System.Drawing.Size(33, 20);
            this.strokeBox.TabIndex = 1;
            this.strokeBox.TabStop = false;
            this.strokeBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.strokeBox_KeyPress);
            // 
            // readingBox
            // 
            this.readingBox.Font = new System.Drawing.Font("MS Gothic", 11.5F);
            this.readingBox.Location = new System.Drawing.Point(6, 122);
            this.readingBox.Multiline = true;
            this.readingBox.Name = "readingBox";
            this.readingBox.ReadOnly = true;
            this.readingBox.Size = new System.Drawing.Size(362, 49);
            this.readingBox.TabIndex = 7;
            this.readingBox.TabStop = false;
            // 
            // meaningBox
            // 
            this.meaningBox.Font = new System.Drawing.Font("MS Gothic", 8.5F);
            this.meaningBox.Location = new System.Drawing.Point(6, 86);
            this.meaningBox.Multiline = true;
            this.meaningBox.Name = "meaningBox";
            this.meaningBox.ReadOnly = true;
            this.meaningBox.Size = new System.Drawing.Size(362, 30);
            this.meaningBox.TabIndex = 8;
            this.meaningBox.TabStop = false;

            // compoundEntry
            // 
            this.compoundEntry.Font = new System.Drawing.Font("MS Gothic", 12.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.compoundEntry.Location = new System.Drawing.Point(374, 27);
            this.compoundEntry.Name = "compoundEntry";
            this.compoundEntry.Size = new System.Drawing.Size(154, 24);
            this.compoundEntry.TabIndex = 9;
            this.compoundEntry.KeyDown += new System.Windows.Forms.KeyEventHandler(this.compoundEntry_KeyDown);
            this.compoundEntry.Leave += new System.EventHandler(this.compoundEntry_Leave);
            // 
            // ambiguousLabel
            // 
            this.ambiguousLabel.AutoSize = true;
            this.ambiguousLabel.Location = new System.Drawing.Point(374, 9);
            this.ambiguousLabel.Name = "ambiguousLabel";
            this.ambiguousLabel.Size = new System.Drawing.Size(61, 13);
            this.ambiguousLabel.TabIndex = 10;
            this.ambiguousLabel.Text = "Compound:";
            // 
            // compoundsResults
            // 
            this.compoundsResults.Font = new System.Drawing.Font("MS Gothic", 15F);
            this.compoundsResults.FormattingEnabled = true;
            this.compoundsResults.ItemHeight = 20;
            this.compoundsResults.Location = new System.Drawing.Point(374, 60);
            this.compoundsResults.Name = "compoundsResults";
            this.compoundsResults.Size = new System.Drawing.Size(154, 244);
            this.compoundsResults.TabIndex = 11;
            this.compoundsResults.SelectedIndexChanged += new System.EventHandler(this.compoundsResults_SelectedIndexChanged);
            this.compoundsResults.KeyDown += new System.Windows.Forms.KeyEventHandler(this.compoundResults_KeyDown);
            this.compoundsResults.Leave += new System.EventHandler(this.compoundsResults_Leave);
            // 
            // includeLowFreqCB
            // 
            this.includeLowFreqCB.AutoSize = true;
            this.includeLowFreqCB.Location = new System.Drawing.Point(374, 320);
            this.includeLowFreqCB.Name = "includeLowFreqCB";
            this.includeLowFreqCB.Size = new System.Drawing.Size(160, 17);
            this.includeLowFreqCB.TabIndex = 12;
            this.includeLowFreqCB.TabStop = false;
            this.includeLowFreqCB.Text = "Low-Frequency Kanji (Ctrl+J)";
            this.includeLowFreqCB.UseVisualStyleBackColor = true;
            this.includeLowFreqCB.Click += new System.EventHandler(this.includeLowFreqCB_Click);
            // 
            // frequencySortCB
            // 
            this.frequencySortCB.AutoSize = true;
            this.frequencySortCB.Location = new System.Drawing.Point(374, 339);
            this.frequencySortCB.Name = "frequencySortCB";
            this.frequencySortCB.Size = new System.Drawing.Size(156, 17);
            this.frequencySortCB.TabIndex = 13;
            this.frequencySortCB.TabStop = false;
            this.frequencySortCB.Text = "Frequency-only Sort (Ctrl+F)";
            this.frequencySortCB.UseVisualStyleBackColor = true;
            this.frequencySortCB.Click += new System.EventHandler(this.frequencySortCB_Click);
            // 
            // RadKey
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(535, 361);
            this.Controls.Add(this.frequencySortCB);
            this.Controls.Add(this.includeLowFreqCB);
            this.Controls.Add(this.compoundsResults);
            this.Controls.Add(this.ambiguousLabel);
            this.Controls.Add(this.compoundEntry);
            this.Controls.Add(this.meaningBox);
            this.Controls.Add(this.readingBox);
            this.Controls.Add(this.strokeBox);
            this.Controls.Add(this.strokeLabel);
            this.Controls.Add(this.messageBox);
            this.Controls.Add(this.resultList);
            this.Controls.Add(this.selectedKanjiBox);
            this.Controls.Add(this.selectedKanjiLabel);
            this.Controls.Add(this.radLabel);
            this.Controls.Add(this.entryField);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.Name = "RadKey";
            this.Text = "RadKey";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RadKey_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RadKey_KeyDown);
            this.Resize += new System.EventHandler(this.RadKey_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox entryField;
        private System.Windows.Forms.Label radLabel;
        private System.Windows.Forms.Label selectedKanjiLabel;
        private System.Windows.Forms.TextBox selectedKanjiBox;
        private System.Windows.Forms.ListBox resultList;
        private System.Windows.Forms.TextBox messageBox;
        private System.Windows.Forms.Label strokeLabel;
        private System.Windows.Forms.TextBox strokeBox;
        private System.Windows.Forms.TextBox readingBox;
        private System.Windows.Forms.TextBox meaningBox;
        //private System.Windows.Forms.NotifyIcon RadKeyNotifyIcon;
        private System.Windows.Forms.TextBox compoundEntry;
        private System.Windows.Forms.Label ambiguousLabel;
        private System.Windows.Forms.ListBox compoundsResults;
        private System.Windows.Forms.CheckBox includeLowFreqCB;
        private System.Windows.Forms.CheckBox frequencySortCB;
    }
}


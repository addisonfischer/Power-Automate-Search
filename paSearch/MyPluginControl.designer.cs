using System.Windows.Forms;

namespace paSearch
{
    partial class MyPluginControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.tssSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.searchTextBox = new System.Windows.Forms.TextBox();
            this.searchButton = new System.Windows.Forms.Button();
            this.resultTextBox = new System.Windows.Forms.ListView();
            this.Name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SolutionName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SolutionID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.categoryTipLabel = new System.Windows.Forms.Label();
            this.fuzzySearchCheckBox = new System.Windows.Forms.CheckBox();
            this.searchContentCheckBox = new System.Windows.Forms.CheckBox();
            this.toolStripMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripMenu
            // 
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tssSeparator1});
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Size = new System.Drawing.Size(1199, 25);
            this.toolStripMenu.TabIndex = 4;
            this.toolStripMenu.Text = "toolStrip1";
            // 
            // tssSeparator1
            // 
            this.tssSeparator1.Name = "tssSeparator1";
            this.tssSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // searchTextBox
            // 
            this.searchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.searchTextBox.Location = new System.Drawing.Point(4, 29);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(679, 20);
            this.searchTextBox.TabIndex = 5;
            this.searchTextBox.TextChanged += new System.EventHandler(this.searchTextBox_TextChanged);
            this.searchTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.searchTextBox_KeyDown);
            // 
            // searchButton
            // 
            this.searchButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchButton.Location = new System.Drawing.Point(689, 28);
            this.searchButton.Name = "searchButton";
            this.searchButton.Size = new System.Drawing.Size(90, 23);
            this.searchButton.TabIndex = 6;
            this.searchButton.Text = "Search";
            this.searchButton.UseVisualStyleBackColor = true;
            this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
            // 
            // resultTextBox
            // 
            this.resultTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.resultTextBox.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Name,
            this.SolutionName,
            this.SolutionID});
            this.resultTextBox.FullRowSelect = true;
            this.resultTextBox.GridLines = true;
            this.resultTextBox.HideSelection = false;
            this.resultTextBox.Location = new System.Drawing.Point(0, 60);
            this.resultTextBox.MultiSelect = false;
            this.resultTextBox.Name = "resultTextBox";
            this.resultTextBox.Size = new System.Drawing.Size(1199, 492);
            this.resultTextBox.TabIndex = 7;
            this.resultTextBox.UseCompatibleStateImageBehavior = false;
            this.resultTextBox.View = System.Windows.Forms.View.Details;
            this.resultTextBox.DoubleClick += new System.EventHandler(this.resultTextBox_DoubleClick);
            // 
            // Name
            // 
            this.Name.Text = "Name";
            this.Name.Width = 400;
            // 
            // SolutionName
            // 
            this.SolutionName.Text = "Solution Name";
            this.SolutionName.Width = 350;
            // 
            // SolutionID
            // 
            this.SolutionID.Text = "Solution ID";
            this.SolutionID.Width = 300;
            // 
            // comboBox1
            // 
            this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "All",
            "Workflow",
            "Dialog",
            "Business Rule",
            "Action",
            "Business Process",
            "Modern/Cloud",
            "Desktop",
            "AI",
            "Web Resource"});
            this.comboBox1.Location = new System.Drawing.Point(785, 29);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(160, 21);
            this.comboBox1.TabIndex = 8;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // categoryTipLabel
            // 
            this.categoryTipLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.categoryTipLabel.AutoSize = true;
            this.categoryTipLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.categoryTipLabel.ForeColor = System.Drawing.Color.DarkOrange;
            this.categoryTipLabel.Location = new System.Drawing.Point(952, 33);
            this.categoryTipLabel.Name = "categoryTipLabel";
            this.categoryTipLabel.Size = new System.Drawing.Size(240, 13);
            this.categoryTipLabel.TabIndex = 9;
            this.categoryTipLabel.Text = "💡 Tip: Select a category for faster searches";
            this.categoryTipLabel.Visible = false;
            // 
            // fuzzySearchCheckBox
            // 
            this.fuzzySearchCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fuzzySearchCheckBox.AutoSize = true;
            this.fuzzySearchCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fuzzySearchCheckBox.Location = new System.Drawing.Point(900, 10);
            this.fuzzySearchCheckBox.Name = "fuzzySearchCheckBox";
            this.fuzzySearchCheckBox.Size = new System.Drawing.Size(100, 17);
            this.fuzzySearchCheckBox.TabIndex = 10;
            this.fuzzySearchCheckBox.Text = "Fuzzy Search";
            this.fuzzySearchCheckBox.UseVisualStyleBackColor = true;
            this.fuzzySearchCheckBox.CheckedChanged += new System.EventHandler(this.fuzzySearchCheckBox_CheckedChanged);
            // 
            // searchContentCheckBox
            // 
            this.searchContentCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchContentCheckBox.AutoSize = true;
            this.searchContentCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.searchContentCheckBox.Location = new System.Drawing.Point(1020, 10);
            this.searchContentCheckBox.Name = "searchContentCheckBox";
            this.searchContentCheckBox.Size = new System.Drawing.Size(110, 17);
            this.searchContentCheckBox.TabIndex = 11;
            this.searchContentCheckBox.Text = "Search Content";
            this.searchContentCheckBox.UseVisualStyleBackColor = true;
            this.searchContentCheckBox.CheckedChanged += new System.EventHandler(this.searchContentCheckBox_CheckedChanged);
            // 
            // MyPluginControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.searchContentCheckBox);
            this.Controls.Add(this.fuzzySearchCheckBox);
            this.Controls.Add(this.categoryTipLabel);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.resultTextBox);
            this.Controls.Add(this.searchButton);
            this.Controls.Add(this.searchTextBox);
            this.Controls.Add(this.toolStripMenu);
            this.Size = new System.Drawing.Size(1199, 552);
            this.Load += new System.EventHandler(this.MyPluginControl_Load);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripSeparator tssSeparator1;
        private System.Windows.Forms.TextBox searchTextBox;
        private System.Windows.Forms.Button searchButton;
        private System.Windows.Forms.ListView resultTextBox;
        private System.Windows.Forms.ColumnHeader Name;
        private System.Windows.Forms.ColumnHeader SolutionName;
        private System.Windows.Forms.ColumnHeader SolutionID;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label categoryTipLabel;
        private System.Windows.Forms.CheckBox fuzzySearchCheckBox;
        private System.Windows.Forms.CheckBox searchContentCheckBox;
    }
}

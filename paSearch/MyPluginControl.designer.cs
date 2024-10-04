namespace paSearch
{
	partial class MyPluginControl
	{
		/// <summary> 
		/// Variable nécessaire au concepteur.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Nettoyage des ressources utilisées.
		/// </summary>
		/// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Code généré par le Concepteur de composants

		/// <summary> 
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
		/// le contenu de cette méthode avec l'éditeur de code.
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
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.SolutionID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
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
			this.searchTextBox.Location = new System.Drawing.Point(4, 29);
			this.searchTextBox.Name = "searchTextBox";
			this.searchTextBox.Size = new System.Drawing.Size(279, 20);
			this.searchTextBox.TabIndex = 5;
			this.searchTextBox.TextChanged += new System.EventHandler(this.searchTextBox_TextChanged);
			// 
			// searchButton
			// 
			this.searchButton.Location = new System.Drawing.Point(307, 29);
			this.searchButton.Name = "searchButton";
			this.searchButton.Size = new System.Drawing.Size(75, 23);
			this.searchButton.TabIndex = 6;
			this.searchButton.Text = "Search";
			this.searchButton.UseVisualStyleBackColor = true;
			this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
			// 
			// resultTextBox
			// 
			this.resultTextBox.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
	new System.Windows.Forms.ColumnHeader() { Text = "Name", Width = 500 },
	new System.Windows.Forms.ColumnHeader() { Text = "SolutionName", Width = 450 },
	new System.Windows.Forms.ColumnHeader() { Text = "SolutionId", Width = 250 }
});
			this.resultTextBox.HideSelection = false;
			this.resultTextBox.Location = new System.Drawing.Point(4, 56);
			this.resultTextBox.MultiSelect = false;
			this.resultTextBox.Name = "resultTextBox";
			this.resultTextBox.Size = new System.Drawing.Size(1200, 1000);
			this.resultTextBox.TabIndex = 7;
			this.resultTextBox.UseCompatibleStateImageBehavior = false;
			this.resultTextBox.DoubleClick += new System.EventHandler(this.resultTextBox_DoubleClick);
			this.resultTextBox.View = System.Windows.Forms.View.Details;
			// 
			// Name
			// 
			this.Name.DisplayIndex = 0;
			this.Name.Width = 250;
			// 
			// SolutionName
			// 
			this.SolutionName.DisplayIndex = 1;
			this.SolutionName.Width = 250;
			// 
			// comboBox1
			// 
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
            "AI"});
			this.comboBox1.Location = new System.Drawing.Point(388, 30);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(121, 21);
			this.comboBox1.TabIndex = 8;
			this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
			// 
			// SolutionID
			// 
			this.SolutionID.Width = 250;
			// 
			// MyPluginControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.comboBox1);
			this.Controls.Add(this.resultTextBox);
			this.Controls.Add(this.searchButton);
			this.Controls.Add(this.searchTextBox);
			this.Controls.Add(this.toolStripMenu);
			//this.Name = "MyPluginControl";
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
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.ColumnHeader SolutionID;
	}
}

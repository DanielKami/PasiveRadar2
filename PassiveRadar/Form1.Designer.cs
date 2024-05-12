namespace PasiveRadar
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.panel2 = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.button5 = new System.Windows.Forms.Button();
            this.imageList2 = new System.Windows.Forms.ImageList(this.components);
            this.displayControl1 = new PasiveRadar.DisplayControl();
            this.button7 = new System.Windows.Forms.Button();
            this.radarControl1 = new PasiveRadar.RadarControl();
            this.button9 = new System.Windows.Forms.Button();
            this.translateControl1 = new PasiveRadar.TranslateControl();
            this.label8 = new System.Windows.Forms.Label();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panelViewport1 = new System.Windows.Forms.Panel();
            this.panelViewport3 = new System.Windows.Forms.Panel();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.panelViewport2 = new System.Windows.Forms.Panel();
            this.panelViewport4 = new System.Windows.Forms.Panel();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.label51 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button3 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonSetSettings2 = new System.Windows.Forms.Button();
            this.buttonSettings2 = new System.Windows.Forms.Button();
            this.buttonSetSettings1 = new System.Windows.Forms.Button();
            this.buttonSettings1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.imageList3 = new System.Windows.Forms.ImageList(this.components);
            this.panel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.White;
            this.imageList1.Images.SetKeyName(0, "play.png");
            this.imageList1.Images.SetKeyName(1, "stop.png");
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.flowLayoutPanel1);
            this.panel2.Controls.Add(this.label8);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(190, 558);
            this.panel2.TabIndex = 34;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.Controls.Add(this.button5);
            this.flowLayoutPanel1.Controls.Add(this.displayControl1);
            this.flowLayoutPanel1.Controls.Add(this.button7);
            this.flowLayoutPanel1.Controls.Add(this.radarControl1);
            this.flowLayoutPanel1.Controls.Add(this.button9);
            this.flowLayoutPanel1.Controls.Add(this.translateControl1);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(190, 558);
            this.flowLayoutPanel1.TabIndex = 56;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // button5
            // 
            this.button5.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.button5.FlatAppearance.BorderSize = 0;
            this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button5.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.button5.ImageIndex = 0;
            this.button5.ImageList = this.imageList2;
            this.button5.Location = new System.Drawing.Point(0, 0);
            this.button5.Margin = new System.Windows.Forms.Padding(0);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(225, 30);
            this.button5.TabIndex = 56;
            this.button5.Text = "Display";
            this.button5.UseVisualStyleBackColor = false;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // imageList2
            // 
            this.imageList2.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList2.ImageStream")));
            this.imageList2.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList2.Images.SetKeyName(0, "ButtonOff.png");
            this.imageList2.Images.SetKeyName(1, "ButtonOn.png");
            // 
            // displayControl1
            // 
            this.displayControl1.Location = new System.Drawing.Point(0, 30);
            this.displayControl1.Margin = new System.Windows.Forms.Padding(0);
            this.displayControl1.Name = "displayControl1";
            this.displayControl1.Size = new System.Drawing.Size(223, 324);
            this.displayControl1.TabIndex = 57;
            // 
            // button7
            // 
            this.button7.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.button7.FlatAppearance.BorderSize = 0;
            this.button7.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button7.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.button7.ImageIndex = 0;
            this.button7.ImageList = this.imageList2;
            this.button7.Location = new System.Drawing.Point(0, 354);
            this.button7.Margin = new System.Windows.Forms.Padding(0);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(225, 30);
            this.button7.TabIndex = 59;
            this.button7.Text = "Radar";
            this.button7.UseVisualStyleBackColor = false;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // radarControl1
            // 
            this.radarControl1.Location = new System.Drawing.Point(5, 389);
            this.radarControl1.Margin = new System.Windows.Forms.Padding(5);
            this.radarControl1.Name = "radarControl1";
            this.radarControl1.Size = new System.Drawing.Size(254, 1032);
            this.radarControl1.TabIndex = 60;
            this.radarControl1.Load += new System.EventHandler(this.radarControl1_Load);
            // 
            // button9
            // 
            this.button9.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.button9.FlatAppearance.BorderSize = 0;
            this.button9.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button9.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.button9.ImageIndex = 0;
            this.button9.ImageList = this.imageList2;
            this.button9.Location = new System.Drawing.Point(0, 1426);
            this.button9.Margin = new System.Windows.Forms.Padding(0);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(225, 30);
            this.button9.TabIndex = 61;
            this.button9.Text = "Translate";
            this.button9.UseVisualStyleBackColor = false;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // translateControl1
            // 
            this.translateControl1.Location = new System.Drawing.Point(5, 1461);
            this.translateControl1.Margin = new System.Windows.Forms.Padding(5);
            this.translateControl1.Name = "translateControl1";
            this.translateControl1.Size = new System.Drawing.Size(223, 362);
            this.translateControl1.TabIndex = 62;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(152, 683);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(0, 16);
            this.label8.TabIndex = 48;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.splitContainer1);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer3.Size = new System.Drawing.Size(1160, 558);
            this.splitContainer3.SplitterDistance = 561;
            this.splitContainer3.SplitterWidth = 5;
            this.splitContainer3.TabIndex = 0;
            this.splitContainer3.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer_SplitterMoved);
            this.splitContainer3.MouseUp += new System.Windows.Forms.MouseEventHandler(this.splitContainer3_MouseUp);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panelViewport1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panelViewport3);
            this.splitContainer1.Size = new System.Drawing.Size(561, 558);
            this.splitContainer1.SplitterDistance = 279;
            this.splitContainer1.TabIndex = 1;
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
            this.splitContainer1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.splitContainer1_MouseUp);
            // 
            // panelViewport1
            // 
            this.panelViewport1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.panelViewport1.Cursor = System.Windows.Forms.Cursors.Cross;
            this.panelViewport1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelViewport1.Location = new System.Drawing.Point(0, 0);
            this.panelViewport1.Name = "panelViewport1";
            this.panelViewport1.Size = new System.Drawing.Size(561, 279);
            this.panelViewport1.TabIndex = 1;
            this.panelViewport1.SizeChanged += new System.EventHandler(this.panelViewport1_SizeChanged);
            this.panelViewport1.ParentChanged += new System.EventHandler(this.panelViewport1_ParentChanged);
            // 
            // panelViewport3
            // 
            this.panelViewport3.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.panelViewport3.Cursor = System.Windows.Forms.Cursors.Cross;
            this.panelViewport3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelViewport3.Location = new System.Drawing.Point(0, 0);
            this.panelViewport3.Name = "panelViewport3";
            this.panelViewport3.Size = new System.Drawing.Size(561, 275);
            this.panelViewport3.TabIndex = 2;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.panelViewport2);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.panelViewport4);
            this.splitContainer2.Size = new System.Drawing.Size(594, 558);
            this.splitContainer2.SplitterDistance = 279;
            this.splitContainer2.TabIndex = 29;
            this.splitContainer2.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer2_SplitterMoved);
            this.splitContainer2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.splitContainer2_MouseUp);
            // 
            // panelViewport2
            // 
            this.panelViewport2.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.panelViewport2.Cursor = System.Windows.Forms.Cursors.Cross;
            this.panelViewport2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelViewport2.Location = new System.Drawing.Point(0, 0);
            this.panelViewport2.Margin = new System.Windows.Forms.Padding(4);
            this.panelViewport2.Name = "panelViewport2";
            this.panelViewport2.Size = new System.Drawing.Size(594, 279);
            this.panelViewport2.TabIndex = 29;
            // 
            // panelViewport4
            // 
            this.panelViewport4.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.panelViewport4.Cursor = System.Windows.Forms.Cursors.Cross;
            this.panelViewport4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelViewport4.Location = new System.Drawing.Point(0, 0);
            this.panelViewport4.Margin = new System.Windows.Forms.Padding(4);
            this.panelViewport4.Name = "panelViewport4";
            this.panelViewport4.Size = new System.Drawing.Size(594, 275);
            this.panelViewport4.TabIndex = 29;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer4.IsSplitterFixed = true;
            this.splitContainer4.Location = new System.Drawing.Point(0, 71);
            this.splitContainer4.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.panel2);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer4.Size = new System.Drawing.Size(1355, 558);
            this.splitContainer4.SplitterDistance = 190;
            this.splitContainer4.SplitterWidth = 5;
            this.splitContainer4.TabIndex = 36;
            this.splitContainer4.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer_SplitterMoved);
            this.splitContainer4.MouseUp += new System.Windows.Forms.MouseEventHandler(this.splitContainer4_MouseUp);
            // 
            // label51
            // 
            this.label51.AutoSize = true;
            this.label51.Location = new System.Drawing.Point(1187, 49);
            this.label51.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(0, 16);
            this.label51.TabIndex = 88;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button3);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.buttonSetSettings2);
            this.groupBox1.Controls.Add(this.buttonSettings2);
            this.groupBox1.Controls.Add(this.buttonSetSettings1);
            this.groupBox1.Controls.Add(this.buttonSettings1);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label51);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(1355, 71);
            this.groupBox1.TabIndex = 33;
            this.groupBox1.TabStop = false;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(525, 22);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(144, 28);
            this.button3.TabIndex = 104;
            this.button3.Text = "Map";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(321, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(72, 16);
            this.label3.TabIndex = 103;
            this.label3.Text = "Nr recivers";
            // 
            // buttonSetSettings2
            // 
            this.buttonSetSettings2.Location = new System.Drawing.Point(989, 44);
            this.buttonSetSettings2.Name = "buttonSetSettings2";
            this.buttonSetSettings2.Size = new System.Drawing.Size(74, 27);
            this.buttonSetSettings2.TabIndex = 102;
            this.buttonSetSettings2.Text = "Set 2";
            this.buttonSetSettings2.UseVisualStyleBackColor = true;
            this.buttonSetSettings2.Click += new System.EventHandler(this.buttonSetSettings2_Click);
            // 
            // buttonSettings2
            // 
            this.buttonSettings2.Location = new System.Drawing.Point(989, 11);
            this.buttonSettings2.Name = "buttonSettings2";
            this.buttonSettings2.Size = new System.Drawing.Size(74, 27);
            this.buttonSettings2.TabIndex = 101;
            this.buttonSettings2.Text = "Restore 2";
            this.buttonSettings2.UseVisualStyleBackColor = true;
            this.buttonSettings2.Click += new System.EventHandler(this.buttonSettings2_Click);
            // 
            // buttonSetSettings1
            // 
            this.buttonSetSettings1.Location = new System.Drawing.Point(903, 44);
            this.buttonSetSettings1.Name = "buttonSetSettings1";
            this.buttonSetSettings1.Size = new System.Drawing.Size(80, 27);
            this.buttonSetSettings1.TabIndex = 100;
            this.buttonSetSettings1.Text = "Set 1";
            this.buttonSetSettings1.UseVisualStyleBackColor = true;
            this.buttonSetSettings1.Click += new System.EventHandler(this.buttonSetSettings1_Click);
            // 
            // buttonSettings1
            // 
            this.buttonSettings1.Location = new System.Drawing.Point(903, 12);
            this.buttonSettings1.Name = "buttonSettings1";
            this.buttonSettings1.Size = new System.Drawing.Size(80, 27);
            this.buttonSettings1.TabIndex = 99;
            this.buttonSettings1.Text = "Restore 1";
            this.buttonSettings1.UseVisualStyleBackColor = true;
            this.buttonSettings1.Click += new System.EventHandler(this.buttonSettings1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(0, 23);
            this.button2.Margin = new System.Windows.Forms.Padding(4);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(187, 28);
            this.button2.TabIndex = 96;
            this.button2.Text = "Show all";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label2.ForeColor = System.Drawing.Color.Crimson;
            this.label2.Location = new System.Drawing.Point(1298, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 20);
            this.label2.TabIndex = 95;
            this.label2.Text = "v. 1.50";
            // 
            // button1
            // 
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.ImageIndex = 0;
            this.button1.ImageList = this.imageList1;
            this.button1.Location = new System.Drawing.Point(196, 13);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(63, 52);
            this.button1.TabIndex = 27;
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.ButtonStart_Click);
            // 
            // imageList3
            // 
            this.imageList3.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList3.ImageStream")));
            this.imageList3.TransparentColor = System.Drawing.Color.White;
            this.imageList3.Images.SetKeyName(0, "locked.jpg");
            this.imageList3.Images.SetKeyName(1, "unlocked.jpg");
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1355, 629);
            this.Controls.Add(this.splitContainer4);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Pasive Radar";
            this.MaximumSizeChanged += new System.EventHandler(this.Form1_MaximumSizeChanged);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load_1);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.ResizeBegin += new System.EventHandler(this.Form1_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.Form1_ResizeEnd);
            this.ClientSizeChanged += new System.EventHandler(this.Form1_ClientSizeChanged);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.ImageList imageList2;
        private System.Windows.Forms.Button button5;
        private DisplayControl displayControl1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label51;
  
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button7;
        private RadarControl radarControl1;
        private System.Windows.Forms.ImageList imageList3;
        private System.Windows.Forms.Button button9;
        private TranslateControl translateControl1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button buttonSetSettings2;
        private System.Windows.Forms.Button buttonSettings2;
        private System.Windows.Forms.Button buttonSetSettings1;
        private System.Windows.Forms.Button buttonSettings1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panelViewport1;
        private System.Windows.Forms.Panel panelViewport3;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Panel panelViewport2;
        private System.Windows.Forms.Panel panelViewport4;
        private System.Windows.Forms.Button button3;
    }
}


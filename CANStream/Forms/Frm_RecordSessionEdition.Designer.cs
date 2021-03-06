﻿/*
 *	This file is part of CANStream.
 *
 *	CANStream program is free software: you can redistribute it and/or modify
 *	it under the terms of the GNU General Public License as published by
 *	the Free Software Foundation, either version 3 of the License, or
 *	(at your option) any later version.
 *
 *	This program is distributed in the hope that it will be useful,
 *	but WITHOUT ANY WARRANTY; without even the implied warranty of
 *	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *	GNU General Public License for more details.
 *
 *	You should have received a copy of the GNU General Public License
 *	along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 *	CANStream Copyright © 2013-2016 whilenotinfinite@gmail.com
 */

namespace CANStream
{
	partial class Frm_RecordSessionEdition
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
            this.TSB_Main = new System.Windows.Forms.ToolStrip();
            this.TSB_Apply = new System.Windows.Forms.ToolStripButton();
            this.TSB_Cancel = new System.Windows.Forms.ToolStripButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.DP_SessionDate = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.rTxt_SessionComment = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Txt_SessionName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.Ctrl_RecordUserInfo = new CANStream.Ctrl_RecordUserInformations();
            this.TSB_Main.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // TSB_Main
            // 
            this.TSB_Main.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.TSB_Main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TSB_Apply,
            this.TSB_Cancel});
            this.TSB_Main.Location = new System.Drawing.Point(0, 0);
            this.TSB_Main.Name = "TSB_Main";
            this.TSB_Main.Size = new System.Drawing.Size(673, 27);
            this.TSB_Main.TabIndex = 1;
            this.TSB_Main.Text = "toolStrip1";
            // 
            // TSB_Apply
            // 
            this.TSB_Apply.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.TSB_Apply.Image = global::CANStream.Icones.Apply_16;
            this.TSB_Apply.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.TSB_Apply.Name = "TSB_Apply";
            this.TSB_Apply.Size = new System.Drawing.Size(24, 24);
            this.TSB_Apply.Text = "toolStripButton1";
            this.TSB_Apply.ToolTipText = "Apply";
            this.TSB_Apply.Click += new System.EventHandler(this.TSB_ApplyClick);
            // 
            // TSB_Cancel
            // 
            this.TSB_Cancel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.TSB_Cancel.Image = global::CANStream.Icones.Cancel_16;
            this.TSB_Cancel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.TSB_Cancel.Name = "TSB_Cancel";
            this.TSB_Cancel.Size = new System.Drawing.Size(24, 24);
            this.TSB_Cancel.Text = "toolStripButton2";
            this.TSB_Cancel.ToolTipText = "Cancel";
            this.TSB_Cancel.Click += new System.EventHandler(this.TSB_CancelClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.DP_SessionDate);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.rTxt_SessionComment);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.Txt_SessionName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(16, 34);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(641, 178);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Session details";
            // 
            // DP_SessionDate
            // 
            this.DP_SessionDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.DP_SessionDate.Location = new System.Drawing.Point(79, 60);
            this.DP_SessionDate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.DP_SessionDate.Name = "DP_SessionDate";
            this.DP_SessionDate.Size = new System.Drawing.Size(125, 22);
            this.DP_SessionDate.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(8, 68);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "Date";
            // 
            // rTxt_SessionComment
            // 
            this.rTxt_SessionComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rTxt_SessionComment.Location = new System.Drawing.Point(79, 98);
            this.rTxt_SessionComment.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rTxt_SessionComment.Name = "rTxt_SessionComment";
            this.rTxt_SessionComment.Size = new System.Drawing.Size(553, 69);
            this.rTxt_SessionComment.TabIndex = 3;
            this.rTxt_SessionComment.Text = "";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(8, 98);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Comment";
            // 
            // Txt_SessionName
            // 
            this.Txt_SessionName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Txt_SessionName.Location = new System.Drawing.Point(79, 28);
            this.Txt_SessionName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Txt_SessionName.Name = "Txt_SessionName";
            this.Txt_SessionName.Size = new System.Drawing.Size(553, 22);
            this.Txt_SessionName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 32);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.Ctrl_RecordUserInfo);
            this.groupBox2.Location = new System.Drawing.Point(16, 220);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Size = new System.Drawing.Size(641, 300);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Session user informations";
            // 
            // Ctrl_RecordUserInfo
            // 
            this.Ctrl_RecordUserInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Ctrl_RecordUserInfo.Location = new System.Drawing.Point(9, 24);
            this.Ctrl_RecordUserInfo.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.Ctrl_RecordUserInfo.Name = "Ctrl_RecordUserInfo";
            this.Ctrl_RecordUserInfo.Size = new System.Drawing.Size(632, 269);
            this.Ctrl_RecordUserInfo.TabIndex = 0;
            // 
            // Frm_RecordSessionEdition
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(673, 527);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.TSB_Main);
            this.Icon = global::CANStream.Icones.CANStream_Icone;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Frm_RecordSessionEdition";
            this.Text = "Record Session Edition";
            this.TSB_Main.ResumeLayout(false);
            this.TSB_Main.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		private CANStream.Ctrl_RecordUserInformations Ctrl_RecordUserInfo;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox Txt_SessionName;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.RichTextBox rTxt_SessionComment;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.DateTimePicker DP_SessionDate;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.ToolStripButton TSB_Cancel;
		private System.Windows.Forms.ToolStripButton TSB_Apply;
		private System.Windows.Forms.ToolStrip TSB_Main;
	}
}

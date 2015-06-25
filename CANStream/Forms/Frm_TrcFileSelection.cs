﻿/*
 * Created by SharpDevelop.
 * User: vbrault
 * Date: 3/7/2014
 * Time: 16:12 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CANStream
{
	/// <summary>
	/// Description of Frm_TrcFileSelection.
	/// </summary>
	public partial class Frm_TrcFileSelection : Form
	{
		#region Private members
		
		private MainForm FrmMain;
		private string RecordsFolder;
		
		private PcanTrcFileInfo[] TrcFilesInfos;
				
		#endregion
		
		public Frm_TrcFileSelection(string TrcFolder, MainForm FrmParent)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			FrmMain = FrmParent;
			RecordsFolder = TrcFolder;
			
			TrcFilesInfos = null;
			
			ShowTrcFiles();
		}
		
		#region Control events
		
		#region Context_LV_TrcFiles
		
		private void SelectAllToolStripMenuItemClick(object sender, EventArgs e)
		{
			foreach(ListViewItem It in LV_TrcFiles.Items)
			{
				It.Checked=true;
			}
		}
		
		private void UnselectAllToolStripMenuItemClick(object sender, EventArgs e)
		{
			foreach(ListViewItem It in LV_TrcFiles.Items)
			{
				It.Checked=false;
			}	
		}
		
		private void InvertSelectionToolStripMenuItemClick(object sender, EventArgs e)
		{
			foreach(ListViewItem It in LV_TrcFiles.Items)
			{
				It.Checked=!It.Checked;
			}
		}
		
		#endregion
		
		private void Cmd_CancelClick(object sender, EventArgs e)
		{
			this.Close();
		}
		
		private void Cmd_ConvertClick(object sender, EventArgs e)
		{
			List<PcanTrcFileInfo> ConversionList = new List<PcanTrcFileInfo>();
			
			for (int i=0; i<LV_TrcFiles.Items.Count; i++)
			{
				if (LV_TrcFiles.Items[i].Checked)
				{
					ConversionList.Add(TrcFilesInfos[(int)LV_TrcFiles.Items[i].Tag]);
				}
			}
			
			if (ConversionList.Count>0)
			{
				FrmMain.SetRecordConversionList(ConversionList.ToArray(),this);
			}
			else
			{
				MessageBox.Show("No file selected !",Application.ProductName,MessageBoxButtons.OK,MessageBoxIcon.Information);
			}
		}
		
		#endregion
		
		#region Private methods
		
		private void ShowTrcFiles()
		{
			LV_TrcFiles.Items.Clear();
			
			if (Directory.Exists(RecordsFolder))
			{
				DirectoryInfo RecDirInfo = new DirectoryInfo(RecordsFolder);
				
				TrcFilesInfos = CANStreamTools.GetTrcFileInfoList(RecDirInfo.FullName);
				
				if (TrcFilesInfos.Length > 0)
				{
					//foreach(PcanTrcFileInfo TrcInfo in TrcFilesInfos)
					for (int iTrc = 0; iTrc < TrcFilesInfos.Length; iTrc++)
					{
						PcanTrcFileInfo TrcInfo = TrcFilesInfos[iTrc];
						
						ListViewItem TrcIt = LV_TrcFiles.Items.Add(TrcInfo.TrcFileInfo.Name);
						TrcIt.Tag = iTrc;
						
						if (!(TrcInfo.TrcFileEvent == null || TrcInfo.TrcFileSession == null))
						{
							ListViewGroup oEventSessionGroup = Get_EventSessionGroup(TrcInfo.TrcFileEvent.Name, TrcInfo.TrcFileSession.Name);
							
							if (oEventSessionGroup == null)
							{
								oEventSessionGroup = LV_TrcFiles.Groups.Add(LV_TrcFiles.Groups.Count.ToString(), TrcInfo.TrcFileEvent.Name + " :: " + TrcInfo.TrcFileSession.Name);
							}
							
							TrcIt.Group = oEventSessionGroup;
						}
						
						TrcIt.SubItems.Add(TrcInfo.TrcFileInfo.LastWriteTime.ToShortDateString() + " " + TrcInfo.TrcFileInfo.LastWriteTime.ToShortTimeString());
						TrcIt.SubItems.Add(CANStreamTools.GetScaledFileSize(TrcInfo.TrcFileInfo.Length));
					}
					
					LV_TrcFiles.Sort();
				}
			}
			else
			{
				MessageBox.Show("Records folder doesn't exist or is not available !",Application.ProductName,MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
			}
		}
		
		private ListViewGroup Get_EventSessionGroup(string EventName, string SessionName)
		{
			foreach (ListViewGroup oGrp in LV_TrcFiles.Groups)
			{
				if (oGrp.Header.Equals(EventName + " :: " + SessionName))
				{
					return(oGrp);
				}
			}
			
			return(null);
		}
		
		#endregion
	}
}
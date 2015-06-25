﻿/*
 * Created by SharpDevelop.
 * User: VBrault
 * Date: 8/15/2014
 * Time: 15:14 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CANStream
{
	public partial class Frm_RecordSessionEdition : Form
	{
		#region Private members
		
		private CS_RecordSession oRecordSession;
		private CS_RecordEvent oEventParent;
		private Form FrmParent;
		private bool bNewSession;
		
		#endregion
		
		public Frm_RecordSessionEdition(CS_RecordEvent ParentEvt, CS_RecordSession RecSession, Form Caller, bool bNew)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			oRecordSession = RecSession;
			oEventParent = ParentEvt;
			FrmParent = Caller;
			bNewSession = bNew;
			Show_SessionProperties();
		}
		
		#region Control events
		
		#region TSB_Main
		
		private void TSB_ApplyClick(object sender, EventArgs e)
		{
			if (Set_SessionProperties())
			{
				if (bNewSession) oEventParent.Sessions.Add(oRecordSession);
				oEventParent.CurrentSession = oRecordSession;
				
				if (FrmParent.GetType().Equals(typeof(MainForm)))
				{
					Update_RecordEventsFile();
					((MainForm)FrmParent).UpDate_RecordEventSession();
				}
				else if (FrmParent.GetType().Equals(typeof(Frm_RecordEventEdition)))
				{
					((Frm_RecordEventEdition)FrmParent).Update_SessionList();
				}
				
				this.Close();
			}
		}
		
		private void TSB_CancelClick(object sender, EventArgs e)
		{
			this.Close();
		}
		
		#endregion
		
		#endregion
		
		#region Private methodes
		
		private void Show_SessionProperties()
		{
			//General properties
			Txt_SessionName.Text = oRecordSession.Name;
			DP_SessionDate.Value = oRecordSession.SessionDate;
			rTxt_SessionComment.Text = oRecordSession.Comment;
			
			//User info
			Ctrl_RecordUserInfo.Set_UserInformations(oRecordSession.UserInfos);
		}
		
		private bool Set_SessionProperties()
		{
			if (!(Txt_SessionName.Text.Equals("")))
			{
				if ((!(oEventParent.RecordSessionExits(Txt_SessionName.Text))) || (!bNewSession))
				{
					oRecordSession.Name = Txt_SessionName.Text;
				}
				else
				{
					MessageBox.Show("A record session already exists in the record event with the same name !",Application.ProductName,MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
					return(false);
				}
			}
			else
			{
				MessageBox.Show("Name of the record session cannot be empty !",Application.ProductName,MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
				return(false);
			}
			
			oRecordSession.SessionDate = DP_SessionDate.Value;
			oRecordSession.Comment = rTxt_SessionComment.Text;
			
			oRecordSession.UserInfos = Ctrl_RecordUserInfo.Get_UserInformations();
			
			return(true);
		}
		
		private void Update_RecordEventsFile()
		{
			CS_RecordEventsCollection oAppEvents = new CS_RecordEventsCollection();
			string AppEventsFile = CANStreamTools.CsDataPath + "\\RecordEvents.xml";
			
			if (File.Exists(AppEventsFile))
			{
				if (oAppEvents.Read_EventsCollectionFile(AppEventsFile))
				{
					int CurrentEventIndex = oAppEvents.Get_RecordEventIndex(oEventParent.Name, oEventParent.StartingDate);
					
					if (!(CurrentEventIndex == -1))
					{
						oAppEvents.Events[CurrentEventIndex] = oEventParent;
						oAppEvents.Write_EventsCollectionFile(AppEventsFile);
					}
				}
			}
		}
		
		#endregion
	}
}
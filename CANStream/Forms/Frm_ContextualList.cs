﻿/*
 * Created by SharpDevelop.
 * User: VBrault
 * Date: 2/6/2014
 * Time: 13:34 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CANStream
{
	/// <summary>
	/// Description of Frm_ContextualList.
	/// </summary>
	public partial class Frm_ContextualList : Form
	{
		#region Private members
		
		private Form FormParent;
		private ExpressionElementType TypeItem;
		
		#endregion
		
		public Frm_ContextualList(Form FrmParent, object Items, ExpressionElementType ItemType)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			FormParent = FrmParent;
			TypeItem = ItemType;
			
			LV_Items.Items.Clear();
			
			if (!(Items == null))
			{
				if (Items.GetType().Equals(typeof(string[])))
				{
					string[] Elements = (string[]) Items;
					int iIcon = (int) TypeItem;
					
					foreach (string ElemName in Elements)
					{
						LV_Items.Items.Add(ElemName, iIcon);
					}
				}
				else
				{
					ExpressionElementItem[] Elements = (ExpressionElementItem[]) Items;
					
					int iIcone = -1;
					
					foreach(ExpressionElementItem sElem in Elements)
					{
						switch (sElem.Source)
						{
							case ExpressionElementChannelSource.CAN_Paramater:
								iIcone = 3;
								break;
							case ExpressionElementChannelSource.VirtualChannel:
								iIcone = 4;
								break;
							case ExpressionElementChannelSource.BuiltInSignal:
								iIcone = 5;
								break;
							case ExpressionElementChannelSource.DataChannel:
								iIcone = 0;
								break;
						}
						
						LV_Items.Items.Add(sElem.Name, iIcone);
					}
					
				}
			}
		}
		
		#region Control events
		
		private void LV_ItemsMouseLeave(object sender, EventArgs e)
		{
			this.Close();
		}
		
		private void LV_ItemsKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode.Equals(Keys.Escape))
			{
				this.Close();
			}
			
			if (e.KeyCode.Equals(Keys.Enter))
			{
				SendSelectionToParentForm();
			}
		}
		
		private void LV_ItemsDoubleClick(object sender, EventArgs e)
		{
			SendSelectionToParentForm();
		}
		
		#endregion
		
		#region Private methods
		
		private void SendSelectionToParentForm()
		{
			if (!(LV_Items.SelectedItems == null))
			{
				if (!(FormParent == null))
				{
					if (FormParent.GetType().Equals(typeof(Frm_VirtualChannel)))
					{
						string sItem = LV_Items.SelectedItems[0].Text;
						
						if (TypeItem.Equals(ExpressionElementType.Function))
						{
							sItem = sItem + "( )";
						}
						
						Frm_VirtualChannel Frm = (Frm_VirtualChannel) FormParent;
						Frm.AddElementFromContextualList(sItem);
						this.Close();
					}
				}
			}
		}
		
		#endregion
	}
}
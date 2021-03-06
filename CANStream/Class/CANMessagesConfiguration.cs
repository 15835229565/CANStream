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

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using NumberBaseConversion;

//PCANBasic includes
using Peak.Can.Basic;
using TPCANHandle = System.Byte;

namespace CANStream
{
	#region CAN Config
	
	#region Enums declarations
	
	/// <summary>
	/// Rx/Tx Message type
	/// </summary>
	public enum CanMsgRxTx
	{
		Rx=0,
		Tx=1,
	}
	
    /// <summary>
    /// CAN parameter value endianess enumeration
    /// </summary>
	public enum CanParameterEndianess
	{
		Unknown=0,
		MSBFirst=1,
		LSBFirst=2,
	}

	/// <summary>
	/// Option value for CAN parameter retrieving function
	/// </summary>
	public enum ParameterResearchOption
	{
		Name=0,
		StartBit=1,
	}
	
	/// <summary>
	/// Option value for CAN message retrieving function
	/// </summary>
	public enum MessageResearchOption
	{
		Name=0,
		Identifier=1,
	}
	
	#endregion
	
	#region Public structures declaration
	
	/// <summary>
	/// Virtual CAN parameter structure
	/// </summary>
	[Serializable]
	public struct VirtualParameter
	{
		public string LibraryName;
		public string ChannelName;
	}

    #endregion

    /// <summary>
    /// Description of a CAN Parameter part of a CAN Message
    /// </summary>
    [Serializable]
	public class CANParameter
	{
		#region Public members
		
		public string Name;
		public string Comment;
		public string Unit;
		public int StartBit;
		public int Length;
		public CanParameterEndianess Endianess;
		public bool Signed;
		public double Gain;
		public double Zero;
		
		public bool IsMultiplexed;
        public long MultiplexerValue;
        
        public double DecodedValue;
        public string RawValue;
		
        public bool IsVirtual;
        public VirtualParameter VirtualChannelReference;

        public SignalFormatProperties ValueFormat;
        public SignalAlarmsProperties Alarms;

        #endregion

        public CANParameter()
		{
			Name = "";
			Comment = "";
			Unit = "";
			StartBit=0;
			Length=0;
			Endianess=CanParameterEndianess.MSBFirst;
			Signed = false;
			Gain=1;
			Zero=0;
            IsMultiplexed = false;
            MultiplexerValue = -1;
            DecodedValue = 0;
            RawValue = "0x0";
            IsVirtual = false;
            VirtualChannelReference = new VirtualParameter();
            ValueFormat = new SignalFormatProperties();
            Alarms = new SignalAlarmsProperties();
        }
		
		#region Public methodes
		
		/// <summary>
		/// Return a clone of the current object
		/// </summary>
		/// <returns>clone of the current object</returns>
		public CANParameter Clone()
		{
			CANParameter oClone = new CANParameter();
			
			oClone.Comment = Comment;
			oClone.DecodedValue = DecodedValue;
			oClone.Endianess = Endianess;
			oClone.Signed = Signed;
			oClone.Gain = Gain;
			oClone.IsMultiplexed = IsMultiplexed;
			oClone.Length = Length;
			oClone.MultiplexerValue = MultiplexerValue;
			oClone.Name = Name;
			oClone.RawValue = RawValue;
			oClone.StartBit = StartBit;
			oClone.Unit = Unit;
			oClone.Zero = Zero;
			oClone.IsVirtual = IsVirtual;
			oClone.VirtualChannelReference = VirtualChannelReference;
            oClone.ValueFormat = this.ValueFormat.Get_Clone();
            oClone.Alarms = this.Alarms.Get_Clone();

			return(oClone);
		}
		
		/// <summary>
		/// Return the bit number of the first bit of the parameter
		/// </summary>
		/// <returns></returns>
		public int GetParamertStartBit()
		{
			switch (Endianess)
			{
				case CanParameterEndianess.MSBFirst:
					
					int ByteStart = StartBit / 8;
					int BitStartInByte = StartBit - (8 * ByteStart);
					
					int iBit = BitStartInByte;
					int iBitStart = StartBit;
					int BitCnt = 1;
					
					while (iBit > 0 && BitCnt <= Length)
					{
						iBit--;
						iBitStart--;
						BitCnt++;
					}
					
					return(iBitStart);
					
				case CanParameterEndianess.LSBFirst:
					
					return(StartBit);
					
				default:
					
					return(-1);
			}
		}
		
		/// <summary>
		/// Return the bit number of the last bit of the parameter
		/// </summary>
		/// <returns>Bit number of the last bit of the parameter</returns>
		public int GetParameterEndBit()
		{
			switch (Endianess)
			{
				case CanParameterEndianess.MSBFirst:
					
					int ByteStart = StartBit / 8;
					int BitStartInByte = StartBit - (8 * ByteStart);
					int BitCnt = 1;
					int BitEnd = StartBit;
					
					while (BitCnt != Length)
					{
						BitEnd--;
						
						if (BitEnd < ByteStart * 8)
						{
							ByteStart++;
							BitEnd = ByteStart * 8 + 7;
						}
						
						BitCnt++;
					}
					
					return(BitEnd);
										
				case CanParameterEndianess.LSBFirst:
					
					return(StartBit + Length - 1);
					
				default:
					
					return(-1);
			}
		}

        /// <summary>
        /// Return paramater engineering min and max values according to its CAN properties
        /// </summary>
        /// <returns>Array of double, index 0: Min value, index 1: Max value</returns>
        public double[] GetParameterMinMax()
        {
            double[] MinMax = new double[2] { (double)0, (double)0 };

            if (Length > 0)
            {
                if (Signed)
                {
                    UInt64 RawMax = (UInt64)(Math.Pow(2, Length - 1) - 1);

                    MinMax[1] = (double)RawMax * Math.Abs(Gain) + Zero;
                    MinMax[0] = MinMax[1] * -1;
                }
                else
                {
                    UInt64 RawMax = (UInt64)(Math.Pow(2, Length) - 1);

                    MinMax[0] = (double)0 * Math.Abs(Gain) + Zero;
                    MinMax[1] = (double)RawMax * Math.Abs(Gain) + Zero;
                }
            }

            return (MinMax);
        }

		#endregion
	}
	
	/// <summary>
	/// Description of a CAN single message
	/// </summary>
	[Serializable]
	public class CANMessage
	{
		#region Public members
		
		public string Name;
        public string Identifier;
        public int DLC;
        public string Comment;
		public CanMsgRxTx RxTx;
		public int Period;
        public int NoRxTimeOut;
		public string MultiplexerName;
		
		public List<CANParameter> Parameters;
		
		#endregion
		
		public CANMessage()
		{
			Name="";
			Identifier="0";
            DLC = 8;
			Comment = "";
			RxTx= CanMsgRxTx.Tx;
			Period=1;
            NoRxTimeOut = 0;
			MultiplexerName = "";
			Parameters=new List<CANParameter>();
			
		}
		
		#region Public methodes
		
		/// <summary>
		/// Retrieve a parameter in the parameters list by the value  and type of one propertie of the parameter
		/// </summary>
		/// <param name="Info">Value of propertie</param>
		/// <param name="SearchType">Type of propertie</param>
		/// <returns>CAN parameter, null if not found</returns>
		public CANParameter GetCANParameter(string Info, ParameterResearchOption SearchType)
		{
			foreach(CANParameter Param in Parameters)
			{
				switch(SearchType)
				{
					case ParameterResearchOption.Name:
						
						if(Param.Name.Equals(Info))
						{
							return(Param);
						}
						
						break;
						
					case ParameterResearchOption.StartBit:
						
						if(Param.StartBit.ToString().Equals(Info))
						{
							return(Param);
						}
						
						break;
				}
			}
			
			return(null);
		}

        /// <summary>
        /// Retrieve a parameter index in the parameters list by the value  and type of one propertie of the parameter
        /// </summary>
        /// <param name="Info">Value of propertie</param>
        /// <param name="SearchType">Type of propertie</param>
        /// <returns>CAN parameter, -1 if not found</returns>
        public int GetCANParameterIndex(string Info, ParameterResearchOption SearchType)
        {
            int i = 0;

            foreach (CANParameter Param in Parameters)
            {
                switch (SearchType)
                {
                    case ParameterResearchOption.Name:

                        if (Param.Name.Equals(Info))
                        {
                            return (i);
                        }

                        break;

                    case ParameterResearchOption.StartBit:

                        if (Param.StartBit.ToString().Equals(Info))
                        {
                            return (i);
                        }

                        break;
                }

                i++;
            }

            return (-1);
        }

        /// <summary>
        /// Return a flag indicating if the parameter name given as argument is already used by another parameter
        /// </summary>
        /// <param name="ParamName">Name to test</param>
        /// <param name="ExcludedParameter">Parameter excluded of the check (null if no parameter excluded)</param>
        /// <returns>True: free name / False: name already used</returns>
        public bool IsFreeParameterName(string ParamName,CANParameter ExcludedParameter)
        {
            foreach (CANParameter Param in Parameters)
            {
                if (!(Param.Equals(ExcludedParameter)))
                {
                    if (Param.Name.Equals(ParamName))
                    {
                        return (false);
                    }
                }
            }

            return (true);
        }

        /// <summary>
        /// Return a flag indicating if the bit range used by the parameter given as argument is free within the current message 
        /// </summary>
        /// <param name="TestParameter">CAN parameter to test</param>
        /// <returns>True: free / False: not free</returns>
        public bool IsParameterSlotFree(CANParameter TestParameter)
        {
            
        	foreach(CANParameter oParam in Parameters)
        	{
        		bool BOverlap=AreParmeterOverlaping(TestParameter,oParam);
        		
        		if(BOverlap) //Parameters are overlapping
        		{
        			if(!(oParam.IsMultiplexed) | !(TestParameter.IsMultiplexed)) //At least one of them is not multiplexed
        			{
        				return(false);
        			}
        			else //Both parameters are multiplexed
        			{
        				if(oParam.MultiplexerValue == TestParameter.MultiplexerValue) //Both parameter are using the same multiplexer
        				{
        					return(false);
        				}
        			}
        		}
        	}
        	
        	return(true);
        }
        
        /// <summary>
        /// Return a clone of the current object
        /// </summary>
        /// <returns>Clone of the current object</returns>
        public CANMessage Clone()
        {
        	CANMessage oClone = new CANMessage();
        	
        	oClone.Comment = Comment;
        	oClone.Identifier = Identifier;
            oClone.DLC = DLC;
        	oClone.MultiplexerName = MultiplexerName;
        	oClone.Name = Name;
        	oClone.Period = Period;
        	oClone.RxTx = RxTx;
        	
        	foreach(CANParameter oParam in Parameters)
        	{
        		oClone.Parameters.Add(oParam.Clone());
        	}
        	
        	return(oClone);
        }
        
        /// <summary>
        /// Return a flag indicating whether the message contains virtual parameter
        /// </summary>
        /// <returns>Flag indicating whether the message contains virtual parameter</returns>
        public bool ContainsVirtualParameters()
        {
        	foreach (CANParameter oParam in Parameters)
        	{
        		if (oParam.IsVirtual)
        		{
        			return(true);
        		}
        	}
        	
        	return(false);
        }

        #endregion

        #region Protected methodes

        protected UInt64 SwapBytes(UInt64 ValIn, int ByteLen)
        {
            UInt64 ValOut = 0;

            for (int iByte = 0; iByte < ByteLen; iByte++)
            {
                UInt64 ByteVal = (ValIn & ((UInt64)0xFF << (iByte * 8))) >> (iByte * 8);
                ValOut |= (ByteVal << ((ByteLen - iByte - 1) * 8));
            }

            return (ValOut);
        }

        #endregion

        #region Private methodes

        private bool AreParmeterOverlaping(CANParameter ParamA, CANParameter ParamB)
		{
			if (!(ParamA.Name.Equals(ParamB.Name)))
			{
				int ParamA_BitsCheckCnt = 0;
				int ParamA_BitIndex = ParamA.StartBit;
				int ParamA_StartByte = ParamA.StartBit / 8;
				
				while (ParamA_BitsCheckCnt < ParamA.Length)
				{
					int ParamB_BitIndex = ParamB.StartBit;
					int Param_B_StartByte = ParamB.StartBit / 8;
					
					//Check all bits of param B for the current bit of param A
					for (int i=0; i<ParamB.Length; i++)
					{
						if (ParamA_BitIndex == ParamB_BitIndex)
						{
							return(true);
						}
						else
						{
							switch (ParamB.Endianess)
							{
								case CanParameterEndianess.MSBFirst:
									
									ParamB_BitIndex--;
									
									if (ParamB_BitIndex < Param_B_StartByte * 8)
									{
										Param_B_StartByte++;
										ParamB_BitIndex = Param_B_StartByte * 8 + 7;
									}
									
									break;
									
								case CanParameterEndianess.LSBFirst:
									
									ParamB_BitIndex++;
									
									if (ParamB_BitIndex > Param_B_StartByte * 8 + 7)
									{
										Param_B_StartByte++;
										ParamB_BitIndex = Param_B_StartByte * 8;
									}
									
									break;
							}
						}
					}
					
					//Next bit of param A
					switch (ParamA.Endianess)
					{
						case CanParameterEndianess.MSBFirst:
							
							ParamA_BitIndex--;
							
							if (ParamA_BitIndex < ParamA_StartByte * 8)
							{
								ParamA_StartByte++;
								ParamA_BitIndex = ParamA_StartByte * 8 + 7;
							}
							
							break;
							
						case CanParameterEndianess.LSBFirst:
							
							ParamA_BitIndex++;
							
							if (ParamA_BitIndex > ParamA_StartByte * 8 + 7)
							{
								ParamA_StartByte++;
								ParamA_BitIndex = ParamA_StartByte * 8;
							}
							
							break;
					}
					
					ParamA_BitsCheckCnt++;
				}
			}
			
			return(false);
			
//			int EndBitParamA = ParamA.StartBit + ParamA.Length - 1;
//			int EndBitParamB = ParamB.StartBit + ParamB.Length - 1;
//			
//			if(!(ParamA.Name.Equals(ParamB.Name)))
//			{
//				if((ParamA.StartBit >= ParamB.StartBit & ParamA.StartBit < EndBitParamB)
//				   | (EndBitParamA >= ParamB.StartBit & EndBitParamA < EndBitParamB))
//				{
//					return(true);
//				}
//			}
//			
//			return(false);
		}
		
		#endregion
	}
	
	/// <summary>
	/// CAN Messages configuration file
	/// Contains the description of CAN messages of the stream
	/// </summary>
	[Serializable]
	public class CANMessagesConfiguration
	{
		#region Public members
		
		public string Name;
		public string ConfigFilePath;
		public int CanRate;
		
		public List<CANMessage> Messages;
		
		#endregion
		
		public CANMessagesConfiguration()
		{
			Name="";
			ConfigFilePath = "";
			CanRate=1000;
			Messages=new List<CANMessage>();
		}
		
		#region Public methodes
		
		/// <summary>
		/// Retrieve a message in the messages list by the value and type of one propertie of the message
		/// </summary>
		/// <param name="Info">Value of propertie</param>
		/// <param name="SearchType">Type of propertie</param>
		/// <returns>CAN Message, null if not found</returns>
		public CANMessage GetCANMessage(string Info,MessageResearchOption SearchType)
		{
			foreach(CANMessage Message in Messages)
			{
				switch(SearchType)
				{
					case MessageResearchOption.Name:
						
						if(Message.Name.Equals(Info))
						{
							return(Message);
						}
						
						break;
						
					case MessageResearchOption.Identifier:
						
						if(Message.Identifier.ToUpper().Equals(Info.ToUpper()))
						{
							return(Message);
						}
						
						break;
				}
			}
			
			return(null);
		}

        /// <summary>
        /// Retrieve a message index in the messages list by the value and type of one propertie of the message
        /// </summary>
        /// <param name="Info">Value of propertie</param>
        /// <param name="SearchType">Type of propertie</param>
        /// <returns>CAN Message index, -1 if not found</returns>
        public int GetCANMessageIndex(string Info, MessageResearchOption SearchType)
        {
            int i = 0;

            foreach (CANMessage Message in Messages)
            {
                switch (SearchType)
                {
                    case MessageResearchOption.Name:

                        if (Message.Name.Equals(Info))
                        {
                            return (i);
                        }

                        break;

                    case MessageResearchOption.Identifier:

                        if (Message.Identifier.ToUpper().Equals(Info.ToUpper()))
                        {
                            return (i);
                        }

                        break;
                }

                i++;
            }

            return (-1);
        }
		
        /// <summary>
        /// Returns the first CAN parameter found among CAN messages that has a name matching a given name
        /// </summary>
        /// <param name="ParameterName">Name of the parameter to search</param>
        /// <returns>CAN Parameter found (null if not found)</returns>
        public CANParameter GetCANParameter(string ParameterName)
        {
            foreach (CANMessage oMsg in Messages)
            {
                CANParameter oParam = oMsg.GetCANParameter(ParameterName, ParameterResearchOption.Name);

                if (!(oParam == null))
                {
                    return (oParam);
                }
            }

            return (null);
        }

        /// <summary>
		/// Write the CAN configuration object into a XML file
		/// </summary>
		/// <param name="FilePath">Path of destination file</param>
        public void WriteCANConfigurationFile(string FilePath)
        {
        	WriteCANConfigurationFile(FilePath, true);
        }
        
		/// <summary>
		/// Write the CAN configuration object into a XML file
		/// </summary>
		/// <param name="FilePath">Path of destination file</param>
		/// <param name="UpdatePath">Configuration file path update flag (do not set for backup copy in the app data folder</param>
		public void WriteCANConfigurationFile(string FilePath, bool UpdatePath)
		{
			if (Name.Equals(""))
			{
				Name = Path.GetFileNameWithoutExtension(FilePath);
			}
			
			if (UpdatePath) ConfigFilePath = FilePath;
			
			XmlDocument oXmlConfig = new XmlDocument();
			
			XmlElement xConfig = CreateCANConfigurationXmlNode(oXmlConfig, "CANConfiguration");
			oXmlConfig.AppendChild(xConfig);
						
			oXmlConfig.Save(FilePath);
		}
				
		/// <summary>
		/// Create the CAN configuration XML node
		/// </summary>
		/// <param name="oXmlDoc">XmlDocument class reference creating the node</param>
		/// <param name="NodeName">Name of the node to create</param>
		/// <returns>CAN configuration XML node</returns>
		public XmlElement CreateCANConfigurationXmlNode(XmlDocument oXmlDoc, string NodeName)
		{
			XmlElement xConfig = oXmlDoc.CreateElement(NodeName);
			
            XmlAttribute xAtrBusRate = oXmlDoc.CreateAttribute("CAN_Rate");
            xAtrBusRate.Value = CanRate.ToString();
            xConfig.Attributes.Append(xAtrBusRate);
            
            XmlAttribute xAtrCfgName = oXmlDoc.CreateAttribute("CAN_ConfigName");
            xAtrCfgName.Value = Name;
            xConfig.Attributes.Append(xAtrCfgName);

			foreach(CANMessage Message in Messages)
			{
				XmlElement xMessage=oXmlDoc.CreateElement("CANMessage");
				
				XmlAttribute xAtrName=oXmlDoc.CreateAttribute("Name");
				xAtrName.Value=Message.Name;
				xMessage.Attributes.Append(xAtrName);
				
				XmlAttribute xAtrIdentifier=oXmlDoc.CreateAttribute("Identifier");
                xAtrIdentifier.Value = Message.Identifier;
				xMessage.Attributes.Append(xAtrIdentifier);

                XmlAttribute xAtrDLC = oXmlDoc.CreateAttribute("DLC");
                xAtrDLC.Value = Message.DLC.ToString();
                xMessage.Attributes.Append(xAtrDLC);
				
				XmlElement xComment=oXmlDoc.CreateElement("MsgComment");
				xComment.InnerText=Message.Comment;
				xMessage.AppendChild(xComment);
				
				XmlElement xRxTx=oXmlDoc.CreateElement("RxTx");
				xRxTx.InnerText=Message.RxTx.ToString();
				xMessage.AppendChild(xRxTx);
				
				XmlElement xPeriod=oXmlDoc.CreateElement("Period");
				xPeriod.InnerText=Message.Period.ToString();
				xMessage.AppendChild(xPeriod);

                XmlElement xNoRx = oXmlDoc.CreateElement("NoRxTimeout");
                xNoRx.InnerText = Message.NoRxTimeOut.ToString();
                xMessage.AppendChild(xNoRx);
				
				XmlElement xMux=oXmlDoc.CreateElement("Multiplexer");
				xMux.InnerText=Message.MultiplexerName;
				xMessage.AppendChild(xMux);
				
				XmlElement xParameters=oXmlDoc.CreateElement("Parameters");
				xMessage.AppendChild(xParameters);
				
				foreach(CANParameter Parameter in Message.Parameters)
				{
					XmlElement xParameter=oXmlDoc.CreateElement("CANParameter");
					
					XmlAttribute xAtrParamName=oXmlDoc.CreateAttribute("ParameterName");
					xAtrParamName.Value=Parameter.Name;
					xParameter.Attributes.Append(xAtrParamName);
					
					XmlElement xParamComment=oXmlDoc.CreateElement("ParameterComment");
					xParamComment.InnerText=Parameter.Comment;
					xParameter.AppendChild(xParamComment);
					
					XmlElement xParamUnit=oXmlDoc.CreateElement("Unit");
					xParamUnit.InnerText=Parameter.Unit;
					xParameter.AppendChild(xParamUnit);
					
					XmlElement xParamStart=oXmlDoc.CreateElement("StartBit");
					xParamStart.InnerText=Parameter.StartBit.ToString();
					xParameter.AppendChild(xParamStart);
					
					XmlElement xParamLength=oXmlDoc.CreateElement("Length");
					xParamLength.InnerText=Parameter.Length.ToString();
					xParameter.AppendChild(xParamLength);
					
					XmlElement xParamEndianess=oXmlDoc.CreateElement("Endianess");
					xParamEndianess.InnerText=Parameter.Endianess.ToString();
					xParameter.AppendChild(xParamEndianess);
					
					XmlElement xParamSigned=oXmlDoc.CreateElement("Signed");
					xParamSigned.InnerText=Parameter.Signed.ToString();
					xParameter.AppendChild(xParamSigned);
					
					XmlElement xParamGain=oXmlDoc.CreateElement("LinearizationGain");
					xParamGain.InnerText=Parameter.Gain.ToString();
					xParameter.AppendChild(xParamGain);
					
					XmlElement xParamZero=oXmlDoc.CreateElement("LinearizationZero");
					xParamZero.InnerText=Parameter.Zero.ToString();
					xParameter.AppendChild(xParamZero);

                    xParameter.AppendChild(Parameter.ValueFormat.GetSignalFormatXmlNode(oXmlDoc, "ValueFormat"));

                    xParameter.AppendChild(Parameter.Alarms.GetSignalAlarmsXmlNode(oXmlDoc, "ParameterAlarms"));

                    if (Parameter.IsMultiplexed)
					{
						XmlElement xParamMux = oXmlDoc.CreateElement("MultiplexerValue");
						xParamMux.InnerText = Parameter.MultiplexerValue.ToString();
                    	xParameter.AppendChild(xParamMux);
					}
					
					if (Parameter.IsVirtual)
					{
						XmlElement xVirtualRef = oXmlDoc.CreateElement("VirtualReference");
						xParameter.AppendChild(xVirtualRef);
						
						XmlElement xVirtualLib = oXmlDoc.CreateElement("VirtualLibrary");
						xVirtualLib.InnerText = Parameter.VirtualChannelReference.LibraryName;
						xVirtualRef.AppendChild(xVirtualLib);
						
						XmlElement xVirtualChan = oXmlDoc.CreateElement("VirtualChannel");
						xVirtualChan.InnerText = Parameter.VirtualChannelReference.ChannelName;
						xVirtualRef.AppendChild(xVirtualChan);
					}
					
					xParameters.AppendChild(xParameter);
				}
				
				xConfig.AppendChild(xMessage);
			}
			
			return(xConfig);
		}
		
		/// <summary>
		/// Read the CAN configuration object in a XML file
		/// </summary>
		/// <param name="FilePath">Path of the file to read</param>
		/// <returns>Read error flag: True = No Error / False = Error</returns>
		public bool ReadCANConfigurationFile(string FilePath)
		{
			XmlDocument oXmlConfig=new XmlDocument();
			oXmlConfig.Load(FilePath);
			ConfigFilePath = FilePath;
			
			return(ReadCANConfigurationXmlNode(oXmlConfig.FirstChild));
		}
		
		/// <summary>
		/// Read the CAN configuration object XML node
		/// </summary>
		/// <param name="xConfig">XML Node to read</param>
		/// <returns>Read error flag: True = No Error / False = Error</returns>
		public bool ReadCANConfigurationXmlNode(XmlNode xConfig)
		{
			Messages=new List<CANMessage>();

			XmlAttribute xAtrBusRate = xConfig.Attributes["CAN_Rate"];
			if (!(xAtrBusRate == null))
			{
				int Rate = 0;
				if (int.TryParse(xAtrBusRate.Value, out Rate))
				{
					CanRate = Rate;
				}
				else
				{
					return (false);
				}
			}
			else
			{
				return (false);
			}
			
			XmlAttribute xAtrCfgName = xConfig.Attributes["CAN_ConfigName"];
			if (!(xAtrCfgName == null))
			{
				Name = xAtrCfgName.Value;
			}

			foreach(XmlNode xMessage in xConfig.ChildNodes)
			{
				if(xMessage.Name.Equals("CANMessage"))
				{
					CANMessage oMessage=new CANMessage();
					
					//Name
					XmlAttribute xAtrName=xMessage.Attributes["Name"];
					if(!(xAtrName==null))
					{
						oMessage.Name=xAtrName.Value;
					}
					else
					{
						return(false);
					}
					
					//Identifier
					XmlAttribute xAtrIdentifier=xMessage.Attributes["Identifier"];
					if(!(xAtrIdentifier==null))
					{
						try
						{
							Int32 Id = Int32.Parse(xAtrIdentifier.Value, System.Globalization.NumberStyles.HexNumber);
							oMessage.Identifier = xAtrIdentifier.Value;
						}
						catch
						{
							return (false);
						}
					}
					else
					{
						return(false);
					}

                    //DLC
                    XmlAttribute xAtrDLC = xMessage.Attributes["DLC"]; //New feature of CANStream 2.2.0.0
                    if (!(xAtrDLC == null))
                    {
                        int DLC = 0;
                        if (int.TryParse(xAtrDLC.Value, out DLC))
                        {
                            oMessage.DLC = DLC;
                        }
                        else
                        {
                            return (false);
                        }
                    }
                    else
                    {
                        oMessage.DLC = 8;
                    }

					//Comment
					XmlNode xComment=xMessage.SelectSingleNode("MsgComment");
					if(!(xComment==null))
					{
						oMessage.Comment=xComment.InnerText;
					}
					
					//RxTx
					XmlNode xRxTx=xMessage.SelectSingleNode("RxTx");
					if(!(xRxTx==null))
					{
						try
						{
							CanMsgRxTx RxTx= (CanMsgRxTx) Enum.Parse(typeof(CanMsgRxTx),xRxTx.InnerText);
							oMessage.RxTx=RxTx;
						}
						catch
						{
							return(false);
						}
					}
					else
					{
						return(false);
					}
					
					//Period
					XmlNode xPeriod=xMessage.SelectSingleNode("Period");
					if(!(xPeriod==null))
					{
						int Period=0;
						if(Int32.TryParse(xPeriod.InnerText,out Period))
						{
							oMessage.Period=Period;
						}
						else
						{
							return(false);
						}
					}
					else
					{
						return(false);
					}

                    //NoRx Timeout
                    XmlNode xNoRx = xMessage.SelectSingleNode("NoRxTimeout");
                    if (!(xNoRx == null))
                    {
                        int NoRxTimeout = 0;
                        if (int.TryParse(xNoRx.InnerText, out NoRxTimeout))
                        {
                            oMessage.NoRxTimeOut = NoRxTimeout;
                        }
                        else
                        {
                            return (false);
                        }
                    }
                    else
                    {
                        //Nothing to do: This node may not exist since it is a new feature of release 2.1.0.3
                    }

					//Multiplexer
					XmlNode xMsgMux=xMessage.SelectSingleNode("Multiplexer");
					if(!(xMsgMux==null))
					{
						oMessage.MultiplexerName=xMsgMux.InnerText;
					}
					else
					{
						oMessage.MultiplexerName="";
					}
					
					//Parameter list
					XmlNode xParameters=xMessage.SelectSingleNode("Parameters");
					if(!(xParameters==null))
					{
						foreach(XmlNode xParam in xParameters.ChildNodes)
						{
							if(xParam.Name.Equals("CANParameter"))
							{
								CANParameter oParameter=new CANParameter();
								
								//Parameter name
								XmlAttribute xAtrParamName=xParam.Attributes["ParameterName"];
								if(!(xAtrParamName==null))
								{
									oParameter.Name=xAtrParamName.Value;
								}
								else
								{
									return(false);
								}
								
								//ParameterComment
								XmlNode xParamComment=xParam.SelectSingleNode("ParameterComment");
								if(!(xParamComment==null))
								{
									oParameter.Comment=xParamComment.InnerText;
								}
								
								//Unit
								XmlNode xParamUnit=xParam.SelectSingleNode("Unit");
								if(!(xParamUnit==null))
								{
									oParameter.Unit=xParamUnit.InnerText;
								}
								
								//Start bit
								XmlNode xParamStart=xParam.SelectSingleNode("StartBit");
								if(!(xParamStart==null))
								{
									int ParamStart=0;
									if (Int32.TryParse(xParamStart.InnerText, out ParamStart))
									{
										oParameter.StartBit=ParamStart;
									}
									else
									{
										return(false);
									}
								}
								else
								{
									return(false);
								}
								
								//Length
								XmlNode xParamLength=xParam.SelectSingleNode("Length");
								if(!(xParamLength==null))
								{
									int ParamLength=0;
									if(Int32.TryParse(xParamLength.InnerText,out ParamLength))
									{
										oParameter.Length=ParamLength;
									}
									else
									{
										return(false);
									}
								}
								else
								{
									return(false);
								}
								
								//Endianess
								XmlNode xEndianess=xParam.SelectSingleNode("Endianess");
								if(!(xEndianess==null))
								{
									try
									{
										CanParameterEndianess Endianess;
										Endianess= (CanParameterEndianess)Enum.Parse(typeof(CanParameterEndianess),xEndianess.InnerText);
										oParameter.Endianess=Endianess;
									}
									catch
									{
										return(false);
									}
								}
								else
								{
									return(false);
								}
								
								//Signed
								XmlNode xParamSigned=xParam.SelectSingleNode("Signed");
								if(!(xParamSigned==null)) //New property of release 1.4.0 => No error if 'Signed' node not found for compatibility with previous release CAN Configuration file
								{
									bool Signedness = false;
									if (bool.TryParse(xParamSigned.InnerText, out Signedness))
									{
										oParameter.Signed = Signedness;
									}
									else
									{
										return(false);
									}
								}
								
								//Gain
								XmlNode xParamGain=xParam.SelectSingleNode("LinearizationGain");
								if(!(xParamGain==null))
								{
									double Gain=0;
									if(Double.TryParse(xParamGain.InnerText,out Gain))
									{
										oParameter.Gain=Gain;
									}
									else
									{
										return(false);
									}
								}
								else
								{
									return(false);
								}
								
								//Zero
								XmlNode xParamZero=xParam.SelectSingleNode("LinearizationZero");
								if(!(xParamZero==null))
								{
									double Zero;
									if(Double.TryParse(xParamZero.InnerText,out Zero))
									{
										oParameter.Zero=Zero;
									}
									else
									{
										return(false);
									}
								}
								else
								{
									return(false);
								}

                                //Format
                                XmlNode xParamFormat = xParam.SelectSingleNode("ValueFormat");
                                if (!(xParamFormat==null))
                                {
                                    if (oParameter.ValueFormat.ReadSignalFormatXmlNode(xParamFormat) == false)
                                    {
                                        return (false);
                                    }
                                }
                                else
                                {
                                    //'ValueFormat' node may not exists since it is a new feature of release 2.1.0.0
                                    //If 'ValueFormat' node doesn't exist, CAN Paramter will use default properties
                                }

                                //Parameter alarms
                                XmlNode xParamAlarms = xParam.SelectSingleNode("ParameterAlarms");
                                if (!(xParamAlarms == null))
                                {
                                    if (oParameter.Alarms.ReadSignalAlarmsXmlNode(xParamAlarms) == false)
                                    {
                                        return (false);
                                    }
                                }
                                else
                                {
                                    //'ParameterAlarms' node may not exists since it is a new feature of release 2.1.0.0
                                    //If 'ParameterAlarms' node doesn't exist, CAN Paramter will use default properties
                                }

                                //MultiplexerId
                                XmlNode xMuxValue = xParam.SelectSingleNode("MultiplexerValue");
								if (!(xMuxValue == null))
								{
									long MuxVal=0;
									if(long.TryParse(xMuxValue.InnerText,out MuxVal))
									{
										oParameter.IsMultiplexed=true;
										oParameter.MultiplexerValue=MuxVal;
									}
									else
									{
										return(false);
									}
								}
								else
								{
									oParameter.IsMultiplexed=false;
									oParameter.MultiplexerValue=0;
								}
								
								//Virtual channel reference
								XmlNode xVirtualRef = xParam.SelectSingleNode("VirtualReference");
								if (!(xVirtualRef == null))
								{
									XmlNode xVirtualLib = xVirtualRef.SelectSingleNode("VirtualLibrary");
									if (!(xVirtualLib == null))
									{
										oParameter.VirtualChannelReference.LibraryName = xVirtualLib.InnerText;
									}
									else
									{
										return(false);
									}
									
									XmlNode xVirtualChan = xVirtualRef.SelectSingleNode("VirtualChannel");
									if (!(xVirtualChan == null))
									{
										oParameter.VirtualChannelReference.ChannelName = xVirtualChan.InnerText;
										oParameter.IsVirtual = true;
									}
									else
									{
										return(false);
									}
								}
								else
								{
									oParameter.IsVirtual = false;
								}
								
								oMessage.Parameters.Add(oParameter);
							}
						}
					}
					else
					{
						return(false);
					}
					
					Messages.Add(oMessage);
				}
			}
			
			
			return(true);
		}
		
        /// <summary>
        /// Check if the name given as argument is already used
        /// </summary>
        /// <param name="MsgName">Name to check</param>
        /// <param name="ExcludedMessage">Message excluded of check (null if no message excluded)</param>
        /// <returns>Flag indicating if the name is already used or not</returns>
        public bool IsFreeMessageName(string MsgName, CANMessage ExcludedMessage)
        {
            foreach (CANMessage oMsg in Messages)
            {
                if (!(oMsg.Equals(ExcludedMessage)))
                {
                    if (oMsg.Name.Equals(MsgName))
                    {
                        return (false);
                    }
                }
            }

            return (true);
        }

        /// <summary>
        /// Check if the identifier given as argument is free
        /// </summary>
        /// <param name="Identifier">Identifier to check</param>
        /// <param name="ExcludedMessage">Message excluded of check (null if no message excluded)</param>
        /// <returns>Flag indicating if the identifier is free or not</returns>
        public bool IsFreeIdentifier(string Identifier, CANMessage ExcludedMessage)
        {
            foreach (CANMessage oMessage in Messages)
            {
                if (!(oMessage.Equals(ExcludedMessage)))
                {
                	if (oMessage.Identifier.ToUpper().Equals(Identifier.ToUpper()))
                    {
                        return (false);
                    }
                }
            }

            return (true);
        }
        
        /// <summary>
        /// Check if the identifier given as argument is used
        /// </summary>
        /// <param name="Identifier">Identifier to check</param>
        /// <returns>Flag indicating if the identifier is used or not</returns>
        public bool IsUsedIdentifier(string Identifier)
        {
        	foreach(CANMessage oMessage in Messages)
        	{
        		if(oMessage.Identifier.ToUpper().Equals(Identifier.ToUpper()))
        		{
        			return(true);
        		}
        	}
        	
        	return(false);
        }
        
        /// <summary>
        /// Return the number of Tx parameters
        /// </summary>
        /// <returns>Number of Tx parameters</returns>
        public int GetTxParameterCount()
        {
            int TxCount = 0;
            foreach (CANMessage oMsg in Messages)
            {
                if (oMsg.RxTx.Equals(CanMsgRxTx.Tx))
                {
                    TxCount = TxCount + oMsg.Parameters.Count;
                }
            }

            return (TxCount);
        }
		
        /// <summary>
        /// Return the number of Tx parameters with or without virtual parameters
        /// </summary>
        /// <param name="IncludeVirtual">Virtual parameters included flag</param>
        /// <returns>Number of Tx parameters</returns>
        public int GetTxParameterCount(bool IncludeVirtual)
        {
        	int TxCount = 0;
            foreach (CANMessage oMsg in Messages)
            {
            	if (oMsg.RxTx.Equals(CanMsgRxTx.Tx))
            	{
            		if (IncludeVirtual)
            		{
            			TxCount += oMsg.Parameters.Count;
            		}
            		else
            		{
            			foreach (CANParameter oParam in oMsg.Parameters)
            			{
            				if (!oParam.IsVirtual)
            				{
            					TxCount++;
            				}
            			}
            		}
            	}
            }
            
            return(TxCount);
        }
        
        /// <summary>
        /// Return the number of parameters
        /// </summary>
        /// <returns>Number of parameters</returns>
        public int GetParameterCount()
        {
        	int Count = 0;
            foreach (CANMessage oMsg in Messages)
            {
                Count = Count + oMsg.Parameters.Count;
            }
            
            return(Count);
        }
        
        /// <summary>
        /// Return a clone of the current object
        /// </summary>
        /// <returns>clone of the current object</returns>
        public CANMessagesConfiguration Clone()
        {
        	CANMessagesConfiguration oClone = new CANMessagesConfiguration();
        	
        	oClone.CanRate = CanRate;
        	oClone.ConfigFilePath = ConfigFilePath;
        	oClone.Name = Name;
        	
        	foreach(CANMessage oMsg in Messages)
        	{
        		oClone.Messages.Add(oMsg.Clone());
        	}
        	
        	return(oClone);
        }
        
		#endregion
	}
	
	/// <summary>
	/// Single CAN bus controller configuration class
	/// </summary>
	[Serializable]
	public class CANBusContoller: CANMessagesConfiguration
	{
		#region Public members
		
		public string ControllerName;
		public string Description;
		
		#endregion
		
		public CANBusContoller()
		{
			ControllerName = "";
		}
		
		public CANBusContoller(CANMessagesConfiguration BaseMsgCfg)
		{
			ControllerName = "";
			Set_ControllerCanConfiguration(BaseMsgCfg);
		}
		
		#region Public methodes
		
		public new CANBusContoller Clone()
		{
			CANBusContoller oClone = new CANBusContoller(base.Clone());
			
			oClone.ControllerName = ControllerName;
			
			return(oClone);
		}
		
		public void Set_ControllerCanConfiguration(CANMessagesConfiguration oCanCfg)
		{
			this.CanRate = oCanCfg.CanRate;
			this.ConfigFilePath = oCanCfg.ConfigFilePath;
			this.Name = oCanCfg.Name;
			
			this.Messages = new List<CANMessage>();
			
			foreach(CANMessage oMsg in oCanCfg.Messages)
			{
				this.Messages.Add(oMsg.Clone());
			}
		}
		
		#endregion
	}
	
	/// <summary>
	/// Multiple CAN bus controller configuration class
	/// </summary>
	public class MultipleContollerCANConfiguration
	{
		#region Public members
		
		public string Name;
		public string FilePath;
		public List<CANBusContoller> Controllers;
		
		#endregion
		
		public MultipleContollerCANConfiguration()
		{
			Name= "";
			FilePath = "";
			Controllers = new List<CANBusContoller>();
		}
		
		#region Public methodes
		
		public void WriteControllersConfiguration(string fPath)
		{
			XmlDocument oXDoc = new XmlDocument();
			
			XmlElement xControllersCfg = oXDoc.CreateElement("MultipleContollerCANConfiguration");
			oXDoc.AppendChild(xControllersCfg);
			
			if (Name.Equals(""))
			{
				Name = Path.GetFileNameWithoutExtension(fPath);
			}
			
			XmlAttribute xAtrName = oXDoc.CreateAttribute("Name");
			xAtrName.Value = Name;
			xControllersCfg.Attributes.Append(xAtrName);
			
				XmlElement xControllers = oXDoc.CreateElement("Controllers");
				xControllersCfg.AppendChild(xControllers);
				
				foreach (CANBusContoller oCanCtrl in Controllers)
				{
					XmlElement xCanCtrl = oCanCtrl.CreateCANConfigurationXmlNode(oXDoc, "ControllerConfiguration");
					
					XmlAttribute xAtrCtrlName = oXDoc.CreateAttribute("ControllerName");
					xAtrCtrlName.Value = oCanCtrl.ControllerName;
					xCanCtrl.Attributes.Append(xAtrCtrlName);
					
						XmlElement xCtrlDesc = oXDoc.CreateElement("ControllerDescription");
						xCtrlDesc.InnerText = oCanCtrl.Description;
						xCanCtrl.AppendChild(xCtrlDesc);
					
					xControllers.AppendChild(xCanCtrl);
				}
				
			oXDoc.Save(fPath);
			FilePath = fPath;
		}
		
		public bool ReadControllersConfiguaration(string fPath)
		{
			XmlDocument oXDoc = new XmlDocument();
			
			try
			{
				oXDoc.Load(fPath);
				
				XmlNode xControllersCfg = oXDoc.SelectSingleNode("MultipleContollerCANConfiguration");
				Name = xControllersCfg.Attributes["Name"].Value;
				
				XmlNode xControllers = xControllersCfg.SelectSingleNode("Controllers");
				Controllers = new List<CANBusContoller>();
				
				foreach (XmlNode xCanCtrl in xControllers.ChildNodes)
				{
					CANBusContoller oCanCtrl =  new CANBusContoller();
					
					oCanCtrl.ControllerName = xCanCtrl.Attributes["ControllerName"].Value;
					
					if (oCanCtrl.ReadCANConfigurationXmlNode(xCanCtrl))
					{
							XmlNode xCtrlDesc = xCanCtrl.SelectSingleNode("ControllerDescription");
							oCanCtrl.Description = xCtrlDesc.InnerText;
						
						Controllers.Add(oCanCtrl);
					}
					else
					{
						return(false);
					}
				}
			}
			catch
			{
				return(false);
			}
			
			return(true);
		}
		
		public CANBusContoller Get_BusController(string ControllerName)
		{
			foreach (CANBusContoller oCtrl in Controllers)
			{
				if (oCtrl.ControllerName.Equals(ControllerName))
				{
					return(oCtrl);
				}
			}
			
			return(null);
		}
		
		public int Get_BusControllerId(string ControllerName)
		{
			for (int i=0; i<Controllers.Count; i++)
			{
				if (Controllers[i].ControllerName.Equals(ControllerName))
				{
					return(i);
				}
			}
			
			return(-1);
		}
		
		public bool ControllerExists(string ControllerName)
		{
			foreach (CANBusContoller oCtrl in Controllers)
			{
				if (oCtrl.ControllerName.Equals(ControllerName))
				{
					return(true);
				}
			}
			
			return(false);
		}
		
		#endregion
	}

	#endregion
	
	#region CAN message decoding
	
	public class CANMessageDecoded:CANMessage
	{
		#region public members
		
		public bool bMessageDecoded;
        public DateTime LastRxDate;
        
		#endregion
		
		#region private members
		
		private TPCANMsg MsgRaw;
        
		#endregion
		
		public CANMessageDecoded(CANMessage MessageCfg, TPCANMsg RawMessage)
		{
			if(!(MessageCfg==null))
			{
				Name=MessageCfg.Name;
				Identifier=MessageCfg.Identifier +"h";
				RxTx= MessageCfg.RxTx;
				Period=MessageCfg.Period;
                NoRxTimeOut = MessageCfg.NoRxTimeOut;
				Comment=MessageCfg.Comment;
				Parameters=MessageCfg.Parameters;
                MultiplexerName = MessageCfg.MultiplexerName;
                LastRxDate = DateTime.Now;
			}
			
			if(RawMessage.DATA.Length>0)
			{
				MsgRaw=RawMessage;
				DecodeMessage();
			}
		}
		
		public CANMessageDecoded(CANMessage MessageCfg, TraceRecord Record)
		{
			if(!(MessageCfg==null))
			{
				Name=MessageCfg.Name;
				Identifier=MessageCfg.Identifier +"h";
				RxTx= MessageCfg.RxTx;
				Period=MessageCfg.Period;
                NoRxTimeOut = MessageCfg.NoRxTimeOut;
				Comment=MessageCfg.Comment;
				Parameters=MessageCfg.Parameters;
                MultiplexerName = MessageCfg.MultiplexerName;
                LastRxDate = DateTime.Now;
			}
			
			if (Record.MessageData.Length>0)
			{
				MsgRaw = new TPCANMsg();
				MsgRaw.DATA = Record.MessageData;
				DecodeMessage();
				
			}
		}
		
		#region public methodes
		
		public void UpdateRawMsgBytes(byte[] Data)
		{
            LastRxDate = DateTime.Now;
            MsgRaw.DATA=Data;
			DecodeMessage();
		}
		
		#endregion
		
		#region private methodes
		
		private bool GetMultiplexerValue(string oMuxName, out long Value)
		{
			Value = 0;
			
			CANParameter PMux=GetCANParameter(oMuxName, ParameterResearchOption.Name);
			
			if(!(PMux==null))
			{
				if(DecodeParameter(PMux))
				{
					Value = (long)PMux.DecodedValue;
					return(true);
				}
			}
			
			return(false);
		}
		
		private void DecodeMessage()
		{
			long MuxVal=-1;
			bool bMuxExists = true;
			
			bMessageDecoded=false;
			
			//Get Multiplexer value
			if(!(MultiplexerName.Equals("")))
			{
				if (!(GetMultiplexerValue(MultiplexerName, out MuxVal)))
				{
					bMuxExists = false;
				}
			}
			
			//Decode all parameters except the multiplexer which has been decoded first
			foreach (CANParameter oParam in Parameters)
			{
				if (!(oParam.Name.Equals(MultiplexerName)))
				{
					if ((!oParam.IsMultiplexed) || (oParam.IsMultiplexed && bMuxExists && oParam.MultiplexerValue == MuxVal))
					{
						if (!DecodeParameter(oParam)) return;
					}
				}
			}
			
			bMessageDecoded=true;
		}
		
		private bool DecodeParameter(CANParameter oParam)
		{
			int Rem = 0;
			int ByteLen = Math.DivRem(oParam.Length, 8, out Rem);
			if (Rem != 0) ByteLen++;
			
			if (ByteLen == 0) return(false);
			
			byte[] ParamBytes = new byte[ByteLen];
			
			int iMapByte = oParam.StartBit / 8;
			int iBitInMapByte = oParam.StartBit - iMapByte * 8;
			
			switch (oParam.Endianess)
			{
				case CanParameterEndianess.MSBFirst:
					
					for (int iByte = 0; iByte < ByteLen; iByte++)
					{
						int iBitStartParamByte = 7;
						if (iByte == 0 && Rem != 0) iBitStartParamByte = Rem - 1;
						
						for (int iBit = iBitStartParamByte; iBit >= 0; iBit--)
						{
							int BitVal = (MsgRaw.DATA[iMapByte] & (1 << iBitInMapByte)) >> iBitInMapByte;
							byte ParamBitVal = (byte)(BitVal << iBit);
							
							ParamBytes[iByte] += ParamBitVal;
							
							iBitInMapByte--;
							if (iBitInMapByte < 0)
							{
								iMapByte++;
								iBitInMapByte = 7;
							}
						}
					}
					
					break;
					
				case CanParameterEndianess.LSBFirst:
					
					for (int iByte = ByteLen - 1; iByte >= 0; iByte--)
					{
						int iBitEndParamByte = 8;
						if (iByte == 0 && Rem != 0) iBitEndParamByte = Rem;
						
						for (int iBit = 0; iBit < iBitEndParamByte; iBit++)
						{
							int BitVal = (MsgRaw.DATA[iMapByte] & (1 << iBitInMapByte)) >> iBitInMapByte;
							byte ParamBitVal = (byte)(BitVal << iBit);
							
							ParamBytes[iByte] += ParamBitVal;
							
							iBitInMapByte++;
							if (iBitInMapByte > 7)
							{
								iMapByte++;
								iBitInMapByte = 0;	
							}
						}
					}
					
					break;
			}
			
			//Addition of all bytes
			UInt64 RawValue = 0;
			for (int iByte = 0; iByte < ByteLen; iByte++)
			{
				RawValue += (UInt64)(ParamBytes[iByte] << ((ByteLen - iByte - 1) * 8));
			}

			//Raw to engineering value conversion
			if (oParam.Signed)
			{
                byte SignBit = (byte)((RawValue & ((UInt64)0x01 << (oParam.Length - 1))) >> (oParam.Length - 1));
                RawValue &= ((UInt64)(Math.Pow(2, (oParam.Length - 1)) - 1));

                oParam.DecodedValue = (double)RawValue * oParam.Gain + oParam.Zero;

                if (SignBit == 0x01)
                {
                    oParam.DecodedValue *= -1;
                }
            }
			else
			{
				oParam.DecodedValue = (double)RawValue * oParam.Gain + oParam.Zero;
			}
			
            if (oParam.Endianess== CanParameterEndianess.LSBFirst)
            {
                RawValue = SwapBytes(RawValue, ByteLen);
            }

            oParam.RawValue = "0x" + string.Format("{0:X" + (ByteLen * 2).ToString() + "}", RawValue);

            return (true);
		}
		
		#endregion
	}
	
	#endregion
	
	#region CAN message encoding
	
	public class CANMessageEncoded:CANMessage
	{
        #region Public members

        public UInt32 uMessageId;
		public bool HasVirtualParameters;

        public ulong TxCount;

        #endregion

        #region Internal members

        internal bool Send;

        #endregion

        #region Private members

        private List<long> MuxTable;
		private int MuxIndex;
		private CANParameter oMultiplexer;
		
		private byte[] ByteMessageData;
		
		#endregion
		
		public CANMessageEncoded(CANMessage MessageCfg)
		{
			if(!(MessageCfg==null))
			{
				Name=MessageCfg.Name;
				Identifier=MessageCfg.Identifier +"h";
                DLC = MessageCfg.DLC;
				RxTx= MessageCfg.RxTx;
				Period=MessageCfg.Period;
				Comment=MessageCfg.Comment;
				MultiplexerName=MessageCfg.MultiplexerName;
				Parameters=MessageCfg.Parameters;
				
				uMessageId=(UInt32)NumberBaseConverter.Hex2Dec(MessageCfg.Identifier);
				ByteMessageData=new byte[MessageCfg.DLC];
                Send = true;

                HasVirtualParameters = false;
				foreach (CANParameter oParam in Parameters)
				{
					if (oParam.IsVirtual)
					{
						HasVirtualParameters = true;
						break;
					}
				}
				
				InitMultiplexer();

                TxCount = 0;
            }
		}
		
		#region Public methodes
				
		public TPCANMsg GetPCANMessage()
		{
			TPCANMsg CANMsg=new TPCANMsg();
			
			CANMsg.ID=uMessageId;
			CANMsg.LEN=(byte)ByteMessageData.Length;
			CANMsg.MSGTYPE= TPCANMessageType.PCAN_MESSAGE_STANDARD;
			CANMsg.DATA=ByteMessageData;
			
			return(CANMsg);
		}
		
		public CANMessageData GetCANMessageData()
		{
			CANMessageData MsgData=new CANMessageData();
			
			MsgData.uMessageId=uMessageId;
			MsgData.byteMessageData=ByteMessageData;
			
			return(MsgData);
		}
		
		public void SetMultiplexer()
		{
			if (MuxTable.Count > 0)
			{
				MuxIndex++;
				if (MuxIndex == MuxTable.Count) MuxIndex = 0;
				
				if (!(oMultiplexer == null))
				{
					oMultiplexer.DecodedValue = (double)MuxTable[MuxIndex];
					EncodeMessage();
				}
				
			}
		}
		
		public void EncodeMessage()
		{
			ByteMessageData = new byte[DLC];
			
			foreach(CANParameter oParam in Parameters)
			{                
                /*
                    All parameters encoded in order to update the raw value of any parameter changed into the CAN data grid, 
                    even if it is a multiplexed parameter which is not sent due to the multiplexer value 
                */
                
                //Get paramter value into an array of bytes, most significant byte being byte 0
                // 4258 = 0x10A2 => Byte[0] = 0x10, Byte[1] = 0xA2
                byte[] ParamBytes = GetParameterBytes(oParam);

                if ((!oParam.IsMultiplexed) ||(oParam.IsMultiplexed && oParam.MultiplexerValue == GetCurrentMultiplexerValue()))
				{
					int iMapByte = oParam.StartBit / 8;
					int iBitInMapByte = oParam.StartBit - iMapByte * 8;
					
					int Rem = 0;
					Math.DivRem(oParam.Length, 8, out Rem);
					
					switch (oParam.Endianess)
					{
						case CanParameterEndianess.MSBFirst: //Most significant bytes first
														
							for (int iByte = 0; iByte < ParamBytes.Length; iByte++) //For each byte of the parameter: From the most (0) to the least (0...7) significant
							{
								int iBitStart = 7;
								if (iByte == 0 && Rem != 0) iBitStart = Rem - 1; //If parameter lenght isn't dividable by 8, fisrt byte isn't full (13 bits => Byte[0]: 5 bits / Byte[1]: 8 bits)
								
								for (int iBit = iBitStart; iBit >= 0; iBit--) //For each used bit of the current byte
								{
									int BitVal = (ParamBytes[iByte] & (1 << iBit)) >> iBit; //Get the value (0 or 1) of the current bit of the current parameter byte
									byte MapBitVal = (byte)(BitVal << iBitInMapByte); //Set the bit value retrieved into the message byte
									
									ByteMessageData[iMapByte] += MapBitVal; //Update message map byte
									
									iBitInMapByte--; //Next MSB parameter bit into the message map
									if (iBitInMapByte < 0)
									{
										iMapByte++;
										iBitInMapByte = 7;
									}
								}
							}
							
							break;
							
						case CanParameterEndianess.LSBFirst: //Least significant bytes first
							
							for (int iByte = ParamBytes.Length - 1; iByte >= 0; iByte--) //For each byte of the parameter: From the least (0...7) to the most (0) significant
							{
								int iBitEnd = 8;
								if (iByte == 0 && Rem != 0) iBitEnd = Rem; //If parameter lenght isn't dividable by 8, fisrt byte isn't full (13 bits => Byte[0]: 5 bits / Byte[1]: 8 bits)
								
								for (int iBit = 0; iBit < iBitEnd; iBit++) //For each used bit of the current byte
								{
									int BitVal = (ParamBytes[iByte] & (1 << iBit)) >> iBit; //Get the value (0 or 1) of the current bit of the current parameter byte
									byte MapBitVal = (byte)(BitVal << iBitInMapByte); //Set the bit value retrieved into the message byte
									
									ByteMessageData[iMapByte] += MapBitVal; //Update message map byte
									
									iBitInMapByte++; //Next LSB parameter bit into the message map
									if (iBitInMapByte > 7)
									{
										iMapByte++;
										iBitInMapByte = 0;
									}
								}	
							}
							
							break;
					}
				}
			}
		}

        public string GetMessageBytesString()
        {
            string BytesString = "";

            for (int iByte = 0; iByte < DLC; iByte++)
            {
                BytesString += string.Format("{0:X2}", ByteMessageData[iByte]);

                if (iByte < (DLC - 1))
                {
                    BytesString += " ";
                }
            }

            return (BytesString);
        }

		#endregion
		
		#region Private methodes
		
		private void InitMultiplexer()
		{
			MuxIndex=0;
			MuxTable=new List<long>();
			oMultiplexer = null;
			
			foreach(CANParameter oParam in Parameters)
			{
				if(oParam.IsMultiplexed)
				{
					//Update Mux values table
					if(!(IsMuxValueInTable(oParam.MultiplexerValue)))
					{
						MuxTable.Add(oParam.MultiplexerValue);
						
						if (oMultiplexer == null)
						{
							oMultiplexer = GetCANParameter(MultiplexerName, ParameterResearchOption.Name);
						}
					}
				}
			}
			
			MuxTable.Sort();
		}
		
		private bool IsMuxValueInTable(long MuxValue)
		{
			for(int i=0;i<MuxTable.Count;i++)
			{
				if(MuxTable[i]==MuxValue)
				{
					return(true);
				}
			}
			
			return(false);
		}
				
		private double ClipParameterEngineeringValue(CANParameter Parameter)
		{
            double[] ParamRange = Parameter.GetParameterMinMax();

            if (Parameter.DecodedValue < ParamRange[0])
            {
                return (ParamRange[0]);
            }
            else if (Parameter.DecodedValue > ParamRange[1])
            {
                return (ParamRange[1]);
            }

            return (Parameter.DecodedValue);
        }
		
		private byte[] GetParameterBytes(CANParameter Parameter)
		{
			int Rem = 0;
			int ByteLen = Math.DivRem(Parameter.Length, 8, out Rem);
			if (Rem != 0) ByteLen++;
			
			byte[] ParamBytes = new byte[ByteLen];
			
			//Min/Max value clipping
			double ClipEngValue = ClipParameterEngineeringValue(Parameter);
			
			//Engineering to raw value conversion
			UInt64 RawVal = 0;

            if (Parameter.Gain < 0)
            {
                ClipEngValue *= -1;
            }

			if (Parameter.Signed)
			{
                RawVal = Convert.ToUInt64((Math.Abs(ClipEngValue) - Parameter.Zero) / Math.Abs(Parameter.Gain));

                if (ClipEngValue < 0)
                {
                    RawVal |= ((UInt64)0x01 << (Parameter.Length - 1));
                }

            }
			else
			{
                RawVal = Convert.ToUInt64((ClipEngValue - Parameter.Zero) / Math.Abs(Parameter.Gain));
			}
			
			for (int iByte = 0; iByte < ByteLen; iByte++)
			{
				UInt64 MaskRawVal = (UInt64)0xFF << ((ByteLen - iByte - 1) * 8);
				UInt64 RawValMasked = RawVal & MaskRawVal;
				ParamBytes[iByte] = (byte)(RawValMasked >> ((ByteLen - iByte - 1) * 8));
			}
			
            if(Parameter.Endianess== CanParameterEndianess.LSBFirst)
            {
                RawVal = SwapBytes(RawVal, ByteLen);
            }

            Parameter.RawValue = "0x" + string.Format("{0:X" + (ByteLen * 2).ToString() + "}", RawVal);

			return(ParamBytes);
		}

		private long GetCurrentMultiplexerValue()
		{
			if (!(MultiplexerName.Equals("")))
			{
				foreach (CANParameter oParam in Parameters)
				{
					if (oParam.Name.Equals(MultiplexerName))
					{
						return((long)oParam.DecodedValue);
					}
				}
			}
			
			return(-1);
		}

		#endregion
	}
	
	#endregion
	
	#region Raw message
	
	public class CAN_RawMessageData
	{
		#region Public members
		
		public int KeyId;
		public UInt32 MessageId;
		public int DLC;
		public int Period;
		public bool Send;
		public byte[] MsgData;
        public long TxCount;
		
		#endregion
		
		public CAN_RawMessageData()
		{
			KeyId = -1;
			MessageId = 0;
			DLC = 8;
			Period = 0;
			Send = true;
			MsgData = new byte[DLC];
            TxCount = 0;
        }
		
		#region Public methode
		
		public TPCANMsg GetPCANMessage()
		{
			TPCANMsg CANMsg=new TPCANMsg();
			
			CANMsg.ID = MessageId;
			CANMsg.LEN = (byte)DLC;
			CANMsg.MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD;
			CANMsg.DATA = MsgData;
			
			return(CANMsg);
		}
		
		#endregion
	}
	
	public class CAN_RawMessageList
	{
		#region public members
		
		public List<CAN_RawMessageData> Messages;
		
		#endregion
		
		public CAN_RawMessageList()
		{
			Messages = new List<CAN_RawMessageData>();
		}
		
		#region Public methodes
		
		public void Save_RawMessagesList(string fPath)
		{
			if (!(fPath.Equals("")))
			{
				XmlDocument oXDoc = new XmlDocument();
				
				XmlElement xMessages = oXDoc.CreateElement("MessageList");
				oXDoc.AppendChild(xMessages);
				
				foreach (CAN_RawMessageData oMsg in Messages)
				{
					XmlElement xProp = null;
					
					XmlElement xMsg = oXDoc.CreateElement("RawMessage");
					xMessages.AppendChild(xMsg);
					
						xProp = oXDoc.CreateElement("ID");
						xProp.InnerText = oMsg.MessageId.ToString();
						xMsg.AppendChild(xProp);
						
						xProp = oXDoc.CreateElement("DLC");
						xProp.InnerText = oMsg.DLC.ToString();
						xMsg.AppendChild(xProp);
						
						xProp = oXDoc.CreateElement("Period");
						xProp.InnerText = oMsg.Period.ToString();
						xMsg.AppendChild(xProp);
						
						xProp = oXDoc.CreateElement("Send");
						xProp.InnerText = oMsg.Send.ToString();
						xMsg.AppendChild(xProp);
						
						xProp = oXDoc.CreateElement("Data");
						xMsg.AppendChild(xProp);
						
						for (int i=0; i<oMsg.DLC; i++)
						{
							XmlElement xByte = oXDoc.CreateElement("Byte");
							
								XmlAttribute xAtr = oXDoc.CreateAttribute("ByteNumber");
								xAtr.Value = i.ToString();
								xByte.Attributes.Append(xAtr);
							
							xByte.InnerText = oMsg.MsgData[i].ToString();
							xProp.AppendChild(xByte);
						}
				}
				
				oXDoc.Save(fPath);
			}
		}
		
		public bool Load_RawMessagesList(string fPath)
		{
			try
			{
				XmlDocument oXDoc = new XmlDocument();
				oXDoc.Load(fPath);
				
				Messages.Clear();
				
				XmlNode xMessages = oXDoc.SelectSingleNode("MessageList");
				
				foreach (XmlNode xMsg in xMessages.ChildNodes)
				{
					if (xMsg.Name.Equals("RawMessage"))
					{
						XmlNode xProp = null;
						
						CAN_RawMessageData oMsg = new CAN_RawMessageData();
							
							xProp = xMsg.SelectSingleNode("ID");
							oMsg.MessageId = UInt32.Parse(xProp.InnerText);
							
							xProp = xMsg.SelectSingleNode("DLC");
							oMsg.DLC = int.Parse(xProp.InnerText);
							
							xProp = xMsg.SelectSingleNode("Period");
							oMsg.Period = int.Parse(xProp.InnerText);
							
							xProp = xMsg.SelectSingleNode("Send");
							oMsg.Send = bool.Parse(xProp.InnerText);
							
							if (oMsg.DLC > 0)
							{
								oMsg.MsgData = new byte[oMsg.DLC];
								
								xProp = xMsg.SelectSingleNode("Data");
								
								foreach (XmlNode xByte in xProp.ChildNodes)
								{
									int iByte = int.Parse(xByte.Attributes["ByteNumber"].Value);
									oMsg.MsgData[iByte] = byte.Parse(xByte.InnerText);
								}
								
								Messages.Add(oMsg);
							}
					}
				}
			}
			catch
			{
				return(false);
			}
			
			return(true);
		}
		
		public CAN_RawMessageData Get_RawMessageAtKeyId(int KeyId)
		{
			foreach (CAN_RawMessageData oMsg in Messages)
			{
				if (oMsg.KeyId == KeyId)
				{
					return(oMsg);
				}
			}
			
			return(null);
		}
		
		public bool ContainsMessageId(UInt32 MsgId)
		{
			foreach (CAN_RawMessageData oMsg in Messages)
			{
				if (oMsg.MessageId == MsgId)
				{
					return(true);
				}
			}
			
			return(false);
		}
		
		#endregion
	}
	
	#endregion
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Vega64SoftPowerTableEditor
{

	using USHORT = UInt16;
	using UCHAR = Byte;
	using ULONG = UInt32;

	public class SoftPowerTable
	{		
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct ATOM_COMMON_TABLE_HEADER
		{
			Int16 usStructureSize;
			Byte ucTableFormatRevision;
			Byte ucTableContentRevision;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public unsafe struct ATOM_POWERPLAY_TABLE
		{
			public ATOM_COMMON_TABLE_HEADER sHeader;
			UCHAR ucTableRevision;
			USHORT usTableSize;                        /* the size of header structure */
			ULONG ulGoldenPPID;                       /* PPGen use only */
			ULONG ulGoldenRevision;                   /* PPGen use only */
			USHORT usFormatID;                         /* PPGen use only */
			ULONG ulPlatformCaps;                     /* See ATOM_Vega10_CAPS_* */
			ULONG ulMaxODEngineClock;                 /* For Overdrive. */
			ULONG ulMaxODMemoryClock;                 /* For Overdrive. */
			USHORT usPowerControlLimit;
			USHORT usUlvVoltageOffset;                 /* in mv units */
			USHORT usUlvSmnclkDid;
			USHORT usUlvMp1clkDid;
			USHORT usUlvGfxclkBypass;
			USHORT usGfxclkSlewRate;
			UCHAR ucGfxVoltageMode;
			UCHAR ucSocVoltageMode;
			UCHAR ucUclkVoltageMode;
			UCHAR ucUvdVoltageMode;
			UCHAR ucVceVoltageMode;
			UCHAR ucMp0VoltageMode;
			UCHAR ucDcefVoltageMode;
			public USHORT usStateArrayOffset;                 /* points to ATOM_Vega10_State_Array */
			public USHORT usFanTableOffset;                   /* points to ATOM_Vega10_Fan_Table */
			USHORT usThermalControllerOffset;          /* points to ATOM_Vega10_Thermal_Controller */
			USHORT usSocclkDependencyTableOffset;      /* points to ATOM_Vega10_SOCCLK_Dependency_Table */
			public USHORT usMclkDependencyTableOffset;        /* points to ATOM_Vega10_MCLK_Dependency_Table */
			public USHORT usGfxclkDependencyTableOffset;      /* points to ATOM_Vega10_GFXCLK_Dependency_Table */
			USHORT usDcefclkDependencyTableOffset;     /* points to ATOM_Vega10_DCEFCLK_Dependency_Table */
			public USHORT usVddcLookupTableOffset;            /* points to ATOM_Vega10_Voltage_Lookup_Table */
			public USHORT usVddmemLookupTableOffset;          /* points to ATOM_Vega10_Voltage_Lookup_Table */
			USHORT usMMDependencyTableOffset;          /* points to ATOM_Vega10_MM_Dependency_Table */
			USHORT usVCEStateTableOffset;              /* points to ATOM_Vega10_VCE_State_Table */
			USHORT usReserve;                          /* No PPM Support for Vega10 */
			USHORT usPowerTuneTableOffset;             /* points to ATOM_Vega10_PowerTune_Table */
			USHORT usHardLimitTableOffset;             /* points to ATOM_Vega10_Hard_Limit_Table */
			USHORT usVddciLookupTableOffset;           /* points to ATOM_Vega10_Voltage_Lookup_Table */
			USHORT usPCIETableOffset;                  /* points to ATOM_Vega10_PCIE_Table */
			USHORT usPixclkDependencyTableOffset;      /* points to ATOM_Vega10_PIXCLK_Dependency_Table */
			USHORT usDispClkDependencyTableOffset;     /* points to ATOM_Vega10_DISPCLK_Dependency_Table */
			USHORT usPhyClkDependencyTableOffset;      /* points to ATOM_Vega10_PHYCLK_Dependency_Table */
		};

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public unsafe struct ATOM_Vega10_State
		{
			UCHAR ucSocClockIndexHigh;
			UCHAR ucSocClockIndexLow;
			UCHAR ucGfxClockIndexHigh;
			UCHAR ucGfxClockIndexLow;
			UCHAR ucMemClockIndexHigh;
			UCHAR ucMemClockIndexLow;
			USHORT usClassification;
			ULONG ulCapsAndSettings;
			USHORT usClassification2;
		};

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public unsafe struct ATOM_Vega10_State_Array
		{
			public UCHAR ucRevId;
			public UCHAR ucNumEntries;                                         /* Number of entries. */
		};

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public unsafe struct ATOM_Vega10_GFXCLK_Dependency_Table
		{
			public UCHAR ucRevId;
			public UCHAR ucNumEntries;                                         /* Number of entries. */
		};

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public unsafe struct ATOM_Vega10_GFXCLK_Dependency_Record
		{
			public ULONG ulClk;                                               /* Clock Frequency */
			public UCHAR ucVddInd;                                            /* SOC_VDD index */
			public USHORT usCKSVOffsetandDisable;                              /* Bits 0~30: Voltage offset for CKS, Bit 31: Disable/enable for the GFXCLK level. */
			public USHORT usAVFSOffset;                                        /* AVFS Voltage offset */
			public UCHAR ucACGEnable;
			public fixed UCHAR ucReserved[3];
		};

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public unsafe struct ATOM_Vega10_MCLK_Dependency_Table
		{
		public UCHAR ucRevId;
		public UCHAR ucNumEntries;                                         /* Number of entries. */
		};

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public unsafe struct ATOM_Vega10_MCLK_Dependency_Record
		{
		public ULONG ulMemClk;                                            /* Clock Frequency */
		public UCHAR ucVddInd;                                            /* SOC_VDD index */
		public UCHAR ucVddMemInd;                                         /* MEM_VDD - only non zero for MCLK record */
		public UCHAR ucVddciInd;                                          /* VDDCI   = only non zero for MCLK record */
		};

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public unsafe struct ATOM_Vega10_Voltage_Lookup_Record
		{
			public USHORT usVdd;                                               /* Base voltage */
		};

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public unsafe struct ATOM_Vega10_Voltage_Lookup_Table
		{
			public UCHAR ucRevId;
			public UCHAR ucNumEntries;                                          /* Number of entries */
		};

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public unsafe struct ATOM_Vega10_Fan_Table
		{
			public UCHAR ucRevId;                         /* Change this if the table format changes or version changes so that the other fields are not the same. */
			public USHORT usFanOutputSensitivity;          /* Sensitivity of fan reaction to temepature changes. */
			public USHORT usFanRPMMax;                     /* The default value in RPM. */
			public USHORT usThrottlingRPM;
			public USHORT usFanAcousticLimit;              /* Minimum Fan Controller Frequency Acoustic Limit. */
			public USHORT usTargetTemperature;             /* The default ideal temperature in Celcius. */
			public USHORT usMinimumPWMLimit;               /* The minimum PWM that the advanced fan controller can set. */
			public USHORT usTargetGfxClk;                   /* The ideal Fan Controller GFXCLK Frequency Acoustic Limit. */
			public USHORT usFanGainEdge;
			public USHORT usFanGainHotspot;
			public USHORT usFanGainLiquid;
			public USHORT usFanGainVrVddc;
			public USHORT usFanGainVrMvdd;
			public USHORT usFanGainPlx;
			public USHORT usFanGainHbm;
			public UCHAR ucEnableZeroRPM;
			public USHORT usFanStopTemperature;
			public USHORT usFanStartTemperature;
		};


		public ATOM_POWERPLAY_TABLE atom_powerplay_table;

		public ATOM_Vega10_State_Array atom_vega10_state_array;

		public ATOM_Vega10_GFXCLK_Dependency_Table atom_vega10_gfxclk_table;
		public List<ATOM_Vega10_GFXCLK_Dependency_Record> atom_vega10_gfxclk_entries = new List<ATOM_Vega10_GFXCLK_Dependency_Record>();

		public ATOM_Vega10_MCLK_Dependency_Table atom_vega10_memclk_table;
		public List<ATOM_Vega10_MCLK_Dependency_Record> atom_vega10_memclk_entries = new List<ATOM_Vega10_MCLK_Dependency_Record>();

		public ATOM_Vega10_Voltage_Lookup_Table atom_vega10_gfxvdd_table;
		public List<ATOM_Vega10_Voltage_Lookup_Record> atom_vega10_gfxvdd_record = new List<ATOM_Vega10_Voltage_Lookup_Record>();

		public ATOM_Vega10_Voltage_Lookup_Table atom_vega10_memvdd_table;
		public List<ATOM_Vega10_Voltage_Lookup_Record> atom_vega10_memvdd_record = new List<ATOM_Vega10_Voltage_Lookup_Record>();

		public ATOM_Vega10_Fan_Table atom_vega10_fan_table;

		private static String STR_WIN_VER = "Windows Registry Editor Version";
		private Regex REGEX_WIN_VER = new Regex("Windows Registry Editor Version (.*)", RegexOptions.IgnorePatternWhitespace);

		private static String STR_PHM_PPT = "PP_PhmSoftPowerPlayTable";
		private static String STR_HEX_START = "=hex:";

		public String windowsRegistryVersion = null;
		public List<int> hexBlobNewLineIndices = new List<int>();

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Vega64SoftPowerTableEditor.SoftPowerTable"/> class.
		/// </summary>
		public SoftPowerTable()
		{
		}

		/// <summary>
		/// Opens the reg file.
		/// </summary>
		/// <returns>SoftPowerTable</returns>
		public static SoftPowerTable openRegFile()
		{
			SoftPowerTable spt = new SoftPowerTable();

			// Parse out the hex data
			StreamReader reader = File.OpenText("RX_VEGA_64_Soft_PP.reg");
			string line;
			String data = null;
			String hexData = null;
			while ((line = reader.ReadLine()) != null)
			{
				// Grab the Win ver just in case, probably the same for everyone.
				if (line.Contains(STR_WIN_VER))
				{
					spt.windowsRegistryVersion = line;
					Console.WriteLine(line);
				}

				// We found the spt definition
				if (line.Contains(STR_PHM_PPT))
				{
					data = line + '\n';
					data += reader.ReadToEnd();
				}
			}

			// parse the data, saving new lines for later reconstruction and grab hex blob
			int start = data.IndexOf(STR_HEX_START, StringComparison.Ordinal) + STR_HEX_START.Length;
			while (start <= data.Length)
			{
				int newlineIndex = data.IndexOf('\n', start);
				if (newlineIndex == -1)
				{
					newlineIndex = data.Length;
				}
				hexData += data.Substring(start, newlineIndex-start);

				spt.hexBlobNewLineIndices.Add(newlineIndex);

				start = newlineIndex + 1;
			}

			// clean up the hex data
			hexData = Regex.Replace(hexData, @"\t|\n|\r|\s+|\\|,", "");

			byte[] byteArray = StringToByteArray(hexData);

			// parse main table
			spt.atom_powerplay_table = fromBytes<ATOM_POWERPLAY_TABLE>(byteArray);

			// parse state array
			int offset = spt.atom_powerplay_table.usStateArrayOffset;
			spt.atom_vega10_state_array = fromBytes<ATOM_Vega10_State_Array>(byteArray.Skip(offset).ToArray());

			// parse gfx clk states (hurray!)
			offset = spt.atom_powerplay_table.usGfxclkDependencyTableOffset;
			spt.atom_vega10_gfxclk_table = fromBytes<ATOM_Vega10_GFXCLK_Dependency_Table>(byteArray.Skip(offset).ToArray());
			offset += Marshal.SizeOf(spt.atom_vega10_gfxclk_table);
			spt.atom_vega10_gfxclk_entries = arrFromBytes<ATOM_Vega10_GFXCLK_Dependency_Record>(byteArray, offset, spt.atom_vega10_gfxclk_table.ucNumEntries);

			// parse mem clk states
			offset = spt.atom_powerplay_table.usMclkDependencyTableOffset;
			spt.atom_vega10_memclk_table = fromBytes<ATOM_Vega10_MCLK_Dependency_Table>(byteArray.Skip(offset).ToArray());
			offset += Marshal.SizeOf(spt.atom_vega10_memclk_table);
			spt.atom_vega10_memclk_entries = arrFromBytes<ATOM_Vega10_MCLK_Dependency_Record>(byteArray, offset, spt.atom_vega10_memclk_table.ucNumEntries);

			// parse gfx voltages
			offset = spt.atom_powerplay_table.usVddcLookupTableOffset;
			spt.atom_vega10_gfxvdd_table = fromBytes<ATOM_Vega10_Voltage_Lookup_Table>(byteArray.Skip(offset).ToArray());
			offset += Marshal.SizeOf(spt.atom_vega10_gfxvdd_table);
			spt.atom_vega10_gfxvdd_record = arrFromBytes<ATOM_Vega10_Voltage_Lookup_Record>(byteArray, offset, spt.atom_vega10_gfxvdd_table.ucNumEntries);

			// parse mem voltages
			offset = spt.atom_powerplay_table.usVddmemLookupTableOffset;
			spt.atom_vega10_memvdd_table = fromBytes<ATOM_Vega10_Voltage_Lookup_Table>(byteArray.Skip(offset).ToArray());
			offset += Marshal.SizeOf(spt.atom_vega10_gfxvdd_table);
			spt.atom_vega10_memvdd_record = arrFromBytes<ATOM_Vega10_Voltage_Lookup_Record>(byteArray, offset, spt.atom_vega10_memvdd_table.ucNumEntries);

			// parse fan table
			spt.atom_vega10_fan_table = fromBytes<ATOM_Vega10_Fan_Table>(byteArray.Skip(spt.atom_powerplay_table.usFanTableOffset).ToArray());

			// debug
			Console.WriteLine(data);
			Console.WriteLine(hexData);

			return spt;
		}

		public void saveRegFile()
		{
			byte[] bytes = getBytes<ATOM_POWERPLAY_TABLE>(this.atom_powerplay_table);
			string hex = BitConverter.ToString(bytes).Replace("-", String.Empty);
			Console.WriteLine(hex);
		}

		static byte[] getBytes<T>(T obj)
		{
			int size = Marshal.SizeOf(obj);
			byte[] arr = new byte[size];

			IntPtr ptr = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(obj, ptr, true);
			Marshal.Copy(ptr, arr, 0, size);
			Marshal.FreeHGlobal(ptr);
			return arr;
		}

		/// <summary>
		/// Marshal the data from byte array
		/// </summary>
		/// <returns>The bytes.</returns>
		/// <param name="arr">Arr.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		static T fromBytes<T>(byte[] arr)
		{
			T obj = default(T);
			int size = Marshal.SizeOf(obj);
			IntPtr ptr = Marshal.AllocHGlobal(size);
			Marshal.Copy(arr, 0, ptr, size);
			obj = (T)Marshal.PtrToStructure(ptr, obj.GetType());
			Marshal.FreeHGlobal(ptr);

			return obj;
		}

		static List<T> arrFromBytes<T>(byte[] arr, int offset, int entries)
		{
			List<T> retVal = new List<T>();
			for (int i = 0; i< entries; i++)
			{				
				T record = fromBytes<T>(arr.Skip(offset).ToArray());
				retVal.Add(record);
				offset += Marshal.SizeOf(record);
			}

			return retVal;
		}

		/// <summary>
		/// Convert a string blob into bytes
		/// </summary>
		/// <returns>The to byte array.</returns>
		/// <param name="hex">Hex.</param>
		public static byte[] StringToByteArray(string hex)
		{
			return Enumerable.Range(0, hex.Length)
							 .Where(x => x % 2 == 0)
							 .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
							 .ToArray();
		}
	}
}

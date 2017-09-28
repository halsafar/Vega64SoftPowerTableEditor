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
			USHORT usStateArrayOffset;                 /* points to ATOM_Vega10_State_Array */
			USHORT usFanTableOffset;                   /* points to ATOM_Vega10_Fan_Table */
			USHORT usThermalControllerOffset;          /* points to ATOM_Vega10_Thermal_Controller */
			USHORT usSocclkDependencyTableOffset;      /* points to ATOM_Vega10_SOCCLK_Dependency_Table */
			USHORT usMclkDependencyTableOffset;        /* points to ATOM_Vega10_MCLK_Dependency_Table */
			USHORT usGfxclkDependencyTableOffset;      /* points to ATOM_Vega10_GFXCLK_Dependency_Table */
			USHORT usDcefclkDependencyTableOffset;     /* points to ATOM_Vega10_DCEFCLK_Dependency_Table */
			USHORT usVddcLookupTableOffset;            /* points to ATOM_Vega10_Voltage_Lookup_Table */
			USHORT usVddmemLookupTableOffset;          /* points to ATOM_Vega10_Voltage_Lookup_Table */
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

		private static String STR_WIN_VER = "Windows Registry Editor Version";
		private Regex REGEX_WIN_VER = new Regex("Windows Registry Editor Version (.*)", RegexOptions.IgnorePatternWhitespace);

		private static String STR_PHM_PPT = "PP_PhmSoftPowerPlayTable";
		private static String STR_HEX_START = "=hex:";

		public String windowsRegistryVersion = null;
		public List<int> hexBlobNewLineIndices = new List<int>();

		public ATOM_POWERPLAY_TABLE atom_powerplay_table;

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

			// parse into structure
			spt.atom_powerplay_table = fromBytes<ATOM_POWERPLAY_TABLE>(byteArray);

			// debug
			Console.WriteLine(data);
			Console.WriteLine(hexData);

			return spt;
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

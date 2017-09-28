using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Vega64SoftPowerTableEditor
{
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
			public Byte ucTableRevision;
			public UInt16 usTableSize;
			public UInt32 ulGoldenPPID;
			public UInt32 ulGoldenRevision;
			public UInt16 usFormatID;
			public UInt32 ulPlatformCaps;
			public UInt32 ulMaxODEngineClock;
			public UInt32 ulMaxODMemoryClock;
			public UInt16 usPowerControlLimit;
			public UInt16 usUlvVoltageOffset;
			public UInt16 usStateArrayOffset;
			public UInt16 usFanTableOffset;
			public UInt16 usThermalControllerOffset;
			public UInt16 usReserv;
			public UInt16 usMclkDependencyTableOffset;
			public UInt16 usSclkDependencyTableOffset;
			public UInt16 usVddcLookupTableOffset;
			public UInt16 usVddgfxLookupTableOffset;
			public UInt16 usMMDependencyTableOffset;
			public UInt16 usVCEStateTableOffset;
			public UInt16 usPPMTableOffset;
			public UInt16 usPowerTuneTableOffset;
			public UInt16 usHardLimitTableOffset;
			public UInt16 usPCIETableOffset;
			public UInt16 usGPIOTableOffset;
			public fixed UInt16 usReserved[6];  };

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

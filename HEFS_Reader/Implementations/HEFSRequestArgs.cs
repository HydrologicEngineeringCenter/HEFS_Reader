using System;

namespace HEFS_Reader.Implementations
{
	public class HEFSRequestArgs
	{
		public DateTime ForecastDate { get; set; }
		public Enumerations.Watersheds WatershedLocation { get; set; }
    public string Path { get; set; }
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SPFDataDump
{
    public enum NoteFace
    {
        FUFF = 1,    // Face up, face first
        FUFL,       // Face up, face last
        FDFF,       // Face down, face first
        FDFL        // Face down, face last
    }

    class Program
    {
        static CMySql m_MySql;
        public const byte SSP_STX = 0x7F;
        public const byte SENSOR_DATA_LENGTH = 50;
        public const string RAW_DATA_PREFIX = "Raw_";
        public const string CORRECTED_DATA_PREFIX = "Corrected_";
        public const string FILE_EXTENSION = ".csv";


        static void Main(string[] args)
        {
            string Database = args[0];
            string Table = args[1];
            int Face = int.Parse(args[2]);

            m_MySql = new CMySql(Database, false);

            List<int> IDs = m_MySql.GetDataIDs(Table, Face);

            string FaceString = ((NoteFace)Face).ToString();

            StringBuilder SbDataFile = new StringBuilder(30);
            string DataFile;

            SbDataFile.Append(RAW_DATA_PREFIX);
            SbDataFile.Append(Database);
            SbDataFile.Append(".");
            SbDataFile.Append(Table);
            SbDataFile.Append("_");
            SbDataFile.Append(FaceString);
            SbDataFile.Append(FILE_EXTENSION);
            DataFile = SbDataFile.ToString();

            if (File.Exists(DataFile))
            {
                File.Delete(DataFile);
            }

            foreach (int ID in IDs)
            {
                Console.WriteLine("Processing data for ID " + ID.ToString());
                byte[] BlobData = m_MySql.GetSensorDataBlob(Table, ID);
                int BlobIndex = 0;
                while (BlobIndex < BlobData.Length)
                {
                    List<byte> SensorData = new List<byte>();
                    byte[] SensorDataBytes = new byte[SENSOR_DATA_LENGTH];
                    Array.Copy(BlobData, BlobIndex, SensorDataBytes, 0, SENSOR_DATA_LENGTH);
                    SensorData.AddRange(SensorDataBytes);
                    BlobIndex += SENSOR_DATA_LENGTH;
                }

                string CsvFileContents = ByteArrayToCsvPadded(BlobData);
                File.AppendAllText(DataFile, CsvFileContents + Environment.NewLine);
            }
        }

        public static string ByteArrayToCsvPadded(byte[] byteArray)
        {
            StringBuilder sbCsv = new StringBuilder(byteArray.Length * 5);
            sbCsv.Append(byteArray[0].ToString("000"));   // First element not preceded by ', ' 
            for (int i = 1; i < byteArray.Length; i++)
            {
                sbCsv.Append(", ");
                sbCsv.Append(byteArray[i].ToString("000"));

            }
            return sbCsv.ToString();

        }

        public static string ByteArrayToCsv(byte[] byteArray)
        {
            StringBuilder sbCsv = new StringBuilder(byteArray.Length * 5);
            sbCsv.Append(byteArray[0].ToString());   // First element not preceded by ', ' 
            for (int i = 1; i < byteArray.Length; i++)
            {
                sbCsv.Append(", ");
                sbCsv.Append(byteArray[i].ToString());

            }
            return sbCsv.ToString();

        }
    }
}

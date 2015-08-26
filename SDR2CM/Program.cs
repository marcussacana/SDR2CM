using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SDR2CM
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == new string[0])
                Console.Write("Super Danganronpa 2 Content Manager\nThis tool has created by marcus and is a open source project.\n\nUsage:\nSDR2CM.exe In_file_to_extract\nOr:\nSDR2CM.exe In_Folder_to_Repack");
            else
            {
                foreach (string arg in args)
                    if (System.IO.File.Exists(arg))
                    {
                        try
                        {
                            ExtractPak(Tools.ByteArrayToString(System.IO.File.ReadAllBytes(arg)).Split('-'), System.IO.Path.GetDirectoryName(arg) + "\\" + System.IO.Path.GetFileName(arg) + "-out\\");
                            Console.Write("\n" + System.IO.Path.GetFileName(arg) + " Extracted...\n");
                        }
                        catch { Console.Write("\nInvalid File Format"); }
                    }
                    else if (System.IO.Directory.Exists(arg))
                    {
                        string folder = arg;
                        if (arg.EndsWith("\\"))
                            folder = arg.Substring(0, arg.Length-1);
                        RepackPak(folder, System.IO.Path.GetDirectoryName(folder)+ "\\" +System.IO.Path.GetFileName(folder).Replace("-out", ""));
                    }
                    else
                        Console.Write("Invalid Folder or File, Make sure you write full file/folder path.");
            }
        }

        private static void RepackPak(string Directory, string PakPath)
        {
            string ExtraHeaderContent = "";
            bool ExtraHeader = false;
            int count = -1;
        again:;
            count++;
            string[] files = System.IO.Directory.GetFiles(Directory).OrderBy(f => f).ToArray();
            string[] FOrder = new string[0];
            for (int FILE = 0; files.Length > FOrder.Length; FILE++)
            {
                if (count == 0 && System.IO.File.Exists((Directory + "\\" + "HeaderContent.SDR2CM")))
                    break;
                if (FILE >= files.Length)
                    FILE = 0;
                if (System.IO.Path.GetFileNameWithoutExtension(files[FILE]) == FOrder.Length.ToString())
                {
                    string[] temp = new string[FOrder.Length + 1];
                    FOrder.CopyTo(temp, 0);
                    temp[FOrder.Length] = files[FILE];
                    FOrder = temp;
                }
            }
            string[] PakHexs = new string[0];
            Packget = new File[files.Length];
            for (int i = 0; i < Packget.Length; i++)
                if (!(System.IO.Path.GetFileName(files[i]) == "HeaderContent.SDR2CM"))
                    Packget[i] = new File();
                else
                {
                    ExtraHeaderContent = System.IO.File.ReadAllText(files[i]);
                    System.IO.File.Delete(files[i]);
                    ExtraHeader = true;
                    goto again;
                }
            files = FOrder;
            if (ExtraHeader)
                System.IO.File.WriteAllText(Directory + "\\" + "HeaderContent.SDR2CM", ExtraHeaderContent);
            string Header = Tools.IntToHex(Packget.Length) + " ";
            for (int i = 0; i < Packget.Length; i++)
            {
                PakHexs = AppendFile(PakHexs, Tools.ByteArrayToString(System.IO.File.ReadAllBytes(files[i])).Split('-'), i);
                Header += Tools.IntToHex(0) + " ";
            }
            Header += ExtraHeaderContent;
            int HeaderSize = (Header.Replace(@" ", "").Length/2);

            Header = Tools.IntToHex(Packget.Length) + " ";
            for (int i = 0; i < Packget.Length; i++)
            {
                Header += Tools.IntToHex(Packget[i].StartPos + HeaderSize) + " ";
            }
                Header += ExtraHeaderContent;
        confirm:;
            if (Header.EndsWith(@" "))
            {
                Header = Header.Substring(0, Header.Length - 1);
                goto confirm;
            }
            string[] OutFile = new string[(Header.Replace(@" ", "").Length / 2) + PakHexs.Length];
            string[] PakHeader = Header.Split(' ');
            PakHeader.CopyTo(OutFile, 0);
            PakHexs.CopyTo(OutFile, PakHeader.Length);
            System.IO.File.WriteAllBytes(PakPath,Tools.StringToByteArray(OutFile));
        }

        private static string[] AppendFile(string[] Pak, string[] FileToWrite, int id)
        {
            string[] temp = new string[Pak.Length + FileToWrite.Length];
            Pak.CopyTo(temp, 0);
            Packget[id].StartPos = Pak.Length;
            FileToWrite.CopyTo(temp, Pak.Length);
            return temp;
        }
        

        public static File[] Packget;
        private static void ExtractPak(string[] pak, string OutPath)
        {
            int TotalFiles = GetOffset(pak, 0);
            Packget = new File[TotalFiles];
            Console.Write("Total files: " + TotalFiles);
            for (int Offset = 4; (Offset / 4) <= TotalFiles; Offset += 4)
            {
                int id = (Offset / 4) - 1;
                Packget[id] = new File();
                Packget[id].StartPos = GetOffset(pak, Offset);
                if (((Offset + 1) / 4) < TotalFiles) //If is the last file of the packget, go to else and...
                    Packget[id].EndPos = (GetOffset(pak, (Offset + 4)) - 1);
                else
                    Packget[id].EndPos = (pak.Length - 1); //...Set the last file end position to end of the packget
                Console.Write("\nFile: " + id + ", Start at: " + Packget[id].StartPos + " and ends at: " + Packget[id].EndPos);
            }
            Console.Write("\nFile Offset Tree generated, Getting Extra Content...");
            string ExtraHeaderContent = "";
            if (Packget == new File[0])
                return;
            else
                if (Packget[0] == null)
                return;
            for (int Offset = (((TotalFiles+1) * 4)); Offset < Packget[0].StartPos; Offset++)
                ExtraHeaderContent += pak[Offset] + " ";
            System.IO.Directory.CreateDirectory(OutPath);
            if (ExtraHeaderContent != "")
                System.IO.File.WriteAllText(OutPath + "HeaderContent.SDR2CM", ExtraHeaderContent.Substring(0, ExtraHeaderContent.Length-1));
            Console.Write("\nPackget Header generated, Identifying files formats...");
            for (int id = 0; id < TotalFiles; id++)
            {
                Packget[id].Extension = GetFormat(pak, Packget[id].StartPos);
            }
            Console.Write("\nAll formats identified... Starting Extraction");
            for (int id = 0; id < TotalFiles; id++)
            {
                string[] file = GetContent(Packget[id], pak);
                string OutFilePath = OutPath + id;
                #region AppendExtension
                switch (Packget[id].Extension)
                {               
                    case File.Extensions.at3:
                        OutFilePath += ".at3";
                        break;
                    case File.Extensions.awb:
                        OutFilePath += ".awb";
                        break;
                    case File.Extensions.bmp:
                        OutFilePath += ".bmp";
                        break;
                    case File.Extensions.dat:
                        OutFilePath += ".dat";
                        break;
                    case File.Extensions.font:
                        OutFilePath += ".font";
                        break;
                    case File.Extensions.gim:
                        OutFilePath += ".gim";
                        break;
                    case File.Extensions.gmo:
                        OutFilePath += ".gmo";
                        break;
                    case File.Extensions.p3d:
                        OutFilePath += ".p3d";
                        break;
                    case File.Extensions.png:
                        OutFilePath += ".png";
                        break;
                    case File.Extensions.sfl:
                        OutFilePath += ".sfl";
                        break;
                    case File.Extensions.vag:
                        OutFilePath += ".vag";
                        break;

                }
                #endregion
                Console.Write("\nExtracting: " + System.IO.Path.GetFileName(OutFilePath));
                byte[] bin = Tools.StringToByteArray(file);
                System.IO.File.WriteAllBytes(OutFilePath, bin);
            }
        }

        private static string[] GetContent(File file, string[] pak)
        {
            string[] content = new string[file.EndPos - file.StartPos+1];
            for (int index = file.StartPos; index < (file.EndPos+1); index++)
            {
                content[index - file.StartPos] = pak[index];
            }
            return content;
        }

        private static File.Extensions GetFormat(string[] pak, int startPos)
        {
            object[] Headers = new object[] { new object[] { "MIG.00.1PSP", File.Extensions.gim},
                new object[] {"LLFS", File.Extensions.sfl}, new object[] {"RIFF", File.Extensions.at3},
                new object[] { "OMG.00.1PSP", File.Extensions.gmo}, new object[] {"0x89504E47", File.Extensions.png},
                new object[] {"BM", File.Extensions.bmp}, new object[] { "VAGp", File.Extensions.vag},
                new object[] { "tFpS", File.Extensions.font}, new object[] {"0x41465332", File.Extensions.awb},
                /*new object[] {"0x7000",  File.Extensions.scp.wrd},*/ new object[] { "0xF0306090020000000C000000", File.Extensions.p3d} };
            foreach (object Header in Headers)
            {
                string Signature = (string)((object[])Header)[0];
                File.Extensions Extension = (File.Extensions)((object[])Header)[1];

                bool Hex = Signature.StartsWith("0x");
                string Content = "";
                if (Hex) Content = "0x";
                if (!Hex)
                    for (int pos = 0; pos < Signature.Length; pos++)
                    {
                        Content += Tools.HexToString(pak[startPos + pos]);
                    }
                else
                    for (int pos = 0; pos < (Signature.Length/2); pos++)
                    {
                        Content += pak[startPos + pos];
                    }
                if (Content == Signature)
                    return Extension;
            }
            return File.Extensions.dat;

        }

        public static int GetOffset(string[] pak, int Position)
        {
            return Tools.HexToInt(pak[Position + 3] + pak[Position + 2] + pak[Position + 1] + pak[Position]);
        }
        
    }
    public class File {
        public int StartPos = -1;
        public int EndPos = -1;
        public Extensions Extension = Extensions.dat;
        public enum Extensions
        {
            gim, sfl, at3, gmo, png, bmp, vag, font, awb, /*.scp.wrd,*/ p3d, dat
        }
    }
    public class Tools
    {
        public static string IntToHex(int val)
        {
            string hexValue = val.ToString("X");
            if (hexValue.Length > 2)
            {
                if (hexValue.Length.ToString().EndsWith("1") || hexValue.Length.ToString().EndsWith("3") || hexValue.Length.ToString().EndsWith("5") || hexValue.Length.ToString().EndsWith("7") || hexValue.Length.ToString().EndsWith("9"))
                { hexValue = "0" + hexValue; }
                string NHEX = "";
                for (int index = hexValue.Length; index != 0; index -= 2)
                {
                    NHEX += hexValue.Substring(index - 2, 2) + " ";
                }
                NHEX = NHEX.Substring(0, NHEX.Length - 1);
                switch (NHEX.Replace(@" ", "").Length)
                {
                    case 2:
                        return NHEX + " 00 00 00";
                    case 4:
                        return NHEX + " 00 00";
                    case 6:
                        return NHEX + " 00";
                    case 8:
                        return NHEX;
                }
                return "null";
            }
            else
            {
                if (hexValue.Length == 1)
                    return "0" + hexValue + " 00 00 00";
                return hexValue + " 00 00 00";
            }
        }

        public static string StringToHex(string _in)
        {
            string input = _in;
            char[] values = input.ToCharArray();
            string r = "";
            foreach (char letter in values)
            {
                int value = Convert.ToInt32(letter);
                string hexOutput = String.Format("{0:X}", value);
                if (value > 255)
                    return UnicodeStringToHex(input);
                r += value + " ";
            }
            string[] bytes = r.Split(' ');
            byte[] b = new byte[bytes.Length - 1];
            int index = 0;
            foreach (string val in bytes)
            {
                if (index == bytes.Length - 1)
                    break;
                if (int.Parse(val) > byte.MaxValue)
                {b[index] = byte.Parse("0");
                }
                else
                    b[index] = byte.Parse(val);
                index++;
            }
            r = ByteArrayToString(b);
            return r.Replace("-", @" ");
        }
        public static string UnicodeStringToHex(string _in)
        {
            string input = _in;
            char[] values = Encoding.Unicode.GetChars(Encoding.Unicode.GetBytes(input.ToCharArray()));
            string r = "";
            foreach (char letter in values)
            {
                int value = Convert.ToInt32(letter);
                string hexOutput = String.Format("{0:X}", value);
                r += value + " ";
            }
            UnicodeEncoding unicode = new UnicodeEncoding();
            byte[] b = unicode.GetBytes(input);
            r = ByteArrayToString(b);
            return r.Replace("-", @" ");
        }
        public static byte[] StringToByteArray(string hex)
        {
            try
            {
                int NumberChars = hex.Length;
                byte[] bytes = new byte[NumberChars / 2];
                for (int i = 0; i < NumberChars; i += 2)
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                return bytes;
            }
            catch { Console.Write("Invalid format file!"); return new byte[0]; }
        }
        public static byte[] StringToByteArray(string[] hex)
        {
            try
            {
                int NumberChars = hex.Length;
                byte[] bytes = new byte[NumberChars];
                for (int i = 0; i < NumberChars; i++)
                    bytes[i] = Convert.ToByte(hex[i], 16);
                return bytes;
            }
            catch { Console.Write("Invalid format file!"); return new byte[0]; }
        }
        public static string ByteArrayToString(byte[] ba)
        {
            string hex = BitConverter.ToString(ba);
            return hex;
        }

        public static int HexToInt(string hex)
        {
            int num = Int32.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            return num;
        }

        public static string HexToString(string hex)
        {
            string[] hexValuesSplit = hex.Split(' ');
            string returnvar = "";
            foreach (string hexs in hexValuesSplit)
            {
                int value = Convert.ToInt32(hexs, 16);
                char charValue = (char)value;
                returnvar += charValue;
            }
            return returnvar;
        }

        public static string UnicodeHexToUnicodeString(string hex)
        {
            string hexString = hex.Replace(@" ", "");
            int length = hexString.Length;
            byte[] bytes = new byte[length / 2];

            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            return Encoding.Unicode.GetString(bytes);
        }

    }
}

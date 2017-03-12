using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace EasyDBFParser
{
    //http://www.dbase.com/Knowledgebase/INT/db7_file_fmt.htm
    public class DBFParser
    {
        public enum NativeDbType : byte
        {
            Autoincrement = 0x2B, //+ in ASCII
            Timestamp = 0x40, //@ in ASCII
            Binary = 0x42, //B in ASCII
            Char = 0x43, //C in ASCII
            Date = 0x44, //D in ASCII
            Float = 0x46, //F in ASCII
            Ole = 0x47, //G in ASCII
            Long = 0x49, //I in ASCII
            Logical = 0x4C, //L in ASCII
            Memo = 0x4D, //M in ASCII
            Numeric = 0x4E, //N in ASCII
            Double = 0x4F, //O in ASCII
        }

        public struct Field
        {
            public string Name { get; internal set; }
            public NativeDbType Type { get; internal set; }
            public byte Length { get; internal set; }
            public byte DecimalCount { get; internal set; }
        }

        public int RecordCount { get; private set; }

        public int RecordSize { get; private set; }

        public List<Field> Fields { get; private set; }
        public List<object[]> Datas { get; private set; }

        public DataTable DataTable { get; private set; }

        public void Parse(string filename)
        {
            Fields = null;
            Datas = null;

            using (BinaryReader reader = new BinaryReader(File.OpenRead(filename)))
            {
                // 0
                byte validDbase = reader.ReadByte(); // bit 0->2: version number  3 for dBASE 5 and 4 for dBASE 7
                int versionNumber = validDbase & 0x07;
                //
                if (versionNumber == 4)
                    ParseV7(reader);
                else
                    throw new Exception("Unsupported DBF file version");
            }
        }

        // TODO: read whole file and parse afterwards
        private void ParseV7(BinaryReader reader)
        {
            // 1-3
            byte[] lastUpdate = reader.ReadBytes(3);
            // 4-7
            int recordCount = reader.ReadInt32();
            // 8-9
            short headerSize = reader.ReadInt16();
            // 10-11
            short recordSize = reader.ReadInt16();
            // 12-13
            reader.ReadBytes(2); // reserved
                                 // 14
            byte incompleteTransaction = reader.ReadByte();
            // 15
            byte encrypted = reader.ReadByte();
            // 16-27
            reader.ReadBytes(12); // reserved for multi-user processing
                                  // 28
            byte hasMdx = reader.ReadByte();
            // 29
            byte languageDriverId = reader.ReadByte();
            // 30-31
            reader.ReadBytes(2); // reserved
                                 // 32-63
            byte[] languageDriverNameRaw = reader.ReadBytes(32);
            string languageDriverName = Encoding.Default.GetString(languageDriverNameRaw).TrimEnd((char)0);
            // 64-67
            reader.ReadBytes(4); // reserved

            RecordCount = recordCount;
            RecordSize = recordSize;

            DataTable = new DataTable();

                // field descriptor array (48 bytes each)
                Fields = new List<Field>();
            while (true)
            {
                int firstChar = reader.PeekChar();
                if (firstChar == 0x0D)
                {
                    reader.ReadByte();
                    // terminator
                    break;
                }
                // 0-31
                byte[] fieldNameRaw = reader.ReadBytes(32);
                string fieldName = Encoding.Default.GetString(fieldNameRaw).TrimEnd((char)0);
                // 32
                byte fieldTypeRaw = reader.ReadByte(); // B, C, D, N, L, M, @, I, +, F, 0, G
                char fieldType = Convert.ToChar(fieldTypeRaw);
                // 33
                byte fieldLength = reader.ReadByte();
                // 34
                byte fieldDecimalCount = reader.ReadByte();
                // 35-36
                reader.ReadBytes(2); // reserved
                                     // 37
                byte fieldProductionField = reader.ReadByte();
                // 38-39
                reader.ReadBytes(2); // reserved
                                     // 40-43
                int nextAutoIncrementValue = reader.ReadInt32();
                // 44-47
                byte[] reserved = reader.ReadBytes(4); // reserved

                //
                Fields.Add(new Field
                {
                    Name = fieldName,
                    Type = (NativeDbType)fieldType,
                    Length = fieldLength,
                    DecimalCount = fieldDecimalCount
                });

                // Create DataColumn
                Type dataColumnType;
                switch ((NativeDbType) fieldType)
                {
                    case NativeDbType.Char:
                        dataColumnType = typeof(string);
                        break;
                    case NativeDbType.Logical:
                        dataColumnType = typeof(bool);
                        break;
                    case NativeDbType.Double:
                        dataColumnType = typeof(double);
                        break;
                    case NativeDbType.Long:
                        dataColumnType = typeof(int);
                        break;
                    default:
                        dataColumnType = typeof(object);
                        break;
                }
                DataTable.Columns.Add(fieldName, dataColumnType);
            }

            int computedFieldLength = Fields.Sum(x => x.Length);
            Debug.Assert(1 + computedFieldLength == recordSize);

            // datas
            Datas = new List<object[]>(RecordCount);
            for (int i = 0; i < RecordCount; i++)
            {
                byte dataHeader = reader.ReadByte();
                if (dataHeader != 0x20 && dataHeader != 0x2A) // 0x2A for deleted record
                    throw new Exception("Invalid data separator");
                object[] data = new object[Fields.Count];

                DataRow row = DataTable.NewRow();

                int fieldIndex = 0;
                foreach (Field field in Fields)
                {
                    byte[] valueRaw = reader.ReadBytes(field.Length);

                    switch (field.Type)
                    {
                        case NativeDbType.Char:
                        {
                            string value = Encoding.Default.GetString(valueRaw).TrimEnd();
                            data [fieldIndex] = value;
                            row[fieldIndex] = value;
                            break;
                        }
                        case NativeDbType.Logical:
                        {
                            bool value = valueRaw[0] == 'T' || valueRaw[0] == 't' || valueRaw[0] == 'Y' || valueRaw[0] == 'y';
                            data[fieldIndex] = value;
                            row[fieldIndex] = value;
                            break;
                        }
                        case NativeDbType.Double: // even if field length is not fixed, we suppose double as 8 bytes field
                            {
                                bool isNegative = (valueRaw[0] & 0x80) == 0;
                                // if negative, inverse every bit
                                if (isNegative)
                                {
                                    for (int byteIndex = 0; byteIndex < 8; byteIndex++)
                                        valueRaw[byteIndex] = (byte) ~valueRaw[byteIndex];
                                }
                                // otherwise, remove 1st bit (negate 1st bit is also correct)
                                else
                                    valueRaw[0] &= 0x7F; // remove 1st bit
                                // reverse byte array
                                for (int byteIndex = 0; byteIndex < 4; byteIndex++)
                                {
                                    byte swap = valueRaw[byteIndex];
                                    valueRaw[byteIndex] = valueRaw[7 - byteIndex];
                                    valueRaw[7 - byteIndex] = swap;
                                }
                                double value = BitConverter.ToDouble(valueRaw, 0);
                                data[fieldIndex] = value;
                                row[fieldIndex] = value;
                                break;
                            }
                        case NativeDbType.Long:
                            {
                                bool isNegative = (valueRaw[0] & 0x80) == 0;
                                valueRaw[0] &= 0x7F; // remove 1st bit
                                int value = (isNegative ? -1 : 1)*valueRaw[3] + valueRaw[2]*0x100 + valueRaw[1]*0x10000 + valueRaw[0]*0x1000000;
                                data[fieldIndex] = value;
                                row[fieldIndex] = value;
                                // BitConverter.ToIn32 could also be used (simply reverse byte array, similarly to double case)
                                break;
                            }
                        default:
                            // TODO: other types
                            //throw new Exception($"Unhandled DbType {field.Type}");
                            data[fieldIndex] = valueRaw; // store without conversion
                            row[fieldIndex] = valueRaw;
                            break;
                    }
                    fieldIndex++;
                }
                Datas.Add(data);
                DataTable.Rows.Add(row);
            }

            int terminator = reader.PeekChar();
            //byte terminator = reader.ReadByte();
            if (terminator != 0x1A && terminator != -1)
                throw new Exception("Invalid DBF terminator");
        }
    }
}

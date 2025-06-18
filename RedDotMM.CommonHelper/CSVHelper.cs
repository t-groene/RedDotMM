using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace RedDotMM.CommonHelper
{


    public class CSVHelperInfoAttribute : Attribute
    {

        public CSVHelperInfoAttribute()
        {

        }

        public CSVHelperInfoAttribute(string displayName)
        {
            this.DisplayName = displayName;
        }

        public string DisplayName { get; set; }
    }


    public static class CsvHelper
    {

        //public static string DataTableToCSV(DataTable dt)
        //{
        //    StringBuilder sb = new StringBuilder();


        //    IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().
        //                              Select(column => column.ColumnName);
        //    sb.AppendLine(string.Join(";", columnNames));

        //    foreach (DataRow row in dt.Rows)
        //    {
        //        IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
        //        sb.AppendLine(string.Join(",", fields));
        //    }

        //    return sb.ToString();


        //}

        public static byte[] CreateCSVFile<T>(List<T> Elemente, string trennzeichen = ";") where T : class, new()
        {
            //Spalten Identifizieren

            Type myType = typeof(T);
            IList<PropertyInfo> Alleprops = new List<PropertyInfo>(myType.GetProperties());
            IList<PropertyInfo> props = Alleprops.Where(p => p.GetCustomAttribute(typeof(CSVHelperInfoAttribute)) != null).ToList();

            //Tabelle mit [Spalten, Zeilen]

            int spalten = 0;

            Dictionary<string, List<object>> KeyDictionaryElemente = new Dictionary<string, List<object>>();


            for (int i = 0; i < props.Count; i++)
            {
                if (props[i].PropertyType.IsAssignableTo(typeof(IDictionary)))
                {

                    KeyDictionaryElemente.Add(props[i].Name, new List<object>());

                    for (int j = 0; j < Elemente.Count; j++)
                    {
                        IDictionary propValue = (IDictionary)props[i].GetValue(Elemente[j], null);
                        if (propValue != null)
                        {

                            foreach (var k in propValue.Keys)
                            {

                                if (!KeyDictionaryElemente[props[i].Name].Contains(k))
                                {
                                    KeyDictionaryElemente[props[i].Name].Add(k);
                                }
                            }
                        }
                    }

                    spalten += KeyDictionaryElemente[props[i].Name].Count;
                }
                else
                {
                    spalten++;
                }
            }

            string[,] table = new string[spalten, Elemente.Count + 1];

            int spalteUeberschrift = 0;
            for (int i = 0; i < props.Count; i++)
            {
                var attr = props[i].GetCustomAttribute(typeof(CSVHelperInfoAttribute));
                var name = ((CSVHelperInfoAttribute)attr).DisplayName;
                if (string.IsNullOrEmpty(name))
                {
                    var displayAttr = props[i].GetCustomAttribute(typeof(DisplayAttribute));
                    if (displayAttr != null)
                    {
                        name = ((DisplayAttribute)displayAttr).Name;
                    }
                    else
                    {
                        name = props[i].Name;
                    }

                }

                if (KeyDictionaryElemente.ContainsKey(props[i].Name))
                {
                    for (int k = 0; k < KeyDictionaryElemente[props[i].Name].Count; k++)
                    {
                        if (!string.IsNullOrEmpty(name))
                        {
                            table[spalteUeberschrift, 0] = name + ": " + KeyDictionaryElemente[props[i].Name][k].ToString();
                        }
                        else
                        {
                            table[spalteUeberschrift, 0] = KeyDictionaryElemente[props[i].Name][k].ToString();
                        }

                        spalteUeberschrift++;
                    }
                }
                else
                {

                    table[spalteUeberschrift, 0] = name;
                    spalteUeberschrift++;
                }

            }



            for (int i = 0; i < Elemente.Count; i++)
            {
                var spalte = 0;
                for (int j = 0; j < props.Count; j++)
                {

                    if (KeyDictionaryElemente.ContainsKey(props[j].Name))
                    {
                        IDictionary propValue = (IDictionary)props[j].GetValue(Elemente[i], null);
                        for (int k = 0; k < KeyDictionaryElemente[props[j].Name].Count; k++)
                        {
                            if (propValue.Contains(KeyDictionaryElemente[props[j].Name][k]))
                            {
                                var valueDict = propValue[KeyDictionaryElemente[props[j].Name][k]];
                                var value = valueDict?.ToString() ?? "";
                                value = value.Replace(trennzeichen, "").Replace("\n", " ");
                                table[spalte, i + 1] = value;
                                spalte++;
                            }
                            else
                            {
                                spalte++;
                            }
                        }
                    }
                    else
                    {
                        object propValue = props[j].GetValue(Elemente[i], null);
                        var value = propValue?.ToString() ?? "";
                        value = value.Replace(trennzeichen, "").Replace("\n", " ");
                        table[spalte, i + 1] = value;
                        spalte++;
                    }
                }
            }

            var sb = new StringBuilder();

            for (int i = 0; i < table.GetLength(1); i++)
            {
                for (int j = 0; j < table.GetLength(0); j++)
                {
                    if (table[j, i] != null)
                    {
                        sb.Append(table[j, i].ToString());
                    }
                    if (j < table.GetLength(0) - 1)
                    {
                        sb.Append(trennzeichen);
                    }
                }
                sb.AppendLine();
            }

            byte[] b;
            b = System.Text.Encoding.Unicode.GetBytes(sb.ToString());

            return b;


        }





        public static List<T> ImportCSV<T>(string csvFilePath, char delimiter = ';') where T : class, new()
        {


            if (!File.Exists(csvFilePath))
                throw new FileNotFoundException($"Die Datei '{csvFilePath}' wurde nicht gefunden.");



            Type myType = typeof(T);
            IList<PropertyInfo> Alleprops = new List<PropertyInfo>(myType.GetProperties());
            IList<PropertyInfo> props = Alleprops.Where(p => p.GetCustomAttribute(typeof(CSVHelperInfoAttribute)) != null).ToList();

            Dictionary<string, PropertyInfo> propertyMap = new Dictionary<string, PropertyInfo>();
            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttribute(typeof(CSVHelperInfoAttribute));
                if (attr != null)
                {
                    string displayName = ((CSVHelperInfoAttribute)attr).DisplayName;
                    if (!string.IsNullOrEmpty(displayName))
                    {
                        propertyMap[displayName] = prop;
                    }
                }
            }

            List<T> result = new List<T>();
            using (StreamReader reader = new StreamReader(csvFilePath))
            {
                string headerLine = reader.ReadLine();
                if (headerLine == null)
                    return result;
                string[] headers = headerLine.Split(delimiter);
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        continue;
                    string[] values = line.Split(delimiter);
                    T item = Activator.CreateInstance<T>();
                    for (int i = 0; i < headers.Length; i++)
                    {

                        PropertyInfo prop = propertyMap.ContainsKey(headers[i]) ? propertyMap[headers[i]] : null;

                        if (prop != null && i < values.Length)
                        {

                            Type t = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                            object value = (values[i] == null) ? null : Convert.ChangeType(values[i], t);
                            prop.SetValue(item, value, null);


                            //object value = Convert.ChangeType(values[i], prop.PropertyType);
                            //    if(prop.PropertyType.IsEnum)
                            //{
                            //    value = Enum.Parse(prop.PropertyType, values[i]);
                            //}
                            //else if (prop.PropertyType == typeof(bool))
                            //{
                            //    value = values[i].Equals("1") || values[i].Equals("true", StringComparison.OrdinalIgnoreCase);
                            //}
                            //else if (prop.PropertyType == typeof(int))
                            //{
                            //    value = int.Parse(values[i]);
                            //}
                            //else if (prop.PropertyType == typeof(double))
                            //{
                            //    value = double.Parse(values[i], System.Globalization.CultureInfo.InvariantCulture);
                            //}
                            //else if(prop.PropertyType == typeof(string)){
                            //    value= values[i].Replace("\n", " ").Replace("\r", "").Trim();
                            //}

                            //    prop.SetValue(item, value);
                            //}


                        }
                        
                    }
                    result.Add(item);
                }
                return result;
            }


        }

    }

}


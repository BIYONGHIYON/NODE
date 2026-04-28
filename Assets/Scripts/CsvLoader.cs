using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class CsvLoader<TCsvData> where TCsvData : CsvData,new()
{
    public static List<TCsvData> LoadData(TextAsset csvText)
    {
        var data=new List<TCsvData>();
        var reader=new StringReader(csvText.text);

        while(reader.Peek()>-1)
        {
            var line=reader.ReadLine();
            var csvData=new TCsvData();
            csvData.SetData(line.Split(','));
            data.Add(csvData);   
        }
        return data;
    }
}

/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;*/

public class ConstellationLineData : CsvData
{
    public string Name { get; set; }
    public int StartHip { get; set; }
    public int EndHip { get; set; }
    public override void SetData(string[] data)
    {
        Name = data[0];
        StartHip=int.Parse(data[1]);
        EndHip=int.Parse(data[2]);
    }
}


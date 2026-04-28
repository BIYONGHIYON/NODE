/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;*/

public class ConstellationNameData : CsvData
{
  public int Id { get; set; }   
  public string Summary { get; set; }//¾àÄª
  public string Name { get; set; }
  public string KoreanName { get; set; }

    public override void SetData(string[] data)
    {
        Id=int.Parse(data[0]);
        Summary=data[1];
        Name=data[2];
        KoreanName=data[3];
    }
}

/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
*/
public class ConstellationPositionData : CsvData
{
   public int Id {  get; set; }
    public float RightAscension { get; set; }
    public float Declination { get; set; }
    public override void SetData(string[] data)
    {
        Id = int.Parse(data[0]);
        RightAscension = RightAscensionToDegree(int.Parse(data[1]), int.Parse(data[2]));
        Declination = DeclinationToDegree(int.Parse(data[3])); 

    }
}

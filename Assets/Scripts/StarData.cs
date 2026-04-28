/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;*/

public class StarData : CsvData
{
    public int Hip { get;set; }  
    public float RightAscension { get;set; }    
    public float Declination { get; set; }
    public float ApparentMagnitude {  get; set; }
    public string ColorType;
    public bool UseConstellation;

    public override void SetData(string[] data)
    {
        Hip=int.Parse(data[0]);//문자열->int형으로 바꾸기
        RightAscension = RightAscensionToDegree(int.Parse(data[1]),int.Parse(data[2]),float.Parse(data[3]));
        Declination = DeclinationToDegree(int.Parse(data[4]), int.Parse(data[5]), float.Parse(data[6]));
        ApparentMagnitude=float.Parse(data[7]);
        ColorType = data[13].Substring(0,1);
    }
}

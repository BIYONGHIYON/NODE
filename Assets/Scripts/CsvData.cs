/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
*/
public abstract class CsvData
{
    public abstract void SetData(string[] data); //읽어 들인 데이터 등록
    public float RightAscensionToDegree(int hour, int min = 0, float sec = 0.0f) //적경을 각도로 전환
    {
        var h = 360.0f / 24.0f; //1시간의 각도
        var m = h / 60.0f;//1분의 각도
        var s = m / 60.0f;//1초의 각도

        return (h * hour + m * min + s * sec) * -1.0f;

    }
    //적위를 각도로 전환
    public float DeclinationToDegree(int deg, int min = 0, float sec = 0.0f)
    {
        var plusMinus = 1.0f;
        if (deg < 0.0f)
        {
            plusMinus = -1.0f;
            deg *= -1;
        }
        return DeclinationToDegree(plusMinus, deg, min, sec);
    }
    public float DeclinationToDegree(float plusMinus, int deg, int min = 0, float sec = 0.0f)
    {
        return (deg * plusMinus + min / 60.0f * plusMinus + sec / (60.0f * 60.0f) * plusMinus) * -1.0f;
    }
}
  
  


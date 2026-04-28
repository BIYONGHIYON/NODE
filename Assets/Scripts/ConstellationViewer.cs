using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class ConstellationViewer : MonoBehaviour
{

    [SerializeField]
    TextAsset starDataCSV;
    [SerializeField]
    TextAsset starMajorDataCSV;
    [SerializeField]
    TextAsset constellationNameDataCSV;
    [SerializeField]
    TextAsset constellationPositionDataCSV;
    [SerializeField]
    TextAsset constellationLineDataCSV;
    [SerializeField]
    GameObject constellationPrefab;

    List<StarData> starData;
    List<StarMajorData> starMajorData;
    List<ConstellationNameData> constellationNameData;
    List<ConstellationPositionData> constellationPositionData;
    List<ConstellationLineData> constellationLineData;

    List<ConstellationData> constellationData;

    void Start()
    {
        LoadCSV();
        ArrangementData();
        CreateConstellation();
    }
    void LoadCSV()
    {
        starData = CsvLoader<StarData>.LoadData(starDataCSV);
        starMajorData = CsvLoader<StarMajorData>.LoadData(starMajorDataCSV);
        constellationNameData = CsvLoader<ConstellationNameData>.LoadData(constellationNameDataCSV);
        constellationPositionData = CsvLoader<ConstellationPositionData>.LoadData(constellationPositionDataCSV);
        constellationLineData = CsvLoader<ConstellationLineData>.LoadData(constellationLineDataCSV);
    }
    void ArrangementData()
    {
        MergeStarData();
        constellationData = new List<ConstellationData>();
        foreach (var name in constellationNameData)
        {
            constellationData.Add(CollectConstellationData(name));
        }
        var data = new ConstellationData();
        data.Stars = starData.Where(s => s.UseConstellation == false).ToList();
        constellationData.Add(data);
    }
    void MergeStarData()
    {
        foreach (var star in starMajorData)
        {
            var data = starData.FirstOrDefault(s => star.Hip == s.Hip);
            if (data != null)
            {
                data.RightAscension = star.RightAscension;
                data.Declination = star.Declination;
            }
            else
            {
                if (star.ApparentMagnitude <= 5.0f)
                {
                    starData.Add(star);
                }
            }

        }
    }
    ConstellationData CollectConstellationData(ConstellationNameData name)
    {
        var data = new ConstellationData();
        data.Name = name;
        data.Position = constellationPositionData.FirstOrDefault(s => name.Id == s.Id);
        data.Lines = constellationLineData.Where(s => name.Summary == s.Name).ToList();
        data.Stars = new List<StarData>();
        foreach (var line in data.Lines)
        {
            var start = starData.FirstOrDefault(s => s.Hip == line.StartHip); 
            data.Stars.Add(start);

            var end = starData.FirstOrDefault(s => s.Hip == line.EndHip); 
            data.Stars.Add(end);

            start.UseConstellation = end.UseConstellation = true;
        }
        return data;
    }
    void CreateConstellation()
    {
        foreach(var data in constellationData)
        {
            var constellation = Instantiate(constellationPrefab);
            var drawConstellation=constellation.GetComponent<DrawConstellation>();
            drawConstellation.ConstellationData = data;

            constellation.transform.SetParent(transform, false);
        }
    }
}
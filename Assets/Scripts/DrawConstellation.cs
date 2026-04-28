using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class DrawConstellation : MonoBehaviour
{
    public float SpaceSize = 1500.0f;
    static float StarBaseSize = 8.0f;
    [SerializeField]
    GameObject starPrefab;
    [SerializeField]
    GameObject linePrefab;
    [SerializeField]
    GameObject namePrefab;
    public ConstellationData ConstellationData { get; set; }
    GameObject linesParent;
    public GameObject LinesParent { get { return linesParent; } }
    void Start()
    {
        if(ConstellationData.Name != null)
        {
            gameObject.name=ConstellationData.Name.Name;
        }
        CreateConstellation();
    }
    void CreateConstellation()
    {
        foreach (var star in ConstellationData.Stars)
        {
            var starObject = CreateStar(star);
            starObject.transform.SetParent(transform, false);
        }
        if (ConstellationData.Lines != null)
        {
            linesParent = new GameObject("Lines");
            linesParent.transform.SetParent(transform, false);
            var parent = linesParent.transform;
            foreach (var line in ConstellationData.Lines)
            {
                var lineObject = CreateLine(line);
                lineObject.transform.SetParent(parent, false);
            }
        }
        if (ConstellationData.Name != null)
        {
            var nameObject = CreateName(ConstellationData.Name, ConstellationData.Position);
            nameObject.transform.SetParent(transform, false);
        }
    }

    GameObject CreateStar(StarData starData)
    {
        var star = Instantiate(starPrefab);
        var starTrans = star.transform;

        starTrans.localRotation = Quaternion.Euler(starData.Declination, starData.RightAscension, 0.0f);

        star.name = string.Format("{0}", starData.Hip);

        var child = starTrans.GetChild(0);
        child.transform.localPosition = new Vector3(0.0f, 0.0f, SpaceSize);

        var size = StarBaseSize - starData.ApparentMagnitude;
        child.transform.localScale = new Vector3(size, size, size);

        var meshRenderer = child.GetComponent<Renderer>();
        var color = Color.white;

        switch (starData.ColorType)
        {
            case "0":
                color = Color.blue;
                break;
            case "B":
                color = Color.Lerp(Color.blue, Color.white, 0.5f);
                break;
            default:
            case "A":
                color = Color.white;
                break;
            case "F":
                color = Color.Lerp(Color.white, Color.yellow, 0.5f);
                break;
            case "G":
                color = Color.yellow;
                break;
            case "K":
                color = new Color(243.0f / 255.0f, 152.0f / 255.0f, 0.0f);
                break;
            case "M":
                color = new Color(200.0f / 255.0f, 10.0f / 255.0f, 0.0f);
                break;
        }
        meshRenderer.material.SetColor("_Color", color);
        return star;
    }

    GameObject CreateLine(ConstellationLineData lineData)
    {
        var start=GetStar(lineData.StartHip);
        var end= GetStar(lineData.EndHip);
        var line = Instantiate(linePrefab);
        var lineRenderer=line.GetComponent<LineRenderer>();

        lineRenderer.SetPosition(0, Quaternion.Euler(start.Declination, start.RightAscension, 0.0f) * new Vector3(0.0f, 0.0f, SpaceSize));
        lineRenderer.SetPosition(1,Quaternion.Euler(end.Declination,end.RightAscension,0.0f)*new Vector3(0.0f, 0.0f, SpaceSize));
        return line;
    }
    StarData GetStar(int hip)
    {
        return ConstellationData.Stars.FirstOrDefault(s => hip == s.Hip);
    }
    GameObject CreateName(ConstellationNameData nameData, ConstellationPositionData positionData)
    {
        var text = Instantiate(namePrefab);
        var textTrans = text.transform;
        textTrans.localRotation=Quaternion.Euler(positionData.Declination,positionData.RightAscension, 0.0f);
        text.name = nameData.Name;

        var child=textTrans.GetChild(0);
        child.transform.localPosition=new Vector3(0.0f,0.0f,SpaceSize);

        var textMesh=child.GetComponent<TextMesh>();
        textMesh.text = string.Format("{0}", nameData.KoreanName);
        return text;
    }
}


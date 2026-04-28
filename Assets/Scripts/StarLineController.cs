using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarLineController : MonoBehaviour
{
    [SerializeField]
    bool lineSwitch = false;
    [SerializeField]
    GameObject Constellation;

    List<GameObject> starLines = new();     // ���ڸ� ����, �̸� ������Ʈ
    List<GameObject> starCetus = new();     // ���ڸ� �̸�
    private bool linesInitialized = false;  // starLines �ʱ�ȭ ����

    void Start()
    {
        Constellation = GameObject.Find("ConstellationViewer");
    }

    void Update()
    {
        // ���ڸ� ���� ������Ʈ ���� �ѹ� ��Ȱ��ȭ
        if (!linesInitialized && Constellation != null)
        {
            InitializeLines();
        }

        // ��ư Ŭ�� �� ���ڸ� ���� ������Ʈ Ȱ��ȭ
        if (lineSwitch && linesInitialized)
        {
            ShowConstellationObj();
        }

        // ��ư Ŭ�� �� ���ڸ� ���� ������Ʈ ��Ȱ��ȭ
        if (!lineSwitch && linesInitialized)
        {
            HideConstellationObj();
        }

        /* SPACE BAR�� ���ڸ� Show/Hide ��� �߰�
        if (Input.GetKeyDown(KeyCode.Space))
        {
            lineSwitch = !lineSwitch;
        }*/
    }


    // �� ó�� �ѹ� ���� ������Ʈ�� ��Ȱ��ȭ ��Ű��
    void InitializeLines()
    {
        foreach (Transform child in Constellation.transform)
        {
            // ���ڸ� Line ������Ʈ ã��
            Transform lineObj = child.Find("Lines");
            if (lineObj != null)
            {
                starLines.Add(lineObj.gameObject);
            }
            // ���ڸ� Name ������Ʈ ã��
            Transform nameObj = child.Find(child.name);
            if (lineObj != null)
            {
                starCetus.Add(nameObj.gameObject);
            }
        }

        // ���ڸ� ���� ������Ʈ ��Ȱ��ȭ
        HideConstellationObj();

        // starLines �ʱ�ȭ �Ϸ�
        linesInitialized = true;
    }

    void ShowConstellationObj()
    {
        foreach (GameObject obj in starLines)
        {
            obj.SetActive(true);
        }

        foreach (GameObject obj in starCetus)
        {
            obj.SetActive(true);
        }
    }

    void HideConstellationObj()
    {
        foreach (GameObject obj in starLines)
        {
            obj.SetActive(false);
        }

        foreach (GameObject obj in starCetus)
        {
            obj.SetActive(false);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class ResourceWidgetsController : MonoBehaviour
{
    public GameObject widgetsPanel;

    [SerializeField] GameObject widgetPrefab;
    [SerializeField] int spacing;

    public List<ResourceWidget> widgets = new List<ResourceWidget>();

    List<ResourceWidget> enabledWidgets = new List<ResourceWidget>();

    public void Init()
    {
        widgetsPanel.transform.localScale = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1 / transform.localScale.z);

        widgets.Clear();
        foreach (Transform t in widgetsPanel.transform)
            Destroy(t.gameObject);
    }

    void Update()
    {
        widgetsPanel.transform.rotation = CameraController.Instance.mainCamera.transform.rotation;
    }

    public void CreateWidget(Article article)
    {
        var p = Instantiate(widgetPrefab, widgetsPanel.transform);

        ResourceWidget widget = p.GetComponent<ResourceWidget>();
        widget.ChangeSprite(article.sprite);
        widget.articleName = article.name;
        widgets.Add(widget);

        UpdateWidgets();
    }

    public void UpdateWidgets()
    {
        enabledWidgets.Clear();

        for (int i = 0; i < widgets.Count; i++)
        {
            if (widgets[i].amount != 0)
            {
                widgets[i].gameObject.SetActive(true);
                enabledWidgets.Add(widgets[i]);
            }
            else
                widgets[i].gameObject.SetActive(false);
        }

        ArrangeWidgets();
    }

    void ArrangeWidgets()
    {
        enabledWidgets.Sort((x, y) => { return x.price.CompareTo(y.price); });

        for (int i = 0; i < enabledWidgets.Count; i++)
        {
            enabledWidgets[i].transform.localPosition = new Vector3(i * spacing - enabledWidgets.Count * spacing / 2 + spacing / 2, 0, 0);
        }
    }
}

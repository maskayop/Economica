using System.Collections.Generic;
using UnityEngine;

public class ResourceWidgetsController : MonoBehaviour
{
    public GameObject widgetsPanel;

    [SerializeField] GameObject widgetPrefab;
    [SerializeField] int spacing;

    public List<ResourceWidget> widgets = new List<ResourceWidget>();

    void Start()
    {
        widgetsPanel.transform.localScale = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1 / transform.localScale.z);
    }

    void Update()
    {
        widgetsPanel.transform.rotation = CameraController.Instance.mainCamera.transform.rotation;
    }

    public void CreateWidget(Resource res)
    {
        var p = Instantiate(widgetPrefab, widgetsPanel.transform);

        ResourceWidget widget = p.GetComponent<ResourceWidget>();
        widget.ChangeSprite(res.sprite);
        widgets.Add(widget);

        ArrangeWidgets();
    }

    void ArrangeWidgets()
    {
        for (int i = 0; i < widgets.Count; i++)
            widgets[i].transform.localPosition = new Vector3(i * spacing - widgets.Count * spacing / 2 + spacing / 2, 0, 0);
    }
}

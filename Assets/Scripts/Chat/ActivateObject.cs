using UnityEngine;
using UnityEngine.UI;

public class ActivateObject : MonoBehaviour
{
    // 设置一个公共变量，以便在Unity编辑器中将其链接到GameObject
    public GameObject targetObject;

    private Button button;

    void Start()
    {
        // 获取Button组件
        button = GetComponent<Button>();

        // 添加点击事件
        button.onClick.AddListener(ActivateTargetObject);

        // 初始时禁用目标对象
        targetObject.SetActive(false);
    }

    // 按钮点击事件处理方法
    void ActivateTargetObject()
    {
        targetObject.SetActive(true);
    }
}

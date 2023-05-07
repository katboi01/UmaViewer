using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class TransparentWindow : MonoBehaviour
{
    [SerializeField]
    private Material m_Material;
    [SerializeField]
    private Camera mainCamera;

    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    // Define function signatures to import from Windows APIs
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    const uint SWP_NOACTIVATE = 0x0010;
    const uint SWP_SHOWWINDOW = 0x0040;

    // Definitions of window styles
    const int GWL_STYLE = -16;
    const int GWL_EXSTYLE = -20;
    const uint WS_POPUP = 0x80000000;
    const uint WS_VISIBLE = 0x10000000;
    const uint WS_EX_TRANSPARENT = 0x20;
    const uint WS_EX_LAYERED = 0x80000;

    private IntPtr hwnd;
    private bool isMouseOverModel = false;

    public GraphicRaycaster graphicRaycaster;
    public EventSystem eventSystem;
    private bool isMouseOverUI = false;

    //对话框
    public GameObject dialogBox;

    void Start()
    {
#if !UNITY_EDITOR
        var margins = new MARGINS() { cxLeftWidth = -1 };
        hwnd = GetActiveWindow();
        DwmExtendFrameIntoClientArea(hwnd, ref margins);
        SetWindowLong(hwnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);
        SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, Screen.currentResolution.width, Screen.currentResolution.height, SWP_NOACTIVATE | SWP_SHOWWINDOW);
#endif
    }

    private void ToggleDialogBox()
    {
        bool isActive = dialogBox.activeSelf;
        dialogBox.SetActive(!isActive);
    }

    void Update()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // 新增的UI检测代码
        PointerEventData pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, results);

        isMouseOverUI = false;
        foreach (RaycastResult result in results)
        {
            if (result.gameObject == dialogBox)
            {
                isMouseOverUI = true;
                break;
            }
        }

        if (isMouseOverModel || isMouseOverUI)
        {
            SetWindowLong(hwnd, GWL_EXSTYLE, WS_EX_LAYERED);
        }
        else
        {
            SetWindowLong(hwnd, GWL_EXSTYLE, WS_EX_TRANSPARENT | WS_EX_LAYERED);
        }

        //检测鼠标右键单击
        if (Input.GetMouseButtonDown(1))
        {
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.name.StartsWith("Chara_"))
                {
                    ToggleDialogBox();
                }
            }
        }

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.name.StartsWith("Chara_"))
            {
                if (!isMouseOverModel)
                {
                    isMouseOverModel = true;
                    SetWindowLong(hwnd, GWL_EXSTYLE, WS_EX_LAYERED);
                }
            }
            else
            {
                if (isMouseOverModel)
                {
                    isMouseOverModel = false;
                    SetWindowLong(hwnd, GWL_EXSTYLE, WS_EX_TRANSPARENT | WS_EX_LAYERED);
                }
            }
        }
        else
        {
            if (isMouseOverModel)
            {
                isMouseOverModel = false;
                SetWindowLong(hwnd, GWL_EXSTYLE, WS_EX_TRANSPARENT | WS_EX_LAYERED);
            }
        }
    }
}
    
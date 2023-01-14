using Gallop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class PageManager : MonoBehaviour
{
    public ScrollRect ScrollRect;
    public InputField PageText;
    public int ShowCount;
    public GameObject ContainerPrefab;
    private int totalPage;
    private int currentPage = 0;
    private List<Entry> Entries;
    public void Initialize(List<Entry> entries, ScrollRect scrollRect)
    {
        ScrollRect = scrollRect;
        Entries = entries;
        ShowCount = Mathf.FloorToInt(ScrollRect.GetComponent<RectTransform>().rect.height / ContainerPrefab.GetComponent<RectTransform>().rect.height);
        totalPage = Mathf.CeilToInt(entries.Count / (float)ShowCount);
        if (totalPage > 0)
        {
            LoadPage(currentPage);
        }
    }

    public void ResetCtrl()
    {
        if(ScrollRect)Clear();
        totalPage = 0;
        currentPage = 0;
        PageText.text = "";
        Entries = null;
        ((Text)PageText.placeholder).text = $"- / -";
    }

    private void LoadPage(int index)
    {
        if (totalPage == 0) return;
        Clear();
        PageText.text = "";
        ((Text)PageText.placeholder).text = $"{currentPage + 1} / {totalPage}";
        var start = ShowCount * index;
        for (int i = 0;i< ShowCount; i++)
        {
            if (start + i >= Entries.Count) break;
            var entry = Entries[start + i];
            var container = Instantiate(ContainerPrefab, ScrollRect.content).GetComponent<UmaUIContainer>();
            container.Name = container.name = entry.Name;
            if (entry.FontSize > 0) 
            {
                container.FontSize = entry.FontSize;
            }
            if(entry.Sprite != null)
            {
                container.Image.sprite = entry.Sprite;
                container.Image.enabled = true;
            }
            container.Button.onClick.AddListener(()=> { entry.OnClick(container); });
        }
    }

    public void JumpToPage(string index)
    {
        PageText.text = "";
        if (totalPage == 0|| index == "") return;
        currentPage = Mathf.Clamp(int.Parse(index) - 1, 0, totalPage - 1);
        LoadPage(currentPage);
    }

    private void Clear()
    {
        for (int i = ScrollRect.content.childCount - 1; i >= 0; i--)
        {
            Destroy(ScrollRect.content.GetChild(i).gameObject);
        }
    }

    public void NextPage()
    {
        if (totalPage == 0) return;
        currentPage = Mathf.Clamp(currentPage + 1, 0, totalPage - 1);
        LoadPage(currentPage);
    }

    public void PrevPage()
    {
        if (totalPage == 0) return;
        currentPage = Mathf.Clamp(currentPage - 1, 0, totalPage - 1);
        LoadPage(currentPage);
    }

    public class Entry
    {
        public string Name;
        public int FontSize;
        public Sprite Sprite;
        public UnityEngine.Events.UnityAction<UmaUIContainer> OnClick;
    }
}


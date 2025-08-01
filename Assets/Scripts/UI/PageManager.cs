using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PageManager : MonoBehaviour
{
    public ScrollRect ScrollRect;
    public InputField PageText;
    public InputField SearchText;
    public int ShowCount;
    public GameObject ContainerPrefab;
    private int totalPage;
    private int currentPage = 0;
    private List<Entry> Entries;
    private List<Entry> SubEntries;

    public void Initialize(List<Entry> entries, ScrollRect scrollRect)
    {
        ScrollRect = scrollRect;
        Entries = entries;
        if (SearchText)
            SearchText.text = "";
        ShowCount = Mathf.FloorToInt(ScrollRect.GetComponent<RectTransform>().rect.height / ContainerPrefab.GetComponent<RectTransform>().rect.height);
        totalPage = Mathf.CeilToInt(entries.Count / (float)ShowCount);
        if (totalPage > 0)
        {
            currentPage = 0;
            LoadPage(currentPage);
            SearchText.interactable = true;
        }
        else
        {
            Clear();
        }
    }

    public void AddEntries(Entry entry)
    {
        if (!ScrollRect) return;

        if (SearchText)
            SearchText.text = "";
        Entries.Add(entry);
        totalPage = Mathf.CeilToInt(Entries.Count / (float)ShowCount);
        if (currentPage == totalPage - 1)
        {
            LoadPage(currentPage);
        }
        else
        {
            ((Text)PageText.placeholder).text = $"{currentPage + 1} / {totalPage}";
        }
    }

    public void RemoveEntry(Entry entry)
    {
        if (!ScrollRect) return;

        if (SearchText)
            SearchText.text = "";
        Entries.Remove(entry);
        totalPage = Mathf.CeilToInt(Entries.Count / (float)ShowCount);
        if (currentPage > totalPage - 1)
        {
            currentPage = Mathf.Max(totalPage - 1, 0);
            LoadPage(currentPage);
        }
        else
        {
            ((Text)PageText.placeholder).text = $"{currentPage + 1} / {totalPage}";
        }
    }

    public void ResetCtrl()
    {
        totalPage = 0;
        currentPage = 0;
        PageText.text = "";
        if(SearchText)
        SearchText.text = "";
        SearchText.interactable = false;
        SubEntries = null;
        Entries = null;
        ((Text)PageText.placeholder).text = $"- / -";
    }

    private void LoadPage(int index)
    {
        if (totalPage == 0) return;
        var entries = (SearchText && SearchText.text != "" ? SubEntries : Entries);
        Clear();
        PageText.text = "";
        ((Text)PageText.placeholder).text = $"{currentPage + 1} / {totalPage}";
        var start = ShowCount * index;
        for (int i = 0;i< ShowCount; i++)
        {
            if (start + i >= entries.Count) break;
            var entry = entries[start + i];
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

    public void OnSearch(string val)
    {
        Clear();
        if (Entries == null) return;
        if(val == "")
        {
            Initialize(Entries, ScrollRect);
        }
        else
        {
            SubEntries = new List<Entry>(Entries.Where(a => a.Name.Contains(val)));
            ShowCount = Mathf.FloorToInt(ScrollRect.GetComponent<RectTransform>().rect.height / ContainerPrefab.GetComponent<RectTransform>().rect.height);
            totalPage = Mathf.CeilToInt(SubEntries.Count / (float)ShowCount);
            if (totalPage > 0)
            {
                currentPage = 0;
                LoadPage(currentPage);
            }
        }
    }

    public void Clear()
    {
        if (!ScrollRect) return;
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


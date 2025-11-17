using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LevelController : MonoBehaviour
{
    [Header("Level Setup")]
    public GameObject levelPrefab;
    public Transform contentRoot;

    [Range(1, 200)]
    public int totalLevels = 50;
    [Range(1, 20)]
    public int itemsPerPage = 10;

    [Header("Pagination UI")]
    public Button previousButton;
    public Button nextButton;

    private readonly List<Level> spawnedLevels = new List<Level>();
    private int currentPage;
    private PlayerData playerData;

    private void Start()
    {
        LoadPlayerData();
        BuildPage(0);

        if (previousButton != null)
            previousButton.onClick.AddListener(ShowPreviousPage);
        if (nextButton != null)
            nextButton.onClick.AddListener(ShowNextPage);
    }

    private void LoadPlayerData()
    {
        playerData = PlayerDataStorage.LoadOrCreateDefault(totalLevels);
        if (playerData != null && playerData.levels != null && playerData.levels.Count > 0)
        {
            totalLevels = playerData.levels.Count;
        }
    }

    private void BuildPage(int pageIndex)
    {
        ClearExistingLevels();

        currentPage = Mathf.Clamp(pageIndex, 0, MaxPageIndex());

        int startIndex = currentPage * itemsPerPage;
        int endIndex = Mathf.Min(startIndex + itemsPerPage, totalLevels);

        for (int i = startIndex; i < endIndex; i++)
        {
            Level levelComponent = Instantiate(levelPrefab, contentRoot).GetComponent<Level>();
            PlayerLevelData levelInfo = GetLevelInfo(i);
            if (levelInfo == null)
            {
                levelInfo = new PlayerLevelData
                {
                    level = i + 1,
                    star = 0,
                    isLocked = i != 0
                };
            }

            levelComponent.Init(levelInfo, playerData);
            spawnedLevels.Add(levelComponent);
        }

        UpdatePaginationButtons();
    }

    private void ClearExistingLevels()
    {
        if (contentRoot != null)
        {
            for (int i = contentRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(contentRoot.GetChild(i).gameObject);
            }
        }

        spawnedLevels.Clear();
    }

    private void UpdatePaginationButtons()
    {
        if (previousButton != null)
            previousButton.interactable = currentPage > 0;
        if (nextButton != null)
            nextButton.interactable = currentPage < MaxPageIndex();
    }

    private int MaxPageIndex()
    {
        if (totalLevels == 0) return 0;
        return Mathf.Max(0, Mathf.CeilToInt((float)totalLevels / itemsPerPage) - 1);
    }

    private PlayerLevelData GetLevelInfo(int index)
    {
        if (playerData == null || playerData.levels == null) return null;
        if (index < 0 || index >= playerData.levels.Count) return null;
        return playerData.levels[index];
    }

    public void ShowNextPage()
    {
        if (currentPage >= MaxPageIndex()) return;
        BuildPage(currentPage + 1);
    }

    public void ShowPreviousPage()
    {
        if (currentPage <= 0) return;
        BuildPage(currentPage - 1);
    }
}

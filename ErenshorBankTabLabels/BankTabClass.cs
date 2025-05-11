using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BankTabPlugin
{
    [BepInPlugin("com.Tingilinde.BankTabLabels", "Bank Tab Plugin", "0.6")]
    public class BankTabPlugin : BaseUnityPlugin
    {
        private Dictionary<int, ConfigEntry<string>> pageNameConfig = new Dictionary<int, ConfigEntry<string>>();
        private GameObject labelObject = null;
        private int lastPage = -1;

        private GameObject bankObject = null;
        private TextMeshProUGUI numberTMP = null;

        private void Awake()
        {
#if DEBUG
            Logger.LogInfo("Bank Tab Labels loaded! You can now better organize your bank mess. <3");
#endif
            for (int i = 1; i <= 98; i++)
            {
                string description = i == 1
                    ? "Custom names for each bank page. Leave blank for default view. Text will cut/truncated if too long."
                    : "";

                pageNameConfig[i] = Config.Bind(
                    "Bank Page Names",
                    $"Bank Page {i}",
                    "",
                    new ConfigDescription(
                        description,
                        null,
                        new ConfigurationManagerAttributes { Order = 99 - i } // Ensures ascending numerical order in Config Manager
                    )
                );
            }

#if DEBUG
            Logger.LogInfo("Default BepInEx config loaded.");
#endif
        }

        private void Start()
        {
            StartCoroutine(MonitorBankUI());
        }

        private IEnumerator MonitorBankUI()
        {
            while (true)
            {
                float waitTime = 0.5f;

                if (bankObject == null)
                {
                    bankObject = GameObject.Find("Bank");
                }

                if (bankObject != null && bankObject.activeInHierarchy)
                {
                    waitTime = 0.1f;

                    if (numberTMP == null)
                    {
                        var tmps = bankObject.GetComponentsInChildren<TextMeshProUGUI>(true);
                        foreach (var tmp in tmps)
                        {
                            if (tmp.gameObject.name == "Text (TMP) (3)" &&
                                tmp.transform.parent != null &&
                                tmp.transform.parent.name == "Bank")
                            {
                                numberTMP = tmp;
                                break;
                            }
                        }
                    }

                    if (numberTMP != null &&
                        int.TryParse(numberTMP.text.Trim(), out int currentPage) &&
                        currentPage != lastPage)
                    {
                        lastPage = currentPage;
                        UpdateLabel(currentPage);
                    }
                }
                else
                {
                    numberTMP = null;
                }

                yield return new WaitForSecondsRealtime(waitTime);
            }
        }

        private void UpdateLabel(int currentPage)
        {
            string customLabel = pageNameConfig.TryGetValue(currentPage, out var entry) ? entry.Value : "";
            if (string.IsNullOrWhiteSpace(customLabel))
            {
                if (labelObject != null)
                    labelObject.SetActive(false);
                return;
            }

            if (labelObject == null)
            {
                labelObject = new GameObject("CustomPageLabel");
                labelObject.transform.SetParent(numberTMP.transform.parent, false);

                var text = labelObject.AddComponent<TextMeshProUGUI>();
                text.font = numberTMP.font;
                text.fontSize = numberTMP.fontSize * 0.9f;
                text.alignment = TextAlignmentOptions.Center;
                text.raycastTarget = false;
                text.overflowMode = TextOverflowModes.Ellipsis;

                var rect = labelObject.GetComponent<RectTransform>();
                rect.anchorMin = numberTMP.rectTransform.anchorMin;
                rect.anchorMax = numberTMP.rectTransform.anchorMax;
                rect.anchoredPosition = numberTMP.rectTransform.anchoredPosition + new Vector2(0, 24);
                rect.sizeDelta = numberTMP.rectTransform.sizeDelta;
            }
            else
            {
                labelObject.SetActive(true);
            }

            var labelTMP = labelObject.GetComponent<TextMeshProUGUI>();
            labelTMP.text = customLabel;

#if DEBUG
            Logger.LogInfo($"✅ Updated label to: '{labelTMP.text}' for page {currentPage}");
#endif
        }

        // Internal helper class to control how settings are shown in Config Manager
        private sealed class ConfigurationManagerAttributes
        {
            public bool? ShowRangeAsPercent;
            public bool? IsAdvanced;
            public int? Order;
            public bool? Browsable;
            public string Category;
            public string DispName;
            public Action<ConfigEntryBase> CustomDrawer;
        }
    }
}

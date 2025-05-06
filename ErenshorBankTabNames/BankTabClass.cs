using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace BankTabPlugin
{
    [BepInPlugin("com.example.banktabnames", "Bank Tab Plugin", "1.0.0")]
    public class BankTabPlugin : BaseUnityPlugin
    {
        private ConfigFile customConfig;
        private Dictionary<int, ConfigEntry<string>> pageNameConfig = new Dictionary<int, ConfigEntry<string>>();
        private GameObject labelObject = null;
        private int lastPage = -1;

        private float checkTimer = 0f;
        private const float checkInterval = 0.1f; // check every 0.1s

        private void Awake()
        {
            Logger.LogInfo("Bank Tab Names loaded! You can now better organize your bank mess. <3");

            customConfig = new ConfigFile(Paths.ConfigPath + "/BankTabNames.cfg", true);

            for (int i = 1; i <= 98; i++)
            {
                string description = i == 1
                    ? "Custom names for each bank page. Leave blank for default view. Text will cut/truncated if too long."
                    : "";

                pageNameConfig[i] = customConfig.Bind(
                    "Bank Page Names",
                    $"PageName_{i}",
                    "",
                    description
                );

                var _ = pageNameConfig[i].Value;
            }

            Logger.LogInfo("BankTabNames.cfg config loaded.");
        }

        private void Update()
        {
            checkTimer += Time.unscaledDeltaTime;
            if (checkTimer < checkInterval)
                return;

            checkTimer = 0f;

            var bank = GameObject.Find("Bank");
            if (bank == null || !bank.activeInHierarchy)
                return;

            TextMeshProUGUI numberTMP = null;

            var tmps = bank.GetComponentsInChildren<TextMeshProUGUI>(true);
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

            if (numberTMP == null)
                return;

            int currentPage;
            if (!int.TryParse(numberTMP.text.Trim(), out currentPage))
                return;

            if (currentPage == lastPage)
                return;

            lastPage = currentPage;

            string customLabel = pageNameConfig.ContainsKey(currentPage) ? pageNameConfig[currentPage].Value : "";
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

                // ✅ Allow automatic ellipsis if too wide
                text.overflowMode = TextOverflowModes.Ellipsis;

                var rect = labelObject.GetComponent<RectTransform>();
                rect.anchorMin = numberTMP.rectTransform.anchorMin;
                rect.anchorMax = numberTMP.rectTransform.anchorMax;
                rect.anchoredPosition = numberTMP.rectTransform.anchoredPosition + new Vector2(0, 24); // position above tab number
                rect.sizeDelta = numberTMP.rectTransform.sizeDelta;
            }
            else
            {
                labelObject.SetActive(true);
            }

            var labelTMP = labelObject.GetComponent<TextMeshProUGUI>();
            labelTMP.text = customLabel;

            Logger.LogInfo($"✅ Updated label to: '{labelTMP.text}' for page {currentPage}");
        }
    }
}

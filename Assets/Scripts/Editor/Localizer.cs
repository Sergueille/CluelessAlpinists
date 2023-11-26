using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;

public static class Localizer
{
    [MenuItem("Custom/Localize everything!")]
    public static void Localize()
    {
        LocalizationManager.UpdateLanguage(LocalizationManager.Language.french);

        int localizedCount = 0;
        int failedCount = 0;

        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject go in objects)
        {
            if (PrefabUtility.GetPrefabType(go) == PrefabType.Prefab || PrefabUtility.GetPrefabType(go) == PrefabType.ModelPrefab)
                continue;

            TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
            LocalizedText localText = go.GetComponent<LocalizedText>();
            if (text != null && localText == null)
            {
                foreach (KeyValuePair<string, string> pair in LocalizationManager.currentDict)
                {
                    if (pair.Value.ToLower().Trim() == text.text.ToLower().Trim())
                    {
                        localText = go.AddComponent<LocalizedText>();
                        localText.key = pair.Key;
                        localText.enabled = true;
                        localizedCount++;
                        break;
                    }
                }

                if (failedCount == 0 || Random.Range(0, 5) == 0)
                EditorGUIUtility.PingObject(text);
                failedCount++;
            }
        }

        Debug.Log($"Localized {localizedCount} texts and failed {failedCount}");
    }

    [MenuItem("Custom/Cancel localization")]
    public static void CancelLocalization()
    {
        int removedCount = 0;

        GameObject[] objects = Object.FindObjectsOfType<GameObject>();
        foreach (GameObject go in objects)
        {
            LocalizedText localText = go.GetComponent<LocalizedText>();
            if (localText != null)
            {
                Object.DestroyImmediate(localText);
                removedCount++;
            }
        }

        Debug.Log($"Removed {removedCount}");
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using NamPhuThuy.Common;
using UnityEditor;

namespace NamPhuThuy.Data
{
    [CreateAssetMenu(fileName = "FoodData", menuName = "Game/FoodData")]
    public class FoodData : ScriptableObject
    {
        [SerializeField] private List<FoodRecord> allFoods;

        private Dictionary<FoodType, FoodRecord> _dictFoodData;

        public Dictionary<FoodType, FoodRecord> DictFoodData
        {
            get
            {
                if (_dictFoodData == null)
                {
                    EnsureDict();
                }

                return _dictFoodData;
            }
        }

        #region Callbacks

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (allFoods == null || allFoods.Count == 0)
                return;

            HashSet<FoodType> seenTypes = new HashSet<FoodType>();
            bool foundDuplicate = false;

            for (int i = 0; i < allFoods.Count; i++)
            {
                if (allFoods[i] == null)
                    continue;

                FoodType currentType = allFoods[i].type;

                if (seenTypes.Contains(currentType))
                {
                    DebugLogger.LogError(message: $"Duplicate FoodType '{currentType}', index {i}", context: this);
                    foundDuplicate = true;
                }
                else
                {
                    seenTypes.Add(currentType);
                }
            }

            if (foundDuplicate)
            {
                UnityEditor.EditorUtility.DisplayDialog(
                    "Duplicate FoodType Detected",
                    "One or more records have duplicate FoodType values. Please ensure each record has a unique FoodType.",
                    "OK"
                );
            }

            _dictFoodData = null;
        }
#endif

        #endregion

        #region Public Methods

        public FoodRecord GetFoodData(FoodType foodType)
        {
            EnsureDict();
            if (_dictFoodData == null) return null;
            return _dictFoodData.GetValueOrDefault(foodType);
        }

        public Sprite GetFoodSprite(FoodType foodType)
        {
            EnsureDict();
            Sprite sprite = null;

            if (_dictFoodData == null) return null;
            if (_dictFoodData.TryGetValue(foodType, out var record))
            {
                sprite = record.sprite;
            }

            return sprite;
        }

        public int GetTotalFoods()
        {
            return allFoods?.Count ?? 0;
        }

        #endregion

        #region Private Methods

        private void EnsureDict()
        {
            if (_dictFoodData != null) return;
            _dictFoodData = new Dictionary<FoodType, FoodRecord>(allFoods?.Count ?? 0);
            if (_dictFoodData == null) return;

            foreach (var food in allFoods)
            {
                if (food == null)
                {
                    DebugLogger.LogError(message: "Food record is null", context: this);
                    continue;
                }

                if (_dictFoodData.ContainsKey(food.type))
                {
                    DebugLogger.LogError(message: $"Duplicate FoodType: {food.type}", context: this);
                    continue;
                }

                _dictFoodData[food.type] = food;
            }
        }

        #endregion
    }

    [Serializable]
    public class FoodRecord
    {
        public FoodType type;
        public Sprite sprite;
        public string displayName;
        public int baseScore;
    }

    public enum FoodMechanic
    {
        NONE = 0,
        NORMAL = 1,
        ADD_TIME = 2,
        UNKNOW = 3,
    }

    public enum FoodType
    {
        NONE = 0,
        BACH_TUOC_NUONG = 1,
        BANH_BAO_NHO = 2,
        BANH_CAM = 3,
        BANH_GAO_TOKBOKKI = 4,
        BANH_RAN_DOREMON = 5,
        BANH_TIEU = 6,
        BANH_XEP = 7,
        BAP_CAI_CUON = 8,
        BBQ_KIEU_NHAT_YAKITORI = 9,
        BURRITO_MINI = 10,
        CAM = 11,
        CA_CHUA_BI = 12,
        CA_HOI_NUONG = 13,
        CA_VIEN_CHIEN = 14,
        CHANH_VANG = 15,
        CHANH_XANH = 16,
        CHURROS_MINI = 17,
        CUA_QUE = 18,
        DAO = 19,
        DAU_BAP = 20,
        DAU_HU = 21,
        DAU_HU_PHO_MAI = 22,
        DAU_QUE = 23,
        DAU_TAY = 24,
        DIMSUM_CHIEN = 25,
        DONUT_NHO = 26,
        DUA_HAU = 27,
        DUA_LEO_NUONG = 28,
        DUA_MIENG = 29,
        DUA_NUONG = 30,
        HANH_TAY = 31,
        HA_CAO = 32,
        HOTDOG_NHO = 33,
        HO_LO = 34,
        KEO_MARSHMALLOW = 35,
        KIWI = 36,
        KIWI_NUONG = 37,
        LUON_NUONG = 38,
        MOCHI_NHAT = 39,
        NAM_DONG_CO = 40,
        NAM_KIM_CHAM = 41,
        NAM_MO = 42,
        NAM_ROM = 43,
        NGO_NUONG = 44,
        NHO_TIM = 45,
        NHO_XANH = 46,
        OC_NUONG = 47,
        OI = 48,
        ORIGIRI_MINI = 49,
        OT_CHUONG = 50,
        OT_XANH_CAY = 51,
        QUYT = 52,
        RAU_CAI_BO_XOI = 53,
        SASHIMI = 54,
        SAU_RIENG = 55,
        SO_DIEP = 56,
        SUI_CAO = 57,
        SUSHI_NHAT = 58,
        TACO_MEXICO = 59,
        TAKOYAKI_XIEN = 60,
        TAO_NUONG = 61,
        THANH_LONG = 62,
        THIT_BO_NUONG_KIEU_XIEN_QUE_SAI_GON = 63,
        THIT_BO_VIEN = 64,
        TOI_NUONG = 65,
        TOM = 66,
        TOM_CHIEN_TEMPURA = 67,
        TRUNG_CHIM_CUT = 68,
        TRUNG_CUT_LON = 69,
        XIEN_OT_CHUONG = 70,
        XUC_XICH_HO_LO = 71,
    }

    // generate FoodMechanicPair
    [Serializable]
    public class FoodMechanicPair
    {
        public FoodMechanic mechanic;
        public int value;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(FoodData))]
    public class FoodDataEditor : Editor
    {
        private DefaultAsset _imageFolder;
        private bool _updateEnum = true;
        private bool _autoCreateMissingRecords = true;
        private float _pixelsPerUnit = 100f;

        // SECTION 2: Assign Sprites (new)
        private DefaultAsset _assignImageFolder;
        private bool _onlyFillMissing = true;
        private bool _updateDisplayName = true;
        private bool _forceImportSettings = true;
        private float _assignPixelsPerUnit = 100f;

        private readonly string[] _exts = { ".png", ".jpg", ".jpeg" };

        #region Callbacks

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            DrawGenerateSection();
            EditorGUILayout.Space(10);
            DrawAssignSection();
        }

        private void DrawGenerateSection()
        {
            EditorGUILayout.LabelField("Auto Generate From Folder", EditorStyles.boldLabel);
            _imageFolder =
                (DefaultAsset)EditorGUILayout.ObjectField("Image Folder", _imageFolder, typeof(DefaultAsset), false);
            _updateEnum = EditorGUILayout.Toggle("Append Missing Enum Values", _updateEnum);
            _autoCreateMissingRecords = EditorGUILayout.Toggle("Create / Update Records", _autoCreateMissingRecords);
            _pixelsPerUnit = EditorGUILayout.FloatField("Sprite Pixels Per Unit", _pixelsPerUnit);

            using (new EditorGUI.DisabledScope(_imageFolder == null))
            {
                if (GUILayout.Button("Sync From Folder"))
                    Sync();
            }
        }

        private void DrawAssignSection()
        {
            EditorGUILayout.LabelField("Assign Sprites From Folder", EditorStyles.boldLabel);
            _assignImageFolder =
                (DefaultAsset)EditorGUILayout.ObjectField("Image Folder", _assignImageFolder, typeof(DefaultAsset),
                    false);
            _onlyFillMissing = EditorGUILayout.Toggle("Only Fill Missing Sprites", _onlyFillMissing);
            _updateDisplayName = EditorGUILayout.Toggle("Update Display Name", _updateDisplayName);
            _forceImportSettings = EditorGUILayout.Toggle("Force Import Settings", _forceImportSettings);
            using (new EditorGUI.DisabledScope(!_forceImportSettings))
                _assignPixelsPerUnit = EditorGUILayout.FloatField("Pixels Per Unit", _assignPixelsPerUnit);

            using (new EditorGUI.DisabledScope(_assignImageFolder == null))
            {
                if (GUILayout.Button("Assign Sprites"))
                    AssignSprites();
            }
        }

        #endregion

        #region Sync FoodType enum with Sprites

        private void Sync()
        {
            var foodData = (FoodData)target;
            string folderPath = AssetDatabase.GetAssetPath(_imageFolder);
            if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError("Invalid folder.");
                return;
            }

            var imagePaths = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(p => _exts.Contains(Path.GetExtension(p).ToLowerInvariant()))
                .ToArray();

            if (imagePaths.Length == 0)
            {
                Debug.LogWarning("No images found.");
                return;
            }

            var wanted = new Dictionary<string, string>();
            foreach (var img in imagePaths)
            {
                var rawName = Path.GetFileNameWithoutExtension(img);
                var enumName = SanitizeEnumIdentifier(rawName);
                if (!string.IsNullOrEmpty(enumName))
                    wanted[enumName] = img;
            }
            
            var existingEnumNames = new HashSet<string>(Enum.GetNames(typeof(FoodType)));
            var missingEnumIds = wanted.Keys.Where(k => !existingEnumNames.Contains(k)).ToList();
            if (_updateEnum && missingEnumIds.Count > 0)
            {
                if (AppendEnumValues(missingEnumIds))
                {
                    Debug.Log($"Appended {missingEnumIds.Count} enum value(s) to FoodType. Recompile will occur; run Sync again to create records.");
                    AssetDatabase.Refresh();
                    return; // wait for domain reload
                }
                Debug.LogError("Failed to append enum values.");
                return;
            }

            var missing = wanted.Keys.Where(k => !existingEnumNames.Contains(k)).ToList();
            if (_updateEnum && missing.Count > 0)
            {
                if (AppendEnumValues(missing))
                {
                    Debug.Log(
                        $"Appended {missing.Count} enum value(s) to FoodType. Recompile will occur; run Sync again to link sprites.");
                    AssetDatabase.Refresh();
                    return; // Important: wait for domain reload before proceeding
                }

                Debug.LogError("Failed to append enum values.");
                return;
            }

            if (!_autoCreateMissingRecords) return;

            // Only create records for types that do not exist yet; never modify existing ones
            var recordsField = typeof(FoodData).GetField("allFoods", BindingFlags.NonPublic | BindingFlags.Instance);
            var list = recordsField.GetValue(foodData) as List<FoodRecord> ?? new List<FoodRecord>();
            var existingTypes = new HashSet<FoodType>(list.Where(r => r != null).Select(r => r.type));

            int created = 0;
            foreach (var kv in wanted)
            {
                if (!Enum.TryParse<FoodType>(kv.Key, out var type))
                    continue;

                if (existingTypes.Contains(type))
                    continue; // preserve existing record and sprite

                // Prepare sprite import for the new record
                EnsureSpriteImport(kv.Value, _pixelsPerUnit);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(kv.Value);

                list.Add(new FoodRecord
                {
                    type = type,
                    sprite = sprite,
                    displayName = type.ToString(),
                    baseScore = 0
                });

                existingTypes.Add(type);
                created++;
            }

            if (created > 0)
            {
                recordsField.SetValue(foodData, list); // no reordering, keep old items untouched
                EditorUtility.SetDirty(foodData);
                AssetDatabase.SaveAssets();
                Debug.Log($"FoodData records: created {created} new record(s). Total={list.Count}. Existing records not modified.");
            }
            else
            {
                Debug.Log("No new records to create. Existing records left untouched.");
            }
        }

        private bool AppendEnumValues(List<string> newIds)
        {
            var script = MonoScript.FromScriptableObject((ScriptableObject)target);
            var path = AssetDatabase.GetAssetPath(script);
            if (string.IsNullOrEmpty(path)) return false;

            string text = File.ReadAllText(path);
            int enumIndex = text.IndexOf("public enum FoodType", StringComparison.Ordinal);
            if (enumIndex < 0) return false;

            int braceStart = text.IndexOf('{', enumIndex);
            if (braceStart < 0) return false;

            // Find matching closing brace
            int i = braceStart + 1;
            int depth = 1;
            while (i < text.Length && depth > 0)
            {
                if (text[i] == '{') depth++;
                else if (text[i] == '}') depth--;
                i++;
            }

            if (depth != 0) return false;
            int braceEnd = i - 1;

            // Current enum body (between braces)
            string enumBody = text.Substring(braceStart + 1, braceEnd - braceStart - 1);

            // Build insertion: append one line at a time and update enumBody
            var sbInsert = new StringBuilder();
            foreach (var id in newIds)
            {
                int nextVal = FindNextEnumValue(enumBody);
                var line = $"        {id} = {nextVal},\n";
                sbInsert.Append(line);
                enumBody += line; // update body so next value increases
            }

            // Reconstruct file
            string newText = text.Substring(0, braceStart + 1) + enumBody + text.Substring(braceEnd);
            File.WriteAllText(path, newText, Encoding.UTF8);
            return true;
        }

        #endregion

        #region Assign Sprites

        private void AssignSprites()
        {
            var foodData = (FoodData)target;
            string folderPath = AssetDatabase.GetAssetPath(_assignImageFolder);
            if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogError("Invalid image folder.");
                return;
            }
            
            var list = EnsureCompleteRecordList(foodData);

            // Build quick lookup by type
            var byType = new Dictionary<FoodType, FoodRecord>();
            foreach (var r in list)
                if (r != null && !byType.ContainsKey(r.type))
                    byType[r.type] = r;

            // Collect images
            var imagePaths = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(p => _exts.Contains(Path.GetExtension(p).ToLowerInvariant()))
                .ToArray();

            if (imagePaths.Length == 0)
            {
                Debug.LogWarning("No images found.");
                return;
            }

            int assigned = 0;
            foreach (var imgPath in imagePaths)
            {
                var fileName = Path.GetFileNameWithoutExtension(imgPath);
                var enumName = SanitizeEnumIdentifier(fileName);
                if (string.IsNullOrEmpty(enumName)) continue;
                if (!Enum.TryParse(enumName, out FoodType type)) continue;

                if (!byType.TryGetValue(type, out var rec)) continue;
                if (_onlyFillMissing && rec.sprite != null) continue;

                if (_forceImportSettings)
                    EnsureSpriteImport(imgPath, _assignPixelsPerUnit);

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(imgPath);
                if (sprite == null) continue;

                rec.sprite = sprite;
                if (_updateDisplayName)
                    rec.displayName = fileName;

                assigned++;
            }

            if (assigned > 0)
            {
                EditorUtility.SetDirty(foodData);
                AssetDatabase.SaveAssets();
            }

            /*var imagePaths = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(p => _exts.Contains(Path.GetExtension(p).ToLowerInvariant()))
                .ToArray();
            if (imagePaths.Length == 0)
            {
                Debug.LogWarning("No images found.");
                return;
            }

            var recordsField = typeof(FoodData).GetField("allFoods",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var list = recordsField.GetValue(foodData) as List<FoodRecord>;
            if (list == null || list.Count == 0)
            {
                Debug.LogWarning("No food records to assign.");
                return;
            }

            var byType = new Dictionary<FoodType, FoodRecord>();
            foreach (var r in list)
            {
                if (r != null && !byType.ContainsKey(r.type))
                    byType[r.type] = r;
            }

            int assigned = 0;
            foreach (var imgPath in imagePaths)
            {
                var fileName = Path.GetFileNameWithoutExtension(imgPath);
                var enumName = SanitizeEnumIdentifier(fileName);
                if (string.IsNullOrEmpty(enumName)) continue;
                if (!Enum.TryParse(enumName, out FoodType type)) continue;
                if (!byType.TryGetValue(type, out var rec)) continue;
                if (_onlyFillMissing && rec.sprite != null) continue;

                if (_forceImportSettings)
                    EnsureSpriteImport(imgPath, _assignPixelsPerUnit);

                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(imgPath);
                if (sprite == null) continue;

                rec.sprite = sprite;
                if (_updateDisplayName)
                    rec.displayName = fileName;
                assigned++;
            }

            if (assigned > 0)
            {
                EditorUtility.SetDirty(foodData);
                AssetDatabase.SaveAssets();
            }*/

            Debug.Log($"Assigned {assigned} sprite(s).");
        }

        private List<FoodRecord> EnsureCompleteRecordList(FoodData foodData)
        {
            var recordsField = typeof(FoodData).GetField("allFoods", BindingFlags.NonPublic | BindingFlags.Instance);
            var list = recordsField.GetValue(foodData) as List<FoodRecord> ?? new List<FoodRecord>();

            // Remove nulls
            list = list.Where(r => r != null).ToList();

            // First occurrence per type
            var byType = new Dictionary<FoodType, FoodRecord>();
            foreach (var r in list)
            {
                if (!byType.ContainsKey(r.type))
                    byType[r.type] = r;
            }

            // Create missing records for every enum value
            foreach (FoodType t in Enum.GetValues(typeof(FoodType)))
            {
                if (!byType.ContainsKey(t))
                {
                    byType[t] = new FoodRecord
                    {
                        type = t,
                        displayName = t.ToString(),
                        baseScore = 0,
                        sprite = null
                    };
                }
            }

            // Order by enum numeric value
            var ordered = byType.Values.OrderBy(r => (int)r.type).ToList();
            recordsField.SetValue(foodData, ordered);
            return ordered;
        }
        #endregion

        #region Helpers

        private int FindNextEnumValue(string enumBody)
        {
            // Try explicit numeric assignments first
            int maxVal = -1;
            var lines = enumBody.Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line.Split(new[] { "//" }, StringSplitOptions.None)[0].Trim();
                int eq = trimmed.IndexOf('=');
                if (eq > 0)
                {
                    var numPart = trimmed.Substring(eq + 1).Trim().TrimEnd(',', ' ');
                    if (int.TryParse(numPart, out int val))
                        if (val > maxVal)
                            maxVal = val;
                }
            }

            if (maxVal >= 0) return maxVal + 1;

            // No explicit numbers: next is count of current enum entries
            int count = 0;
            foreach (var part in enumBody.Split(','))
            {
                var token = part.Split(new[] { "//" }, StringSplitOptions.None)[0].Trim();
                if (string.IsNullOrEmpty(token)) continue;
                // Take last line segment (handles multi-line formatting)
                var last = token.Split('\n').Last().Trim();
                if (string.IsNullOrEmpty(last)) continue;
                char c0 = last[0];
                if (char.IsLetter(c0) || c0 == '_') count++;
            }

            return count;
        }


        private string SanitizeEnumIdentifier(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            raw = raw.Trim().ToUpperInvariant();
            raw = raw.Replace(' ', '_').Replace('-', '_');
            var sb = new StringBuilder();
            foreach (var c in raw)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_')
                    sb.Append(c);
            }

            var s = sb.ToString();
            while (s.Contains("__")) s = s.Replace("__", "_");
            if (s.Length == 0) return null;
            if (char.IsDigit(s[0])) s = "_" + s;
            return s;
        }

        private void EnsureSpriteImport(string assetPath, float ppu)
        {
            var importer = (TextureImporter)TextureImporter.GetAtPath(assetPath);
            if (importer == null) return;
            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            if (Mathf.Abs(importer.spritePixelsPerUnit - ppu) > 0.01f)
            {
                importer.spritePixelsPerUnit = Mathf.Max(1f, ppu);
                changed = true;
            }

            if (changed) importer.SaveAndReimport();
        }

        #endregion
    }
#endif
}
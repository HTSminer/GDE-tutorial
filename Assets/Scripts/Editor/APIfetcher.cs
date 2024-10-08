using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace MrAmorphic
{
    public enum PokemonDataType
    { Pokemon, Species, Evolution, }

    public enum VersionGroup
    {
        latest, scarlet_violet, sword_shield, lets_go_pikachu_lets_go_eevee, ultra_sun_ultra_moon, sun_moon, omega_ruby_alpha_sapphire, x_y, black_2_white_2, xd, colosseum, black_white, heartgold_soulsilver, platinum, diamond_pearl, firered_leafgreen, emerald, ruby_sapphire, crystal, gold_silver, yellow, red_blue,
    }

    public enum Version
    {
        latest, scarlet, violet, shield, sword, lets_go_eevee, lets_go_pikachu, ultra_moon, ultra_sun, moon, sun, alpha_sapphire, omega_ruby, y, x, white_2, black_2, xd, colosseum, white, black, soulsilver, heartgold, platinum, pearl, diamond, leafgreen, firered, emerald, ruby, crystal, silver, gold, yellow, blue, red,
    }

    public enum Language
    {
        en, ja, ko, fr, de, es,
    }

    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => input.First().ToString().ToUpper() + input.Substring(1),
            };

        public static string InsertSpaceBeforeUpperCase(this string str)
        {
            var sb = new StringBuilder();

            char previousChar = char.MinValue; // Unicode '\0'

            foreach (char c in str)
            {
                if (char.IsUpper(c))
                {
                    // If not the first character and previous character is not a space, insert a space before uppercase
                    if (sb.Length != 0 && previousChar != ' ')
                    {
                        sb.Append(' ');
                    }
                }

                sb.Append(c);

                previousChar = c;
            }

            return sb.ToString();
        }
    }

    public class CoroutineWithData
    {
        public object result;
        private IEnumerator target;

        public CoroutineWithData(IEnumerator target)
        {
            this.target = target;
            PokeApi pokeApi = ScriptableObject.CreateInstance<PokeApi>();
            this.Coroutine = EditorCoroutineUtility.StartCoroutine(Run(), pokeApi);
        }

        public EditorCoroutine Coroutine { get; private set; }

        private IEnumerator Run()
        {
            while (target.MoveNext())
            {
                result = target.Current;
                yield return result;
            }
        }
    }

    public class PokeApiSettings : EditorWindow
    {
        private PokeApi pokeApi;
        private bool groupEnabled;

        [MenuItem("Window/MrAmorphic/PokeAPI Settings")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(PokeApiSettings));
        }

        private void OnGUI()
        {
            if (pokeApi == null)
                pokeApi = ScriptableObject.CreateInstance<PokeApi>();

            GUILayout.Label("Version Settings", EditorStyles.boldLabel);
            pokeApi.Version_group = (VersionGroup)EditorGUILayout.EnumPopup("Version Group", pokeApi.Version_group);
            pokeApi.Version = (Version)EditorGUILayout.EnumPopup("Version", pokeApi.Version);
            pokeApi.Language = (Language)EditorGUILayout.EnumPopup("Language", pokeApi.Language);
            if (GUILayout.Button("Create Folders", GUILayout.Height(20)))
            {
                PokeApi.CreateFolders();
            }
            GUILayout.Label("Items & Moves", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create Items", GUILayout.Height(30)))
            {
                EditorCoroutineUtility.StartCoroutine(pokeApi.GetItems(), this);
            }
            if (GUILayout.Button("Create Moves", GUILayout.Height(30)))
            {
                EditorCoroutineUtility.StartCoroutine(pokeApi.GetMoves(), this);
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Pokemons", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Gen 1", GUILayout.Height(30)))
            {
                EditorCoroutineUtility.StartCoroutine(pokeApi.GetPokemons(151, 1), this);
            }
            if (GUILayout.Button("Gen 2", GUILayout.Height(30)))
            {
                EditorCoroutineUtility.StartCoroutine(pokeApi.GetPokemons(100, 152), this);
            }
            if (GUILayout.Button("Gen 3", GUILayout.Height(30)))
            {
                EditorCoroutineUtility.StartCoroutine(pokeApi.GetPokemons(135, 252), this);
            }
            if (GUILayout.Button("Gen 4", GUILayout.Height(30)))
            {
                EditorCoroutineUtility.StartCoroutine(pokeApi.GetPokemons(107, 387), this);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Gen 5", GUILayout.Height(30)))
            {
                EditorCoroutineUtility.StartCoroutine(pokeApi.GetPokemons(156, 494), this);
            }
            if (GUILayout.Button("Gen 6", GUILayout.Height(30)))
            {
                EditorCoroutineUtility.StartCoroutine(pokeApi.GetPokemons(72, 650), this);
            }
            if (GUILayout.Button("Gen 7", GUILayout.Height(30)))
            {
                EditorCoroutineUtility.StartCoroutine(pokeApi.GetPokemons(88, 722), this);
            }
            if (GUILayout.Button("Gen 8", GUILayout.Height(30)))
            {
                EditorCoroutineUtility.StartCoroutine(pokeApi.GetPokemons(89, 810), this);
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("\n", EditorStyles.boldLabel);
            groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
            if (groupEnabled)
            {
                GUILayout.Label("Folder Paths", EditorStyles.boldLabel);
                pokeApi.PathToResources = EditorGUILayout.TextField("Resources", pokeApi.PathToResources);
                pokeApi.CacheFolder = EditorGUILayout.TextField("Cache", pokeApi.CacheFolder);
                pokeApi.PathToMoveAssets = EditorGUILayout.TextField("Moves", pokeApi.PathToMoveAssets);
                pokeApi.PathToPokemonAssets = EditorGUILayout.TextField("Pokemons", pokeApi.PathToPokemonAssets);
                pokeApi.PathToItemAssets = EditorGUILayout.TextField("Items", pokeApi.PathToItemAssets);
                GUILayout.Label("Item Subfolders", EditorStyles.boldLabel);
                pokeApi.PokeballsFolder = EditorGUILayout.TextField("Poke Balls", pokeApi.PokeballsFolder);
                pokeApi.HealingFolder = EditorGUILayout.TextField("Medicine", pokeApi.HealingFolder);
                pokeApi.ItemsFolder = EditorGUILayout.TextField("Items", pokeApi.ItemsFolder);
                pokeApi.BattleItemsFolder = EditorGUILayout.TextField("Battle Items", pokeApi.BattleItemsFolder);
                pokeApi.MachinesFolder = EditorGUILayout.TextField("Machines (TM&HM)", pokeApi.MachinesFolder);
                pokeApi.KeyItemsFolder = EditorGUILayout.TextField("Key Items", pokeApi.KeyItemsFolder);
                pokeApi.HeldItemsFolder = EditorGUILayout.TextField("Held Items", pokeApi.HeldItemsFolder);
                pokeApi.EvolutionItemsFolder = EditorGUILayout.TextField("Evolution Items", pokeApi.EvolutionItemsFolder);
            }
            EditorGUILayout.EndToggleGroup();
        }
    }

    [CustomEditor(typeof(MoveBase))]
    [ExecuteInEditMode]
    public class MoveBaseEditor : Editor
    {
        private MoveBase move;
        private PokeApi pokeApi;

        public override void OnInspectorGUI()
        {
            this.move = (MoveBase)this.target;

            if (this.move.Id > 0)
            {
                pokeApi = CreateInstance<PokeApi>();

                if (GUILayout.Button("Update Data", GUILayout.Height(40)))
                {
                    EditorCoroutineUtility.StartCoroutine(pokeApi.GetMoveData(this.move.Id), this);
                }

                pokeApi.Version_group = (VersionGroup)EditorGUILayout.EnumPopup("Version Group", pokeApi.Version_group);
                pokeApi.Version = (Version)EditorGUILayout.EnumPopup("Version", pokeApi.Version);

                base.OnInspectorGUI();

                if (GUILayout.Button("Clear Data", GUILayout.Height(40)))
                {
                    this.ClearMoveData();
                }
            }
            else
            {
                base.OnInspectorGUI();
            }
        }

        private void ClearMoveData()
        {
            this.move.Accuracy = 0;
            this.move.Description = string.Empty;
            this.move.Type = PokemonType.None;
            this.move.Power = 0;
            this.move.PP = 0;
            this.move.Name = string.Empty;
            this.move.Secondaries.Clear();
            this.move.Effects.Boosts.Clear();
            this.move.Effects.Status = ConditionID.none;
            this.move.Effects.VolatileStatus = ConditionID.none;
        }
    }

    [CustomEditor(typeof(PokemonBase))]
    [ExecuteInEditMode]
    public class PokemonBaseEditor : Editor
    {
        private PokemonBase pokemon;
        private PokeApi pokeApi;

        public override void OnInspectorGUI()
        {
            this.pokemon = (PokemonBase)this.target;

            if (this.pokemon.Id > 0)
            {
                pokeApi = CreateInstance<PokeApi>();

                if (GUILayout.Button("Update Data", GUILayout.Height(40)))
                {
                    EditorCoroutineUtility.StartCoroutine(pokeApi.GetPokemonData(this.pokemon.Id), this);
                }

                pokeApi.Version_group = (VersionGroup)EditorGUILayout.EnumPopup("Version Group", pokeApi.Version_group);
                pokeApi.Version = (Version)EditorGUILayout.EnumPopup("Version", pokeApi.Version);

                base.OnInspectorGUI();

                if (GUILayout.Button("Clear Data", GUILayout.Height(40)))
                {
                    this.ClearData();
                }
            }
            else
            {
                base.OnInspectorGUI();
            }
        }

        private void ClearData()
        {
            this.pokemon.ExpYield = 0;
            this.pokemon.Description = string.Empty;
            this.pokemon.Type1 = PokemonType.None;
            this.pokemon.Type2 = PokemonType.None;
            this.pokemon.CatchRate = 0;
            this.pokemon.GrowthRateId = GrowthRateID.medium;
            this.pokemon.LearnableMoves.Clear();
            this.pokemon.LearnableByItems.Clear();
            this.pokemon.Evolutions.Clear();
            this.pokemon.MaxHp = 0;
            this.pokemon.Attack = 0;
            this.pokemon.Defense = 0;
            this.pokemon.SpAttack = 0;
            this.pokemon.SpDefense = 0;
            this.pokemon.Speed = 0;
            this.pokemon.Name = string.Empty;
            this.pokemon.CatchRate = 0;
            this.pokemon.FrontSprite = null;
            this.pokemon.BackSprite = null;
        }
    }

    [CustomEditor(typeof(EvolutionItem))]
    [ExecuteInEditMode]
    public class EvolutionItemEditor : Editor
    {
        private EvolutionItem item;
        private PokeApi pokeApi;

        public override void OnInspectorGUI()
        {
            this.item = (EvolutionItem)this.target;
            if (this.item.Id > 0)
            {
                pokeApi = CreateInstance<PokeApi>();

                if (GUILayout.Button("Update Data", GUILayout.Height(40)))
                {
                    EditorCoroutineUtility.StartCoroutine(pokeApi.GetItemData(this.item.Id), this);
                }

                pokeApi.Version_group = (VersionGroup)EditorGUILayout.EnumPopup("Version Group", pokeApi.Version_group);
                pokeApi.Version = (Version)EditorGUILayout.EnumPopup("Version", pokeApi.Version);

                base.OnInspectorGUI();

                if (GUILayout.Button("Clear Data", GUILayout.Height(40)))
                {
                    this.ClearData();
                }
            }
            else
            {
                base.OnInspectorGUI();
            }
        }

        private void ClearData()
        {
            this.item.CanUseInBattle = false;
            this.item.CanUseOutsideBattle = false;
            this.item.Name = string.Empty;
        }
    }

    [CustomEditor(typeof(TmItem))]
    [ExecuteInEditMode]
    public class TmItemEditor : Editor
    {
        private TmItem item;
        private PokeApi pokeApi;

        public override void OnInspectorGUI()
        {
            this.item = (TmItem)this.target;
            if (this.item.Id > 0)
            {
                pokeApi = CreateInstance<PokeApi>();

                if (GUILayout.Button("Update Data", GUILayout.Height(40)))
                {
                    EditorCoroutineUtility.StartCoroutine(pokeApi.GetItemData(this.item.Id), this);
                }

                pokeApi.Version_group = (VersionGroup)EditorGUILayout.EnumPopup("Version Group", pokeApi.Version_group);
                pokeApi.Version = (Version)EditorGUILayout.EnumPopup("Version", pokeApi.Version);

                base.OnInspectorGUI();

                if (GUILayout.Button("Clear Data", GUILayout.Height(40)))
                {
                    this.ClearData();
                }
            }
            else
            {
                base.OnInspectorGUI();
            }
        }

        private void ClearData()
        {
            this.item.CanUseInBattle = false;
            this.item.CanUseOutsideBattle = false;
            this.item.Name = string.Empty;
        }
    }

    [CustomEditor(typeof(RecoveryItem))]
    [ExecuteInEditMode]
    public class RecoveryItemEditor : Editor
    {
        private RecoveryItem item;
        private PokeApi pokeApi;

        public override void OnInspectorGUI()
        {
            this.item = (RecoveryItem)this.target;
            if (this.item.Id > 0)
            {
                pokeApi = CreateInstance<PokeApi>();
                if (GUILayout.Button("Update Data", GUILayout.Height(40)))
                {
                    EditorCoroutineUtility.StartCoroutine(pokeApi.GetItemData(this.item.Id), this);
                }

                pokeApi.Version_group = (VersionGroup)EditorGUILayout.EnumPopup("Version Group", pokeApi.Version_group);
                pokeApi.Version = (Version)EditorGUILayout.EnumPopup("Version", pokeApi.Version);

                base.OnInspectorGUI();

                if (GUILayout.Button("Clear Data", GUILayout.Height(40)))
                {
                    this.ClearData();
                }
            }
            else
            {
                base.OnInspectorGUI();
            }
        }

        private void ClearData()
        {
            this.item.CanUseInBattle = false;
            this.item.CanUseOutsideBattle = false;
            this.item.Name = string.Empty;
        }
    }

    [CustomEditor(typeof(PokeballItem))]
    [ExecuteInEditMode]
    public class PokeballItemEditor : Editor
    {
        private PokeballItem item;
        private PokeApi pokeApi;

        public override void OnInspectorGUI()
        {
            this.item = (PokeballItem)this.target;
            if (this.item.Id > 0)
            {
                pokeApi = CreateInstance<PokeApi>();
                if (GUILayout.Button("Update Data", GUILayout.Height(40)))
                {
                    EditorCoroutineUtility.StartCoroutine(pokeApi.GetItemData(this.item.Id), this);
                }

                pokeApi.Version_group = (VersionGroup)EditorGUILayout.EnumPopup("Version Group", pokeApi.Version_group);
                pokeApi.Version = (Version)EditorGUILayout.EnumPopup("Version", pokeApi.Version);

                base.OnInspectorGUI();

                if (GUILayout.Button("Clear Data", GUILayout.Height(40)))
                {
                    this.ClearData();
                }
            }
            else
            {
                base.OnInspectorGUI();
            }
        }

        private void ClearData()
        {
            this.item.CanUseInBattle = false;
            this.item.CanUseOutsideBattle = false;
            this.item.Name = string.Empty;
        }
    }

    [CustomEditor(typeof(ItemBase))]
    [ExecuteInEditMode]
    public class ItemBaseEditor : Editor
    {
        private ItemBase item;
        private PokeApi pokeApi;

        public override void OnInspectorGUI()
        {
            this.item = (ItemBase)this.target;

            if (this.item.Id > 0)
            {
                pokeApi = CreateInstance<PokeApi>();
                if (GUILayout.Button("Update Data", GUILayout.Height(40)))
                {
                    EditorCoroutineUtility.StartCoroutine(pokeApi.GetItemData(this.item.Id), this);
                }

                pokeApi.Version_group = (VersionGroup)EditorGUILayout.EnumPopup("Version Group", pokeApi.Version_group);
                pokeApi.Version = (Version)EditorGUILayout.EnumPopup("Version", pokeApi.Version);

                base.OnInspectorGUI();

                if (GUILayout.Button("Clear Data", GUILayout.Height(40)))
                {
                    this.ClearData();
                }
            }
            else
            {
                base.OnInspectorGUI();
            }
        }

        private void ClearData()
        {
            this.item.CanUseInBattle = false;
            this.item.CanUseOutsideBattle = false;
            this.item.Name = string.Empty;
        }
    }

    public class PokeApi : Editor
    {
        private static PokeApi instance;
        private static bool fetchError = false;
        private static string pathToResources = "/Game/Resources/";
        private static string pathToMoveAssets = "Moves/Test/";
        private static string pathToItemAssets = "Items/Test/";
        private static string pathToPokemonAssets = "Pokemons/Test/";
        private static string cacheFolder = "_Cache/";
        private static string spritesFolder = "_Sprites/";
        private static string pokeballsFolder = "Poke Balls/";
        private static string healingFolder = "Medicine/";
        private static string itemsFolder = "Items/";
        private static string battleItemsFolder = "Battle Items/";
        private static string machinesFolder = "Machines (TM&HM)/";
        private static string keyItemsFolder = "Key Items/";
        private static string heldItemsFolder = "Held Items/";
        private static string evolutionItemsFolder = "Evolution Items/";
        private static Language language = Language.en;
        private static VersionGroup version_group = VersionGroup.latest;
        private static Version version = Version.latest;
        private static bool fetchSprites = true;
        private static bool createPokemonStubIfNotExistForEvolution = true;
        private static bool createItemStubIfNotExistForEvolution = true;
        private static bool createMoveStubIfNotExistForPokemonMoves = true;
        public VersionGroup Version_group { get => version_group; set => version_group = value; }
        public Version Version { get => version; set => version = value; }
        public string PathToResources { get => pathToResources; set => pathToResources = value; }
        public string PathToMoveAssets { get => pathToMoveAssets; set => pathToMoveAssets = value; }
        public string PathToItemAssets { get => pathToItemAssets; set => pathToItemAssets = value; }
        public string PathToPokemonAssets { get => pathToPokemonAssets; set => pathToPokemonAssets = value; }
        public string CacheFolder { get => cacheFolder; set => cacheFolder = value; }
        public string SpritesFolder { get => spritesFolder; set => spritesFolder = value; }
        public string PokeballsFolder { get => pokeballsFolder; set => pokeballsFolder = value; }
        public string HealingFolder { get => healingFolder; set => healingFolder = value; }
        public string ItemsFolder { get => itemsFolder; set => itemsFolder = value; }
        public string BattleItemsFolder { get => battleItemsFolder; set => battleItemsFolder = value; }
        public string MachinesFolder { get => machinesFolder; set => machinesFolder = value; }
        public string KeyItemsFolder { get => keyItemsFolder; set => keyItemsFolder = value; }
        public string HeldItemsFolder { get => heldItemsFolder; set => heldItemsFolder = value; }
        public string EvolutionItemsFolder { get => evolutionItemsFolder; set => evolutionItemsFolder = value; }
        public bool FetchSprites { get => fetchSprites; set => fetchSprites = value; }
        public bool CreatePokemonStubIfNotExistForEvolution { get => createPokemonStubIfNotExistForEvolution; set => createPokemonStubIfNotExistForEvolution = value; }
        public bool CreateItemStubIfNotExistForEvolution { get => createItemStubIfNotExistForEvolution; set => createItemStubIfNotExistForEvolution = value; }
        public bool CreateMoveStubIfNotExistForPokemonMoves { get => createMoveStubIfNotExistForPokemonMoves; set => createMoveStubIfNotExistForPokemonMoves = value; }
        public Language Language { get => language; set => language = value; }

        public static string FormatJson(string json, string indent = "    ")
        {
            var indentation = 0;
            var quoteCount = 0;
            var escapeCount = 0;

            var result =
                from ch in json ?? string.Empty
                let escaped = (ch == '\\' ? escapeCount++ : escapeCount > 0 ? escapeCount-- : escapeCount) > 0
                let quotes = ch == '"' && !escaped ? quoteCount++ : quoteCount
                let unquoted = quotes % 2 == 0
                let colon = ch == ':' && unquoted ? ": " : null
                let nospace = char.IsWhiteSpace(ch) && unquoted ? string.Empty : null
                let lineBreak = ch == ',' && unquoted ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(indent, indentation)) : null
                let openChar = (ch == '{' || ch == '[') && unquoted ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(indent, ++indentation)) : ch.ToString()
                let closeChar = (ch == '}' || ch == ']') && unquoted ? Environment.NewLine + string.Concat(Enumerable.Repeat(indent, --indentation)) + ch : ch.ToString()
                select colon ?? nospace ?? lineBreak ?? (
                    openChar.Length > 1 ? openChar : closeChar
                );

            return string.Concat(result);
        }

        [MenuItem("MrAmorphic/PokeAPI/Create Folders #&f")]
        public static void CreateFolders()
        {
            instance = CreateInstance<PokeApi>();
            instance.CreateItemFolders();
            instance.CreatePokemonFolders();
            instance.CreateMoveFolders();
        }

        public IEnumerator GetMoveData(int index)
        {
            string file = Application.dataPath + $"{pathToResources}{pathToMoveAssets}{cacheFolder}{index}.json";
            fetchError = false;

            if (File.Exists(file))
            {
                var data = File.ReadAllText(file);
                ProcessMoveData(data);
            }
            else
            {
                UnityWebRequest www = UnityWebRequest.Get("https://pokeapi.co/api/v2/move/" + index);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    fetchError = true;
                }
                else
                {
                    var data = www.downloadHandler.text;
                    File.WriteAllText(file, FormatJson(data));
                    ProcessMoveData(data);
                }

                yield return new WaitForEndOfFrame();
            }
        }

        public IEnumerator GetItemData(int index)
        {
            string file = Application.dataPath + $"{pathToResources}{pathToItemAssets}{cacheFolder}{index}.json";
            fetchError = false;

            if (File.Exists(file))
            {
                var data = File.ReadAllText(file);
                yield return ProcessItemData(data);
            }
            else
            {
                UnityWebRequest www = UnityWebRequest.Get("https://pokeapi.co/api/v2/item/" + index);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    fetchError = true;
                }
                else
                {
                    var data = www.downloadHandler.text;
                    File.WriteAllText(file, FormatJson(data));
                    yield return ProcessItemData(data);
                }

                yield return new WaitForEndOfFrame();
            }
        }

        public IEnumerator GetPokemonSubData(string url, PokemonDataType dataType)
        {
            fetchError = false;

            var index = Int32.Parse(url.TrimEnd('/').Split('/').Last());

            string folder = dataType switch
            {
                PokemonDataType.Species => "Species/",
                PokemonDataType.Evolution => "Evolution/",
                _ => string.Empty,
            };

            string file = Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{cacheFolder}{folder}{index}.json";

            if (File.Exists(file))
            {
                var data = File.ReadAllText(file);
                yield return data;
            }
            else
            {
                UnityWebRequest www = UnityWebRequest.Get(url);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    fetchError = true;
                }
                else
                {
                    var data = www.downloadHandler.text;
                    File.WriteAllText(file, FormatJson(data));
                    yield return data;
                }
            }
        }

        public IEnumerator GetPokemonData(int index)
        {
            string file = Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{cacheFolder}Pokemon/{index}.json";
            fetchError = false;

            if (File.Exists(file))
            {
                var data = File.ReadAllText(file);
                yield return ProcessPokemonData(data);
            }
            else
            {
                UnityWebRequest www = UnityWebRequest.Get("https://pokeapi.co/api/v2/pokemon/" + index);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    fetchError = true;
                }
                else
                {
                    var data = www.downloadHandler.text;
                    File.WriteAllText(file, FormatJson(data));
                    yield return ProcessPokemonData(data);
                }

                yield return new WaitForEndOfFrame();
            }
        }

        public IEnumerator GetItems(int count = 1700, int start = 1)
        {
            DateTime time = DateTime.Now;
            int countFetched = 0;
            CreateItemFolders();
            instance = CreateInstance<PokeApi>();

            for (int i = start; i < (start + count); i++)
            {
                yield return instance.GetItemData(i);

                if (fetchError)
                {
                    Debug.Log($"Item with ID {i} not found in API. Skipping.");
                    continue;
                }

                countFetched++;
            }

            TimeSpan t = DateTime.Now.Subtract(time);
            string answer = string.Format("{0:D2}m:{1:D2}s", t.Minutes, t.Seconds);

            Debug.Log($"{countFetched} items fetched in {answer}.");
        }

        public IEnumerator FetchEverythingCO()
        {
            DateTime time = DateTime.Now;
            yield return GetMoves();
            yield return GetItems();
            yield return GetPokemons();
            AssetDatabase.Refresh();
            TimeSpan t = DateTime.Now.Subtract(time);
            string answer = string.Format("{0:D2}m:{1:D2}s", t.Minutes, t.Seconds);

            Debug.Log($"Fetched everything in {answer}.");
        }

        public IEnumerator GetMoves(int count = 826, int start = 1)
        {
            DateTime time = DateTime.Now;

            CreateMoveFolders();
            instance = CreateInstance<PokeApi>();
            int countFetched = 0;
            for (int i = start; i < (start + count); i++)
            {
                yield return instance.GetMoveData(i);

                if (fetchError)
                    break;

                countFetched++;
            }

            TimeSpan t = DateTime.Now.Subtract(time);
            string answer = string.Format("{0:D2}m:{1:D2}s", t.Minutes, t.Seconds);

            Debug.Log($"{countFetched} moves fetched in {answer}.");
        }

        public IEnumerator GetPokemons(int count = 898, int start = 1)
        {
            DateTime time = DateTime.Now;

            CreatePokemonFolders();
            instance = CreateInstance<PokeApi>();

            if (createItemStubIfNotExistForEvolution)
                CreateItemFolders();

            if (createMoveStubIfNotExistForPokemonMoves)
                CreateMoveFolders();

            int countFetched = 0;
            for (int i = start; i < (start + count); i++)
            {
                yield return instance.GetPokemonData(i);

                if (fetchError)
                    break;

                countFetched++;
            }

            TimeSpan t = DateTime.Now.Subtract(time);
            string answer = string.Format("{0:D2}m:{1:D2}s", t.Minutes, t.Seconds);

            Debug.Log($"{countFetched} pokemons fetched in {answer}.");
        }

        [MenuItem("MrAmorphic/PokeAPI/ALL/Fetch All")]
        private static void FetchEverything()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.FetchEverythingCO(), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Moves/Fetch All Moves #&m")]
        private static void FetchAllMoves()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetMoves(), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Items/Fetch All Items #&i")]
        private static void FetchAllItems()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetItems(), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Pokemons/Fetch All Pokemons #&p")]
        private static void FetchAllPokemons()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetPokemons(), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Pokemons/Fetch Gen 1 #&1")]
        private static void FetchAllPokemonsGen1()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetPokemons(151, 1), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Pokemons/Fetch Gen 2 #&2")]
        private static void FetchAllPokemonsGen2()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetPokemons(100, 152), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Pokemons/Fetch Gen 3 #&3")]
        private static void FetchAllPokemonsGen3()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetPokemons(135, 252), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Pokemons/Fetch Gen 4 #&4")]
        private static void FetchAllPokemonsGen4()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetPokemons(107, 387), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Pokemons/Fetch Gen 5 #&5")]
        private static void FetchAllPokemonsGen5()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetPokemons(156, 494), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Pokemons/Fetch Gen 6 #&6")]
        private static void FetchAllPokemonsGen6()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetPokemons(72, 650), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Pokemons/Fetch Gen 7 #&7")]
        private static void FetchAllPokemonsGen7()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetPokemons(88, 722), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Pokemons/Fetch Gen 8 #&8")]
        private static void FetchAllPokemonsGen8()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetPokemons(89, 810), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Pokemons/Fetch Gen 9 #&8")]
        private static void FetchAllPokemonsGen9()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            EditorCoroutineUtility.StartCoroutine(instance.GetPokemons(120, 906), instance);
        }

        [MenuItem("MrAmorphic/PokeAPI/Moves/Set TM Moves")]
        private static void TMs()
        {
            ClearConsole();
            instance = CreateInstance<PokeApi>();
            instance.SetTMMoves();
        }

        private static void ClearConsole()
        {
            var assembly = Assembly.GetAssembly(typeof(SceneView));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }

        private IEnumerator DownloadSprite(string url, string fileToSave)
        {
            if (url?.Length == 0)
                yield break;

            if (!File.Exists(fileToSave))
            {
                using WebClient client = new WebClient();
                client.DownloadFile(new Uri(url), fileToSave);
            }

            yield return new WaitForEndOfFrame();
        }

        private void CreateItemFolders()
        {
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}{cacheFolder}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}{pokeballsFolder}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}{healingFolder}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}{itemsFolder}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}{battleItemsFolder}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}{machinesFolder}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}{keyItemsFolder}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}{heldItemsFolder}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}{evolutionItemsFolder}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToItemAssets}{spritesFolder}");
        }

        private void CreatePokemonFolders()
        {
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToPokemonAssets}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{cacheFolder}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{cacheFolder}Pokemon/");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{cacheFolder}Species/");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{cacheFolder}Evolution/");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{spritesFolder}");
        }

        private void CreateMoveFolders()
        {
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToMoveAssets}");
            Directory.CreateDirectory(Application.dataPath + $"{pathToResources}{pathToMoveAssets}{cacheFolder}");
        }

        private IEnumerator ProcessItemData(string data)
        {
            // Replace is a hack because "default" can't be used.
            PokeApiItem item = JsonUtility.FromJson<PokeApiItem>(data.Replace("\"default\":", "\"defaul\":"));

            List<string> categoryPokeball = new List<string>() { "standard-balls", "special-balls", "apricorn-balls" };
            List<string> categoryMedicine = new List<string>() { "healing", "status-cures", "revival", "pp-recovery", "vitamins", "flutes", "medicine", "picky-healing", "baking-only", "effort-drop", "type-protection", "in-a-pinch", "other", };
            List<string> categoryItem = new List<string>() { "stat-boosts", "spelunking", "collectibles", "loot", "dex-completion", "mulch", "all-mail", "species-specific", "apricorn-box", "data-cards", };
            List<string> categoryEvolution = new List<string>() { "evolution", "mega-stones", };
            List<string> categoryHeld = new List<string>() { "held-items", "choice", "type-enhancement", "effort-training", "training", "scarves", "bad-held-items", "plates", "jewels", };
            List<string> categoryMachines = new List<string>() { "all-machines", };
            List<string> categoryKey = new List<string>() { "gameplay", "plot-advancement", "event-items", };
            List<string> categoryBattle = new List<string>() { "miracle-shooter", };
            List<string> categoryUnused = new List<string>() { "unused", };

            // Abort for unused items
            if (categoryUnused.Contains(item.category.name))
            {
                yield break;
            }

            ItemBase itemToAdd;
            bool isNew = false;

            var folder = item.category.name switch
            {
                var x when categoryPokeball.Contains(x) => pokeballsFolder,
                var x when categoryMedicine.Contains(x) => healingFolder,
                var x when categoryItem.Contains(x) => itemsFolder,
                var x when categoryEvolution.Contains(x) => evolutionItemsFolder,
                var x when categoryHeld.Contains(x) => heldItemsFolder,
                var x when categoryMachines.Contains(x) => machinesFolder,
                var x when categoryKey.Contains(x) => keyItemsFolder,
                var x when categoryBattle.Contains(x) => battleItemsFolder,
                _ => itemsFolder,
            };

            if (File.Exists(Application.dataPath + $"{pathToResources}{pathToItemAssets}{folder}{item.name}.asset"))
            {
                itemToAdd = AssetDatabase.LoadAssetAtPath<ItemBase>($"Assets/{pathToResources}{pathToItemAssets}{folder}{item.name}.asset");

                yield return new WaitForEndOfFrame();
                if (itemToAdd == null)
                {
                    Debug.Log("File failed to load");
                }
                yield return new WaitForEndOfFrame();

                EditorUtility.SetDirty(itemToAdd);
            }
            else
            {
                isNew = true;

                itemToAdd = item.category.name switch
                {
                    var x when categoryPokeball.Contains(x) => CreateInstance<PokeballItem>(),
                    var x when categoryMedicine.Contains(x) => CreateInstance<RecoveryItem>(),
                    var x when categoryItem.Contains(x) => CreateInstance<ItemBase>(),
                    var x when categoryEvolution.Contains(x) => CreateInstance<EvolutionItem>(),
                    var x when categoryHeld.Contains(x) => CreateInstance<ItemBase>(),
                    var x when categoryMachines.Contains(x) => CreateInstance<TmItem>(),
                    var x when categoryKey.Contains(x) => CreateInstance<ItemBase>(),
                    var x when categoryBattle.Contains(x) => CreateInstance<ItemBase>(),
                    _ => null,
                };
            }

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = itemToAdd;

            if (itemToAdd == null)
            {
                Debug.LogWarning($"Category {item.category.name} not found for item {item.name}");
                yield break;
            }

            itemToAdd.Name = item.names.First(n => n.language.name == language.ToString().Replace("_", "-")).name;

            if (fetchSprites)
            {
                if (!File.Exists(Application.dataPath + $"{pathToResources}{pathToItemAssets}{spritesFolder}{item.name}.png"))
                {
                    yield return instance.DownloadSprite(item.sprites.defaul, Application.dataPath + $"{pathToResources}{pathToItemAssets}{spritesFolder}{item.name}.png");
                    yield return new WaitForSeconds(0.15f);
                    AssetDatabase.Refresh();
                }

                yield return null;

                Sprite ic = Resources.Load<Sprite>($"{pathToItemAssets}{spritesFolder}{item.name}");
                itemToAdd.Icon = ic;
            }

            itemToAdd.Description = item.flavor_text_entries switch
            {
                var x when x.Count(n => n.language.name == language.ToString().Replace("_", "-") && n.version_group.name == version_group.ToString().Replace("_", "-")) > 0 => x.First(n => n.language.name == language.ToString().Replace("_", "-") && n.version_group.name == version_group.ToString().Replace("_", "-")).text,
                var x when x.Count(n => n.language.name == language.ToString().Replace("_", "-")) > 0 => x.First(n => n.language.name == language.ToString().Replace("_", "-")).text,
                _ => string.Empty,
            };

            itemToAdd.Cost = item.cost;
            itemToAdd.Id = item.id;

            if (item.attributes.Any(a => a.name == "usable-in-battle"))
            {
                itemToAdd.CanUseInBattle = true;
            }

            if (item.attributes.Any(a => a.name == "usable-overworld"))
            {
                itemToAdd.CanUseOutsideBattle = true;
            }

            itemToAdd.PokeApiItem = item;

            EditorUtility.FocusProjectWindow();

            if (isNew)
                AssetDatabase.CreateAsset(itemToAdd, $"Assets/{pathToResources}{pathToItemAssets}{folder}{item.name}.asset");

            AssetDatabase.SaveAssets();
        }

        private void ProcessMoveData(string data)
        {
            PokeApiMove move = JsonUtility.FromJson<PokeApiMove>(data);

            MoveBase moveToAdd;
            bool isNew = false;

            if (File.Exists(Application.dataPath + $"{pathToResources}{pathToMoveAssets}{move.name}.asset"))
            {
                moveToAdd = AssetDatabase.LoadAssetAtPath<MoveBase>($"Assets/{pathToResources}{pathToMoveAssets}{move.name}.asset");
                EditorUtility.SetDirty(moveToAdd);
            }
            else
            {
                isNew = true;
                moveToAdd = CreateInstance<MoveBase>();
            }

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = moveToAdd;

            moveToAdd.Id = move.id;
            moveToAdd.Name = move.names.First(n => n.language.name == language.ToString().Replace("_", "-")).name;
            moveToAdd.Accuracy = move.accuracy;
            moveToAdd.Description = move.flavor_text_entries switch
            {
                var x when x.Count(n => n.language.name == language.ToString().Replace("_", "-") && n.version_group.name == version_group.ToString().Replace("_", "-")) > 0 => x.First(n => n.language.name == language.ToString().Replace("_", "-") && n.version_group.name == version_group.ToString().Replace("_", "-")).flavor_text,
                var x when x.Count(n => n.language.name == language.ToString().Replace("_", "-")) > 0 => x.First(n => n.language.name == language.ToString().Replace("_", "-")).flavor_text,
                _ => string.Empty,
            };
            moveToAdd.Power = move.power;
            moveToAdd.PP = move.pp;
            moveToAdd.Priority = move.priority;
            moveToAdd.Type = (PokemonType)Enum.Parse(typeof(PokemonType), move.type.name.FirstCharToUpper());
            moveToAdd.Category = (MoveCategory)Enum.Parse(typeof(MoveCategory), move.damage_class.name.FirstCharToUpper());
            moveToAdd.Effects = new MoveEffects();
            moveToAdd.Effects.Boosts = new List<StatBoost>();
            moveToAdd.Secondaries = new List<SecondaryEffects>();

            foreach (var stat_change in move.stat_changes)
            {
                StatBoost effectToAdd = new StatBoost();
                effectToAdd.stat = (Stat)Enum.Parse(typeof(Stat), stat_change.stat.name.FirstCharToUpper().Replace("-", "_"));
                effectToAdd.boost = stat_change.change;
                moveToAdd.Effects.Boosts.Add(effectToAdd);
            }

            if (move.meta.ailment.name != "none")
            {
                SecondaryEffects effects = new SecondaryEffects();
                effects.Status = (ConditionID)Enum.Parse(typeof(ConditionID), move.meta.ailment.name.Replace("-", "_"));
                effects.Chance = move.meta.ailment_chance;
                moveToAdd.Secondaries.Add(effects);
            }

            moveToAdd.PokeApiMove = move;

            if (isNew)
                AssetDatabase.CreateAsset(moveToAdd, $"Assets/{pathToResources}{pathToMoveAssets}{move.name}.asset");

            AssetDatabase.SaveAssets();
        }

        private IEnumerator ProcessPokemonData(string data)
        {
            PokeApiPokemon pokemon = JsonUtility.FromJson<PokeApiPokemon>(data);
            PokemonBase pokemonToAdd;
            bool isNew = false;

            if (File.Exists(Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{pokemon.name}.asset"))
            {
                pokemonToAdd = AssetDatabase.LoadAssetAtPath<PokemonBase>($"Assets/{pathToResources}{pathToPokemonAssets}{pokemon.name}.asset");
                EditorUtility.SetDirty(pokemonToAdd);
            }
            else
            {
                isNew = true;
                pokemonToAdd = CreateInstance<PokemonBase>();
            }

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = pokemonToAdd;

            CoroutineWithData speciesCO = new CoroutineWithData(GetPokemonSubData(pokemon.species.url, PokemonDataType.Species));
            yield return speciesCO.Coroutine;
            PokeApiSpecies species = JsonUtility.FromJson<PokeApiSpecies>(speciesCO.result.ToString());
            pokemon.species_data = species;

            CoroutineWithData evolutionCO = new CoroutineWithData(GetPokemonSubData(species.evolution_chain.url, PokemonDataType.Evolution));
            yield return evolutionCO.Coroutine;
            PokeApiEvolution evolutions = JsonUtility.FromJson<PokeApiEvolution>(evolutionCO.result.ToString());
            species.evolution_data = evolutions;

            
            // General Info -----------------------------------------------------------------------------------
            pokemonToAdd.Name = Array.Find(species.names, n => n.language.name == language.ToString().Replace("_", "-")).name;
            pokemonToAdd.Id = pokemon.id;
            pokemonToAdd.Height = pokemon.height;
            pokemonToAdd.Weight = pokemon.weight;
            pokemonToAdd.IsBaby = species.is_baby;
            pokemonToAdd.IsLegendary = species.is_legendary;
            pokemonToAdd.IsMythical = species.is_mythical;
            pokemonToAdd.CatchRate = species.capture_rate;

            // Description --------------------------------------------------------------------------------------
            pokemonToAdd.Description = string.Empty;

            if (version == Version.latest)
            {
                foreach (var ver in Enum.GetValues(typeof(Version)))
                {
                    var str = ver.ToString().Replace("_", "-");

                    if (str == "latest")
                        continue;

                    if (species.flavor_text_entries.Count(n => n.language.name == language.ToString().Replace("_", "-") && n.version.name == str) > 0)
                    {
                        pokemonToAdd.Description = species.flavor_text_entries.First(n => n.language.name == language.ToString().Replace("_", "-") && n.version.name == str).flavor_text;
                        break;
                    }
                }
            }
            else
            {
                if (species.flavor_text_entries.Count(n => n.language.name == language.ToString().Replace("_", "-") && n.version.name == version.ToString().Replace("_", "-")) > 0)
                {
                    pokemonToAdd.Description = species.flavor_text_entries.First(n => n.language.name == language.ToString().Replace("_", "-") && n.version.name == version.ToString().Replace("_", "-")).flavor_text;
                }
            }

            // Abilities ----------------------------------------------------------------------------------------------
            if (pokemonToAdd.Abilities == null)
                pokemonToAdd.Abilities = new List<AbilityInfo>();

            pokemonToAdd.Abilities.Clear();

            foreach (var a in pokemon.abilities) 
            {
                string originalName = a.ability.name;
                string replaceName = originalName.Replace('-', '_');

                if (Enum.TryParse(replaceName, true, out AbilityID abilityID))
                {
                    pokemonToAdd.Abilities.Add(new AbilityInfo
                    {
                        Ability = abilityID,
                        IsHidden = a.is_hidden
                    });
                }
            }

            // Gender Ratio -------------------------------------------------------------------------------------------
            pokemonToAdd.GenderRatio = species.gender_rate switch
            {
                -1 => PokemonGenderRatio.Genderless,
                0 => PokemonGenderRatio.MaleOnly,
                1 => PokemonGenderRatio.OneInEightFemale,
                2 => PokemonGenderRatio.OneInFourFemale,
                3 => PokemonGenderRatio.SevenInEightFemale,
                4 => PokemonGenderRatio.OneInTwoFemale,
                6 => PokemonGenderRatio.ThreeInFourFemale,
                7 => PokemonGenderRatio.Ditto,
                8 => PokemonGenderRatio.FemaleOnly,
                _ => throw new ArgumentOutOfRangeException($"Unexpected get rate value: {pokemon.species_data.gender_rate}")
            };

            // HeldItems -----------------------------------------------------------------------------------------------

            foreach (var itemSlot in pokemon.held_items)
            {
                Debug.Log(itemSlot.item.name);

                var heldItem = ItemDB.GetObjectByName(itemSlot.item.name);
                var version = itemSlot.version_details;

                for (int i = 0; i < version.Length; i++)
                {
                    if (version[i].rarity == 5)
                        pokemonToAdd.SItem = heldItem;
                    else if (version[i].rarity == 50)
                        pokemonToAdd.RItem = heldItem;
                    else if (version[i].rarity == 100)
                        pokemonToAdd.CItem = heldItem;
                }
            }

            // Experience & EVs -----------------------------------------------------------------------------------------
            pokemonToAdd.ExpYield = pokemon.base_experience;

            string growthRate = species.growth_rate.name
                .Replace("slow-then-very-fast", "erratic")
                .Replace("fast-then-very-slow", "fluctuating")
                .Replace('-', '_');
            pokemonToAdd.GrowthRateId = (GrowthRateID)Enum.Parse(typeof(GrowthRateID), growthRate);

            foreach (var stat in pokemon.stats)
            {
                switch (stat.stat.name)
                {
                    case "hp":
                        pokemonToAdd.MaxHp = stat.base_stat;
                        //pokemonToAdd.HP_EV = stat.effort;
                        break;

                    case "attack":
                        pokemonToAdd.Attack = stat.base_stat;
                        //pokemonToAdd.ATK_EV = stat.effort;
                        break;

                    case "defense":
                        pokemonToAdd.Defense = stat.base_stat;
                        //pokemonToAdd.DEF_EV = stat.effort;
                        break;

                    case "special-attack":
                        pokemonToAdd.SpAttack = stat.base_stat;
                        //pokemonToAdd.SPD_EV = stat.effort;
                        break;

                    case "special-defense":
                        pokemonToAdd.SpDefense = stat.base_stat;
                        //pokemonToAdd.SPA_EV = stat.effort;
                        break;

                    case "speed":
                        pokemonToAdd.Speed = stat.base_stat;
                        //pokemonToAdd.SPE_EV = stat.effort;
                        break;
                }
            }

            // Types -----------------------------------------------------------------------------------------------
            foreach (var type in pokemon.types)
            {
                switch (type.slot)
                {
                    case 1:
                        pokemonToAdd.Type1 = (PokemonType)Enum.Parse(typeof(PokemonType), type.type.name.FirstCharToUpper());
                        break;

                    case 2:
                        pokemonToAdd.Type2 = (PokemonType)Enum.Parse(typeof(PokemonType), type.type.name.FirstCharToUpper());
                        break;
                }
            }

            // Breeding ---------------------------------------------------------------------------------------------
            pokemonToAdd.EggCycles = species.hatch_counter;

            if (pokemonToAdd.EggGroup == null)
                pokemonToAdd.EggGroup = new List<EggGroups>();

            pokemonToAdd.EggGroup.Clear();
            foreach (var group in species.egg_groups)
            {
                var s = group.name.Replace('-', '_');
                pokemonToAdd.EggGroup.Add((EggGroups)Enum.Parse(typeof(EggGroups), s));
            }
            

            // Moves ------------------------------------------------------------------------------------------------
            pokemonToAdd.LearnableByItems = new List<MoveBase>();
            pokemonToAdd.LearnableMoves = new List<LearnableMoves>();
            pokemonToAdd.LearnableByBreeding = new List<MoveBase>();

            foreach (var moves in pokemon.moves)
            {
                foreach (var details in moves.version_group_details)
                {
                    if (details.move_learn_method.name == "machine")
                    {
                        if (version_group == VersionGroup.latest)
                        {
                            foreach (var ver in Enum.GetValues(typeof(VersionGroup)))
                            {
                                var str = ver.ToString().Replace("_", "-");

                                if (str == "latest")
                                    continue;

                                if (details.version_group.name == str)
                                {
                                    MoveBase move = Resources.Load<MoveBase>($"{pathToMoveAssets}{moves.move.name}");

                                    if (move == null && createMoveStubIfNotExistForPokemonMoves && moves.move.name?.Length > 0)
                                    {
                                        MoveBase moveBase = CreateInstance<MoveBase>();
                                        AssetDatabase.CreateAsset(moveBase, $"Assets{pathToResources}{pathToMoveAssets}{moves.move.name}.asset");
                                        yield return null;
                                        move = Resources.Load<MoveBase>($"{pathToMoveAssets}{moves.move.name}");
                                    }

                                    if (move != null && pokemonToAdd.LearnableByItems.Count(m => m == move) == 0)
                                    {
                                        pokemonToAdd.LearnableByItems.Add(move);
                                        break;
                                    }
                                }
                            }
                        }
                        else if (details.version_group.name == version_group.ToString().Replace("_", "-"))
                        {
                            MoveBase move = Resources.Load<MoveBase>($"{pathToMoveAssets}{moves.move.name}");

                            if (move == null && createMoveStubIfNotExistForPokemonMoves && moves.move.name?.Length > 0)
                            {
                                MoveBase moveBase = CreateInstance<MoveBase>();
                                AssetDatabase.CreateAsset(moveBase, $"Assets{pathToResources}{pathToMoveAssets}{moves.move.name}.asset");
                                yield return null;
                                move = Resources.Load<MoveBase>($"{pathToMoveAssets}{moves.move.name}");
                            }

                            if (move != null && pokemonToAdd.LearnableByItems.Count(m => m == move) == 0)
                            {
                                pokemonToAdd.LearnableByItems.Add(move);
                                break;
                            }
                        }
                    }
                    else if (details.move_learn_method.name == "level-up")
                    {
                        if (version_group == VersionGroup.latest)
                        {
                            foreach (var ver in Enum.GetValues(typeof(VersionGroup)))
                            {
                                var str = ver.ToString().Replace("_", "-");

                                if (str == "latest")
                                    continue;

                                if (details.version_group.name == str)
                                {
                                    MoveBase move = Resources.Load<MoveBase>($"{pathToMoveAssets}{moves.move.name}");

                                    if (move == null && createMoveStubIfNotExistForPokemonMoves && moves.move.name?.Length > 0)
                                    {
                                        MoveBase moveBase = CreateInstance<MoveBase>();
                                        AssetDatabase.CreateAsset(moveBase, $"Assets{pathToResources}{pathToMoveAssets}{moves.move.name}.asset");
                                        yield return null;
                                        move = Resources.Load<MoveBase>($"{pathToMoveAssets}{moves.move.name}");
                                    }

                                    if (move != null && pokemonToAdd.LearnableMoves.Count(m => m.Base == move) == 0)
                                    {
                                        var lm = new LearnableMoves
                                        {
                                            Base = move,
                                            Level = details.level_learned_at
                                        };

                                        pokemonToAdd.LearnableMoves.Add(lm);
                                        break;
                                    }
                                }
                            }
                        }
                        else if (details.version_group.name == version_group.ToString().Replace("_", "-"))
                        {
                            MoveBase move = Resources.Load<MoveBase>($"{pathToMoveAssets}{moves.move.name}");

                            if (move == null && createMoveStubIfNotExistForPokemonMoves && moves.move.name?.Length > 0)
                            {
                                MoveBase moveBase = CreateInstance<MoveBase>();
                                AssetDatabase.CreateAsset(moveBase, $"Assets{pathToResources}{pathToMoveAssets}{moves.move.name}.asset");
                                yield return null;
                                move = Resources.Load<MoveBase>($"{pathToMoveAssets}{moves.move.name}");
                            }

                            if (move != null && pokemonToAdd.LearnableMoves.Count(m => m.Base == move && m.Level == details.level_learned_at) == 0)
                            {
                                var lm = new LearnableMoves
                                {
                                    Base = move,
                                    Level = details.level_learned_at
                                };

                                pokemonToAdd.LearnableMoves.Add(lm);
                            }
                            break;
                        }
                    }
                    else if (details.move_learn_method.name == "egg")
                    {
                        if (version_group == VersionGroup.latest)
                        {
                            foreach (var ver in Enum.GetValues(typeof(VersionGroup)))
                            {
                                var str = ver.ToString().Replace("_", "-");

                                if (str == "latest")
                                    continue;

                                if (details.version_group.name == str)
                                {
                                    MoveBase move = Resources.Load<MoveBase>($"{pathToMoveAssets}{moves.move.name}");

                                    if (move == null && createMoveStubIfNotExistForPokemonMoves && moves.move.name?.Length > 0)
                                    {
                                        MoveBase moveBase = CreateInstance<MoveBase>();
                                        AssetDatabase.CreateAsset(moveBase, $"Assets{pathToResources}{pathToMoveAssets}{moves.move.name}.asset");
                                        yield return null;
                                        move = Resources.Load<MoveBase>($"{pathToMoveAssets}{moves.move.name}");
                                    }

                                    if (move != null && pokemonToAdd.LearnableByBreeding.Count(m => m == move) == 0)
                                    {
                                        pokemonToAdd.LearnableByBreeding.Add(move);
                                        break;
                                    }
                                }
                            }
                        }
                        else if (details.version_group.name == version_group.ToString().Replace("_", "-"))
                        {
                            MoveBase move = Resources.Load<MoveBase>($"{pathToMoveAssets}{moves.move.name}");

                            if (move == null && createMoveStubIfNotExistForPokemonMoves && moves.move.name?.Length > 0)
                            {
                                MoveBase moveBase = CreateInstance<MoveBase>();
                                AssetDatabase.CreateAsset(moveBase, $"Assets{pathToResources}{pathToMoveAssets}{moves.move.name}.asset");
                                yield return null;
                                move = Resources.Load<MoveBase>($"{pathToMoveAssets}{moves.move.name}");
                            }

                            if (move != null && pokemonToAdd.LearnableByBreeding.Count(m => m == move) == 0)
                            {
                                pokemonToAdd.LearnableByBreeding.Add(move);
                                break;
                            }
                        }
                    }
                }
            }

            // Evolution --------------------------------------------------------------------------------------
            pokemonToAdd.Evolutions = new List<Evolution>();

            List<PokeApiEvolvesTo> evolvesTo = new List<PokeApiEvolvesTo>();

            if (pokemon.species_data.evolution_data.chain.species.name == pokemon.name)
            {
                evolvesTo = pokemon.species_data.evolution_data.chain.evolves_to.ToList();
            }
            else
            {
                foreach (var evolution in pokemon.species_data.evolution_data.chain.evolves_to)
                {
                    if (evolution.species.name == pokemon.name)
                    {
                        foreach (var evolution2 in evolution.evolves_to)
                        {
                            var evo = new PokeApiEvolvesTo
                            {
                                evolution_details = evolution2.evolution_details,
                                species = evolution2.species,
                                is_baby = evolution2.is_baby
                            };

                            evolvesTo.Add(evo);
                        }
                    }
                }
            }

            foreach (var evolution in evolvesTo)
            {
                foreach (var detail in evolution.evolution_details)
                {
                    Debug.Log($"{detail.trigger.name}");
                    switch (detail.trigger.name)
                    {
                        case "level-up":
                            if (detail.min_level == 0)
                                continue;

                            Evolution evolutionLevelUp = new Evolution();
                            evolutionLevelUp.RequiredLevel = detail.min_level;
                            evolutionLevelUp.EvolvesInto = Resources.Load<PokemonBase>($"{pathToPokemonAssets}{evolution.species.name}");

                            if (evolutionLevelUp.EvolvesInto == null && createPokemonStubIfNotExistForEvolution)
                            {
                                PokemonBase pokemonBase = CreateInstance<PokemonBase>();
                                AssetDatabase.CreateAsset(pokemonBase, $"Assets/{pathToResources}{pathToPokemonAssets}{evolution.species.name}.asset");
                                yield return null;
                                evolutionLevelUp.EvolvesInto = Resources.Load<PokemonBase>($"{pathToPokemonAssets}{evolution.species.name}");
                            }

                            if (evolutionLevelUp.EvolvesInto != null && pokemonToAdd.Evolutions.Where(e => e.RequiredItem == null && e.RequiredLevel == detail.min_level && e.EvolvesInto.name == evolution.species.name).Count() == 0)
                                pokemonToAdd.Evolutions.Add(evolutionLevelUp);
                            break;

                        case "trade":
                            Debug.Log($"{detail.trade_species}");
                            Evolution evolutionTrade = new Evolution();
                            evolutionTrade.HeldItem = Resources.Load<EvolutionItem>($"{pathToItemAssets}{detail.held_item.name}");
                            evolutionTrade.EvolvesInto = Resources.Load<PokemonBase>($"{pathToPokemonAssets}{evolution.species.name}");
                            evolutionTrade.RequiredPokemon = Resources.Load<PokemonBase>($"{pathToPokemonAssets}{detail.trade_species.name}");

                            if (evolutionTrade.EvolvesInto == null && createPokemonStubIfNotExistForEvolution)
                            {
                                PokemonBase pokemonBase = CreateInstance<PokemonBase>();
                                AssetDatabase.CreateAsset(pokemonBase, $"Assets{pathToResources}{pathToPokemonAssets}{evolution.species.name}.asset");
                                yield return null;
                                evolutionTrade.EvolvesInto = Resources.Load<PokemonBase>($"{pathToPokemonAssets}{evolution.species.name}");
                            }

                            if (evolutionTrade.RequiredPokemon == null && createPokemonStubIfNotExistForEvolution)
                            {
                                PokemonBase pokemonBase = CreateInstance<PokemonBase>();
                                AssetDatabase.CreateAsset(pokemonBase, $"Assets{pathToResources}{pathToPokemonAssets}{detail.trade_species.name}.asset");
                                yield return null;
                                evolutionTrade.RequiredPokemon = Resources.Load<PokemonBase>($"{pathToPokemonAssets}{detail.trade_species.name}");
                            }

                            if (evolutionTrade.HeldItem == null && createItemStubIfNotExistForEvolution && detail.held_item.name?.Length > 0)
                            {
                                EvolutionItem itemBase = CreateInstance<EvolutionItem>();
                                AssetDatabase.CreateAsset(itemBase, $"Assets{pathToResources}{pathToItemAssets}{evolutionItemsFolder}{detail.held_item.name}.asset");
                                evolutionTrade.HeldItem = Resources.Load<EvolutionItem>($"{pathToItemAssets}{evolutionItemsFolder}{detail.held_item.name}");
                            }

                            if (evolutionTrade.EvolvesInto != null && evolutionTrade.HeldItem != null && 
                                evolutionTrade.RequiredPokemon != null && !pokemonToAdd.Evolutions.Contains(evolutionTrade))
                            {
                                pokemonToAdd.Evolutions.Add(evolutionTrade);
                            }
                            break;

                        case "use-item":
                            Evolution evolutionItem = new Evolution();
                            evolutionItem.RequiredItem = Resources.Load<EvolutionItem>($"{pathToItemAssets}{detail.item.name}");
                            evolutionItem.EvolvesInto = Resources.Load<PokemonBase>($"{pathToPokemonAssets}{evolution.species.name}");

                            if (evolutionItem.EvolvesInto == null && createPokemonStubIfNotExistForEvolution)
                            {
                                PokemonBase pokemonBase = CreateInstance<PokemonBase>();
                                AssetDatabase.CreateAsset(pokemonBase, $"Assets{pathToResources}{pathToPokemonAssets}{evolution.species.name}.asset");
                                yield return null;
                                evolutionItem.EvolvesInto = Resources.Load<PokemonBase>($"{pathToPokemonAssets}{evolution.species.name}");
                            }

                            if (evolutionItem.RequiredItem == null && createItemStubIfNotExistForEvolution && detail.item.name?.Length > 0)
                            {
                                EvolutionItem itemBase = CreateInstance<EvolutionItem>();
                                AssetDatabase.CreateAsset(itemBase, $"Assets{pathToResources}{pathToItemAssets}{evolutionItemsFolder}{detail.item.name}.asset");
                                evolutionItem.RequiredItem = Resources.Load<EvolutionItem>($"{pathToItemAssets}{evolutionItemsFolder}{detail.item.name}");
                            }

                            if (evolutionItem.EvolvesInto != null && evolutionItem.RequiredItem != null && !pokemonToAdd.Evolutions.Contains(evolutionItem))
                            {
                                pokemonToAdd.Evolutions.Add(evolutionItem);
                            }
                            break;
                    }
                }
            }

            //if (fetchSprites)
            //{
            //    // Front Sprite
            //    if (!File.Exists(Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{spritesFolder}{pokemon.id}.png"))
            //    {
            //        instance = CreateInstance<PokeApi>();
            //        yield return instance.DownloadSprite(pokemon.sprites.front_default, Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{spritesFolder}{pokemon.id}.png");
            //        yield return new WaitForEndOfFrame();
            //    }

            //    // Back Sprite
            //    if (!File.Exists(Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{spritesFolder}{pokemon.id}-back.png"))
            //    {
            //        instance = ScriptableObject.CreateInstance<PokeApi>();
            //        yield return instance.DownloadSprite(pokemon.sprites.back_default, Application.dataPath + $"{pathToResources}{pathToPokemonAssets}{spritesFolder}{pokemon.id}-back.png");
            //        yield return new WaitForEndOfFrame();
            //    }

            //    AssetDatabase.Refresh();
            //    yield return null;
            //    Sprite front = Resources.Load<Sprite>($"{pathToPokemonAssets}{spritesFolder}{pokemon.id}");
            //    Sprite back = Resources.Load<Sprite>($"{pathToPokemonAssets}{spritesFolder}{pokemon.id}-back");
            //    pokemonToAdd.FrontSprite = front;
            //    pokemonToAdd.BackSprite = back;
            //}

            if (isNew)
                AssetDatabase.CreateAsset(pokemonToAdd, $"Assets/{pathToResources}{pathToPokemonAssets}{pokemon.name}.asset");

            AssetDatabase.SaveAssets();

            yield return null;
        }

        private List<TmItem> LoadAllAssetsAtPath(string path)
        {
            List<TmItem> objects = new List<TmItem>();
            if (Directory.Exists(path))
            {
                string[] assets = Directory.GetFiles(path);
                foreach (string assetPath in assets)
                {
                    if (assetPath.Contains(".asset") && !assetPath.Contains(".meta"))
                    {
                        objects.Add(AssetDatabase.LoadMainAssetAtPath(assetPath) as TmItem);
                    }
                }
            }

            return objects;
        }

        private void SetTMMoves()
        {
            List<TmItem> tmItems = this.LoadAllAssetsAtPath($"Assets/{pathToResources}{pathToItemAssets}{machinesFolder}");

            MoveDB.Init();
            foreach (var tmItem in tmItems)
            {
                string moveName = tmItem.PokeApiItem.effect_entries[0].effect.Replace("Teaches ", string.Empty).Replace(" to a compatible Pok�mon.", string.Empty);
                moveName = moveName.InsertSpaceBeforeUpperCase().Replace(" ", "-").Replace("--", "-").ToLower();
                EditorUtility.SetDirty(tmItem);
                tmItem.Move = MoveDB.GetObjectByName(moveName);
                if (tmItem.name.Substring(0, 2) == "hm")
                {
                    tmItem.IsHM = true;
                }
            }
        }
    }
}


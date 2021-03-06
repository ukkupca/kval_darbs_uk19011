using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Controllers;
using GameManagerData.data;
using GameManagerData.objClasses;
using MenuSystem.Main;
using UnityEngine;
using Application = UnityEngine.Application;

namespace GameManagerData
{
    //Klase pārvalda visu loģiku, kas saistīta ar jaunas spēles sākšanu, spēles turpināšanu, dzēšanu, ainu maiņu, 
    //saglabāto spēļu saraksta iegūšanu
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public SceneController sceneController;

        [Header("Load data")] private string _saveNameData;
        private static bool _loadGame = false;
        private PlayerData _playerLoadData = new PlayerData();
        private List<HomeLoadData> _homeLoadData = new List<HomeLoadData>();
        private List<RoomData> _freeroamRoomData = new List<RoomData>();
        private FurnitureLoadData _furnitureLoadData = new FurnitureLoadData();
        private PlayableLoadData _playableLoadData = new PlayableLoadData();
        public InstantiateLoadedData instantiateLoadedData;

        [Header("Constants")] 
        private const string SAVES = "/saves/";
        private const string PLAYER = "/player";

        private const string ROOMS_SUB = "/rooms";
        private const string ROOMS_COUNT_SUB = "/rooms.count";

        private const string HOME_CONTROLLERS_SUB = "/controllers";
        private const string HOME_CONTROLLERS_COUNT_SUB = "/controllers.count";

        private const string FURNITURE_SUB = "/furniture";
        private const string FURNITURE_COUNT_SUB = "/furniture.count";

        private const string PLAYABLE_SUB = "/playable";
        private const string PLAYABLE_COUNT_SUB = "/playable.count";

        private void Awake()
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        public static GameManager Instance()
        {
            return _instance;
        }

        public void StartNewGame()
        {
            ResetGameData();
            PlayerData.GameID = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
            Directory.CreateDirectory(Application.persistentDataPath + SAVES + PlayerData.GameID);
            LoadNewScene("Testing"); //Spēles ainas nosaukums ir palicis no laika, kad autore testēja kā būvēt spēles 
            //ainas un ir saglabājis savu vēsturisko nosaukumu
        }

        //Funkcija nodrošina spēles datu saglabāšanu failu krātuvē, to veicot ar asinhronu funkciju, kas sagaida
        //kad visi uzdevumi ir pabeigti, pirms tā ļauj spēlētājam turpināt spēli
        public async void SaveGame()
        {
            string folder = PlayerData.GameID;

            BinaryFormatter formatter = new BinaryFormatter();

            var tasks = new List<Task>();
            try
            {
                //Saving Player 
                tasks.Add(SavePlayer(formatter, folder));

                //Saving Home controllers
                tasks.Add(SaveControllerData(formatter, folder));

                //Saving Rooms
                tasks.Add( SaveRoomData(formatter, folder));

                //Saving Furniture
                tasks.Add( SaveFurnitureData(formatter, folder));

                //Saving Playables
                tasks.Add( SavePlayableData(formatter, folder));
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            await Task.WhenAll(tasks);
        }

        //Saglabāšana visiem spēles datiem ir līdzīga, tie tiek saglabāti bināros failos
        private async Task SavePlayer(BinaryFormatter formatter, string folder)
        {
            string path = Application.persistentDataPath + SAVES + folder +  PLAYER;
            
            FileStream stream = new FileStream(path, FileMode.Create);
            PlayerController playerController = PlayerController.Instance();
            PlayerData gameData = playerController.SetPlayerData();

            formatter.Serialize(stream, gameData);
            stream.Close();

            await Task.Yield();
        }

        private async Task SaveControllerData(BinaryFormatter formatter, string folder)
        {
            string path = Application.persistentDataPath + SAVES + folder +  HOME_CONTROLLERS_SUB;
            string countPath = Application.persistentDataPath + SAVES + folder + HOME_CONTROLLERS_COUNT_SUB;

            FileStream countStream = new FileStream(countPath, FileMode.Create);
            formatter.Serialize(countStream, GameData.HomeControllers.Count);
            countStream.Close();

            for (int i = 0; i < GameData.HomeControllers.Count; i++)
            {
                FileStream stream = new FileStream(path + i, FileMode.Create);
                HomeControllerData data = new HomeControllerData(GameData.HomeControllers[i]);

                formatter.Serialize(stream, data);
                stream.Close();
            }

            await Task.Yield();
        }
        
        private async Task SaveRoomData(BinaryFormatter formatter, string folder)
        {
            string path = Application.persistentDataPath + SAVES + folder + ROOMS_SUB;
            string countPath = Application.persistentDataPath + SAVES + folder + ROOMS_COUNT_SUB;

            FileStream countStream = new FileStream(countPath, FileMode.Create);
            formatter.Serialize(countStream, GameData.Rooms.Count);
            countStream.Close();

            for (int i = 0; i < GameData.Rooms.Count; i++)
            {
                FileStream stream = new FileStream(path + i, FileMode.Create);
                RoomData data = new RoomData(GameData.Rooms[i]);

                formatter.Serialize(stream, data);
                stream.Close();
            }
            
            await Task.Yield();
        }
        
        private async Task SaveFurnitureData(BinaryFormatter formatter, string folder)
        {
            string path = Application.persistentDataPath + SAVES + folder + FURNITURE_SUB;
            string countPath = Application.persistentDataPath + SAVES + folder + FURNITURE_COUNT_SUB;

            FileStream countStream = new FileStream(countPath, FileMode.Create);
            formatter.Serialize(countStream, GameData.Furniture.Count);
            countStream.Close();

            for (int i = 0; i < GameData.Furniture.Count; i++)
            {
                FileStream stream = new FileStream(path + i, FileMode.Create);
                FurnitureData data = new FurnitureData(GameData.Furniture[i]);

                formatter.Serialize(stream, data);
                stream.Close();
            }
            
            await Task.Yield();
        }
        
        private async Task SavePlayableData(BinaryFormatter formatter, string folder)
        {
            string path = Application.persistentDataPath + SAVES + folder + PLAYABLE_SUB;
            string countPath = Application.persistentDataPath + SAVES + folder + PLAYABLE_COUNT_SUB;

            FileStream countStream = new FileStream(countPath, FileMode.Create);
            formatter.Serialize(countStream, GameData.Playables.Count);
            countStream.Close();

            for (int i = 0; i < GameData.Playables.Count; i++)
            {
                FileStream stream = new FileStream(path + i, FileMode.Create);
                PlayableData data = new PlayableData(GameData.Playables[i]);

                formatter.Serialize(stream, data);
                stream.Close();
            }
            
            await Task.Yield();
        }

        //Spēles turpināšanas funkcija
        public void LoadGame(string saveName)
        {
            ResetGameData();
            _saveNameData = saveName;
            MainMenu mainMenu = MainMenu.Instance();
            
            if (!Directory.Exists(Application.persistentDataPath + SAVES + saveName))
            {
                Debug.Log("Game does not exist, path: " + Application.persistentDataPath + SAVES + saveName);
                mainMenu.ShowGameCouldNotBeLoadedError();
                return;
            }

            //situācija, kurā spēlētājs ir izveidojis jaunu spēli, bet neko tajā nav saglabājis
            string[] files = Directory.GetFiles(Application.persistentDataPath + SAVES + saveName);
             if (files.Length == 0)
             {
                 _saveNameData = saveName;
                 LoadNewScene("Testing");
                 return;
             }

            BinaryFormatter formatter = new BinaryFormatter();

            //try catch bloks mēģina ielādēt saglabātos spēles datus, ja tajos ir kāda kļūda
            //spēlētājam tiek parādīts kļūdas ziņojums galvenajā izvēlnē
            try
            {
                LoadGameData();
            }
            catch (Exception e)
            {
                Debug.Log(e);
                mainMenu.ShowGameCouldNotBeLoadedError();
                return;
            }
            LoadNewScene(_playerLoadData.sceneType);
            _loadGame = true;
        }

        public void LoadNewScene(string sceneName)
        {
            sceneController.StartSceneLoad(sceneName);
        }

        public void LoadGameData()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            
            //Laoding Player
            LoadPlayer(formatter, _saveNameData);
            
            //Loading Home controllers
            LoadControllers(formatter, _saveNameData);

            //Loading Rooms
            LoadRooms(formatter, _saveNameData);

            //Loading Furniture
            LoadFurniture(formatter, _saveNameData);

            //Loading Playables
            LoadPlayables(formatter, _saveNameData);
        }

        
        //Datu ielāde notiek līdzīgā kartībā kā datu saglabāšana. Atšķirība ir tajā, ka nolasītos datus saglabā
        //struktūrās, kuras vēlāk tiks izmantotas spēles objektu izveidošanai spēles ainā
        private void LoadPlayer(BinaryFormatter formatter, string saveName)
        {
            
            string path = Application.persistentDataPath + SAVES + saveName +  PLAYER;
            
            if (File.Exists(path))
            {
                FileStream stream = new FileStream(path, FileMode.Open);
                _playerLoadData = formatter.Deserialize(stream) as PlayerData;

                stream.Close();
            }
            else
            {
                throw new Exception("Path not found " + path);
            }
        }
        
        private void LoadControllers(BinaryFormatter formatter, string saveName)
        {
            string controllersPath = Application.persistentDataPath + SAVES + saveName + HOME_CONTROLLERS_SUB;
            string controllersCountPath = Application.persistentDataPath + SAVES + saveName + HOME_CONTROLLERS_COUNT_SUB;

            int controllerCount = 0;
            if (File.Exists(controllersCountPath))
            {
                FileStream controllersCountStream = new FileStream(controllersCountPath, FileMode.Open);
                controllerCount = (int) formatter.Deserialize(controllersCountStream);
                controllersCountStream.Close();
            }
            else
            {
                throw new Exception("Path not found " + controllersCountPath);
            }

            for (int i = 0; i < controllerCount; i++)
            {
                if (File.Exists(controllersPath + i))
                {
                    FileStream stream = new FileStream(controllersPath + i, FileMode.Open);
                    HomeControllerData data = formatter.Deserialize(stream) as HomeControllerData;

                    stream.Close();
                    
                    if (data == null)
                    {
                        return;
                    }

                    HomeLoadData newControllerStruct = new HomeLoadData();
                    newControllerStruct.AddControllerData(data);
                    _homeLoadData.Add(newControllerStruct);
                }
                else
                {
                    throw new Exception("Path not found " + controllersPath + i);
                }
            }
        }

        private void LoadRooms(BinaryFormatter formatter, string saveName)
        {
            string roomsPath = Application.persistentDataPath + SAVES + saveName + ROOMS_SUB;
            string roomsCountPath = Application.persistentDataPath + SAVES + saveName + ROOMS_COUNT_SUB;

            int roomCount = 0;
            if (File.Exists(roomsCountPath))
            {
                FileStream countStream = new FileStream(roomsCountPath, FileMode.Open);
                roomCount = (int) formatter.Deserialize(countStream);
                countStream.Close();
            }
            else
            {
                throw new Exception("Path not found " + roomsCountPath);
            }

            for (int i = 0; i < roomCount; i++)
            {
                if (File.Exists(roomsPath + i))
                {
                    FileStream stream = new FileStream(roomsPath + i, FileMode.Open);
                    RoomData data = formatter.Deserialize(stream) as RoomData;

                    stream.Close();

                    if (data == null)
                    {
                        return;
                    }
                    
                    foreach (var controller in _homeLoadData)
                    {
                        if (controller.ControllerID == data.controllerID)
                        {
                            controller.AddRoomData(data);
                            data = null;
                            break;
                        }
                    }
                    //Brīvās istabas ir jāapstrādā atsevišķi, lai ielādes procesā nerastos kļūdaini savienojumi brīvajām
                    //istabām ar mājas sturktūrām
                    if (data != null)
                    {
                        _freeroamRoomData.Add(data);
                    }
                }
                else
                {
                    throw new Exception("Path not found " + roomsPath + i);
                }
            }
        }
        
        private void LoadFurniture(BinaryFormatter formatter, string saveName)
        {
            string path = Application.persistentDataPath + SAVES + saveName + FURNITURE_SUB;
            string countPath = Application.persistentDataPath + SAVES + saveName + FURNITURE_COUNT_SUB;

            int furnitureCount = 0;
            if (File.Exists(countPath))
            {
                FileStream countStream = new FileStream(countPath, FileMode.Open);
                furnitureCount = (int) formatter.Deserialize(countStream);
                countStream.Close();
            }
            else
            {
                throw new Exception("Path not found " + countPath);
            }

            for (int i = 0; i < furnitureCount; i++)
            {
                if (File.Exists(path + i))
                {
                    FileStream stream = new FileStream(path + i, FileMode.Open);
                    FurnitureData data = formatter.Deserialize(stream) as FurnitureData;

                    stream.Close();

                    if (data == null)
                    {
                        return;
                    }
                    
                    _furnitureLoadData.AddFurnitureData(data);
                }
                else
                {
                    throw new Exception("Path not found " + path + i);
                }
            }
        }
        
        private void LoadPlayables(BinaryFormatter formatter, string saveName)
        {
            string path = Application.persistentDataPath + SAVES + saveName + PLAYABLE_SUB;
            string countPath = Application.persistentDataPath + SAVES + saveName + PLAYABLE_COUNT_SUB;

            int playableCount = 0;
            if (File.Exists(countPath))
            {
                FileStream countStream = new FileStream(countPath, FileMode.Open);
                playableCount = (int) formatter.Deserialize(countStream);
                countStream.Close();
            }
            else
            {
                throw new Exception("Path not found " + countPath);
            }

            for (int i = 0; i < playableCount; i++)
            {
                if (File.Exists(path + i))
                {
                    FileStream stream = new FileStream(path + i, FileMode.Open);
                    PlayableData data = formatter.Deserialize(stream) as PlayableData;

                    stream.Close();

                    if (data == null)
                    {
                        return;
                    }
                    
                    _playableLoadData.AddPlayableData(data);
                }
                else
                {
                    throw new Exception("Path not found " + path + i);
                }
            }
        }
        
        //Funkciju izsauce GameDataLoader klase, lai iesāktu ielādēto datu izveidošanas procesu
        public void InstantiateLoadedData()
        {
            if (!_loadGame)
            {
                return;
            }
            StartCoroutine(ProcessLoadedData());
        }

        //Funkcija secīgi apstrādā un izveido ielādētos datus, nodrošina to konfigurāciju un kontrolē spēlētāja pozīciju
        //un tam pieejamās darbības pēc datu izveidošanas
        IEnumerator ProcessLoadedData()
        {
            //Vispirms tiek izveidots katrs mājas kontrolieris un tam piederošās mājas
            foreach (var controller in _homeLoadData)
            {
                GameObject home = instantiateLoadedData.LoadSavedController(controller.HomeControllerData);
                foreach (var room in controller.ControllersRooms)
                {
                    instantiateLoadedData.LoadSavedRoom(room);
                }
                yield return new WaitForSeconds(0.5f);
                EmptyActiveSocketController.TurnOffAllForSpecificHome(home.GetComponent<HomeControllerObject>().controllerID);
            }
            
            //Tad tiek izveidotas brīvās istabas
            foreach (var room in _freeroamRoomData)
            {
                instantiateLoadedData.LoadSavedRoom(room);
            }

            //Tiek pievienotas mēbeles
            foreach (var furniture in _furnitureLoadData.Furniture)
            {
                instantiateLoadedData.LoadSavedFurniture(furniture);
            }
            
            //Tiek pievienoti spēlējamie objekti
            foreach (var playable in _playableLoadData.Playables)
            {
                instantiateLoadedData.LoadSavedPlayable(playable);
            }
            
            //Spēle tiek sagatavota spēlēšanas stadijai
            RoomController.ToggleGrabOffForGrabbableRooms();
            FurnitureController.SetAllFurnitureNotMovable();
            
            yield return new WaitForSeconds(2f);
            PlayerController playerController = PlayerController.Instance();
            //Spēlētājs tiek sagatavos saglabātas spēles turpināšanai
            playerController.PreparePlayerLoadGame(_playerLoadData);
            
            _loadGame = false;
        }

        public static bool IsLoadGame()
        {
            return _loadGame;
        }

        //Funkcija atgriež saglabāto spēļu sarakstu
        public string[] GetSavedGames()
        {
            string path = Application.persistentDataPath + SAVES;
            string[] savedGames = Directory.GetDirectories(path)
                .Select(Path.GetFileName)
                .ToArray();
            return savedGames;
        }

        public void DeleteGame(string saveName)
        {
            Directory.Delete(Application.persistentDataPath + SAVES + saveName, true);
        }

        //Funkcija nodrošina, ka visi aktīvās spēles dati ir tukši, kad tiek ielādēta jauna vai turpināma spēle
        public void ResetGameData()
        {
            EmptyActiveSocketController.EmptyActiveSockets.Clear();
            RoomController.GrabbableRooms.Clear();
            GameData.HomeControllers.Clear();
            GameData.Rooms.Clear();
            GameData.Furniture.Clear();
            GameData.Playables.Clear();
            
            _homeLoadData.Clear();
            _freeroamRoomData.Clear();
            _furnitureLoadData.Furniture.Clear();
            _playableLoadData.Playables.Clear();
        }

        public void QuitGame()
        {
            #if UNITY_EDITOR
                // Application.Quit() does not work in the editor so
                // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}

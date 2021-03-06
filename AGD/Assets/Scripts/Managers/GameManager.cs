﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;
using JetBrains.Annotations;
using Rewired;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.ThirdPerson;

public enum	EnemyType
{
	Type1 = 0,
	Type2,
	Type3,
	Type4,
	Type5,
	Type6
}

[System.Serializable]
public sealed class EnemyDefinition
{
	public EnemyType enemySpawner;
	public int quantity;
};

[System.Serializable]
public sealed class WaveDefinition
{
	public List<EnemyDefinition> enemyDef = new List<EnemyDefinition>();
};

public class GameManager : NetworkBehaviour {

	public static GameManager instance;

	[HideInInspector]public List<WaveDefinition> waves = new List<WaveDefinition>();
	[SyncVar]public int currentWave = 0;
	WaveDefinition currentWaveDefinition;
	int enemyCount = 0;
	public int waveSleepTimer;
	private bool waveComplete = false;
	public bool WaveComplete { get { return waveComplete; } }
	public List<GameObject> enemyPrefabs = new List<GameObject>();
    public List<Text>enemiesRemainigText = new List<Text>();
    public bool playerOneAssigned = false;
    public GameObject GameOverPanel, StatusPanel;
    public bool firstWave = false;
    private Dictionary<int, int> scoreTable = new Dictionary<int, int>();
    public Dictionary<int, int> ScoreTable { get { return scoreTable; } }
    public List<NetworkedThirdPersonCharacter> players = new List<NetworkedThirdPersonCharacter>();
    private List<ScorePanel> scorePanels = new List<ScorePanel>();
    private List<MakeRadarObject> RadarHelper = new List<MakeRadarObject>();

	public void Reset()
	{
		currentWave = 0;
		enemyCount = 0;
		waveComplete = false;
		enemiesRemainigText.Clear ();
		playerOneAssigned = false;
		firstWave = false;
		scoreTable.Clear ();
		scorePanels.Clear ();
		RadarHelper.Clear ();
	}

    void Awake()
	{
        if (!instance)
            instance = this;
        else 
            DestroyObject(gameObject);
        DontDestroyOnLoad(this);

	    ReWiredAwake();
        SceneManager.sceneLoaded += ResetOnMenuLoad;
	}

	// Use this for initialization
	void Start () {
		enabled = false;

    }

    private bool forcedWaveSpawning = false;
	// Update is called once per frame
	void Update () 
	{
		if(!isServer)
			return;
		
		if(enemyCount <= 0 && !waveComplete && firstWave)
			StartCoroutine(WaveWaitTimer());

	    if (SceneManager.GetActiveScene().buildIndex == 2)
	    {
	        if(Input.GetKeyUp("1"))
                ForceSpawnWave(0);
            else if (Input.GetKeyUp("2"))
                ForceSpawnWave(1);
            else if (Input.GetKeyUp("3"))
                ForceSpawnWave(2);
            else if (Input.GetKeyUp("4"))
                ForceSpawnWave(3);
            else if (Input.GetKeyUp("5"))
                ForceSpawnWave(4);
            else if (Input.GetKeyUp("6"))
                ForceSpawnWave(5);
            else if (Input.GetKeyUp("7"))
                ForceSpawnWave(6);
            else if (Input.GetKeyUp("8"))
                ForceSpawnWave(7);
            else if (Input.GetKeyUp("9"))
                ForceSpawnWave(8);
            else if (Input.GetKeyUp("0"))
                ForceSpawnWave(9);
	        if (Input.GetKeyUp(KeyCode.F9))
	        {
                for (int i = 0; i < players.Count; ++i)
                    PostScoreToScoreTable(i, i * Random.Range(10, 36));
            }
            if (Input.GetKeyUp(KeyCode.F10))
	        {
                for(int i = 0; i < players.Count; ++i)
                    players[i].gameObject.SetActive(false);
                GameObject gp = GameObject.Find("LevelPreview");
                gp.SetActive(true);
	            gp.GetComponent<Animation>().Play();
            }
            if (Input.GetKeyUp(KeyCode.F11))
            {
                currentWave = waves.Count-1;
                var enemies = FindObjectsOfType<GhostBehaviour>();
                for (int i = 0; i < enemies.Length; ++i)
                    enemies[i].Kill();
                currentWave = waves.Count;
                
            }
        }

        
	}

    private AnimationClip levelPreviewClip;

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= ResetOnMenuLoad;
    }

    void ResetOnMenuLoad(Scene newScene, LoadSceneMode mode)
    {
        if (newScene.buildIndex == 0)
        {
            playerOneAssigned = false;
            players.Clear();
            scoreTable.Clear();
        }
        else if (newScene.buildIndex == 2)
        {
            controller = FindObjectOfType<GameOverController>();
            if(controller)
                controller.gameObject.SetActive(false);
        }
    }

	public void DestroyEnemy()
	{
		enemyCount--;
	    if (enemyCount > 0)
	    {
            foreach (Text text in enemiesRemainigText)
                text.text = enemyCount.ToString();
        }
	    else
	    {
            foreach (Text text in enemiesRemainigText)
                text.text = "Loading next wave";
        }
	}

	IEnumerator WaveWaitTimer()
	{
		waveComplete = true;
		
		currentWave++;
	    if (currentWave < waves.Count)
	    {
            yield return new WaitForSeconds(waveSleepTimer);
            if(!forcedWaveSpawning)
                SpawnEnemies();
	    }
		else
		{
            yield return null;
            //GameOverPanel.SetActive(true);
            //StatusPanel.SetActive(true);
		    if (SceneManager.GetActiveScene().name != "Menu");
		    {
		        GameOver();
		        Cursor.lockState = CursorLockMode.None;
		        Cursor.visible = true;
		    }
		}
	    waveComplete = false;
	}
    public IEnumerator SpawnFirstWaveWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        firstWave = true;
        SpawnEnemies();
    }

    public void SpawnEnemies()
    {
	    if (!isServer)
	        return;
        if (!firstWave)
            firstWave = true;

        StartCoroutine(Spawn());

    }


    void ForceSpawnWave(int waveID)
    {
        var enemies = FindObjectsOfType<GhostBehaviour>();
        for (int i = 0; i < enemies.Length; ++i)
        {
            enemies[i].Kill();
        }
        currentWave = waveID;
        forcedWaveSpawning = true;
        StartCoroutine(Spawn());
    }


    IEnumerator Spawn()
    {
        scorePanels.Clear();
        scorePanels = FindObjectsOfType<ScorePanel>().ToList();
        var spawnLocations = GameObject.FindObjectsOfType<NetworkStartPosition>().ToList();
        var enemySpawns = spawnLocations.FindAll(x => x.gameObject.CompareTag("enemySpawn")).ToList();

        for (int i = 0; i < players.Count; ++i)
            players[i].waveText.text = "WAVE " + (currentWave+1);

        for (int i = 0; i < waves[currentWave].enemyDef.Count; ++i)
        {
            EnemyType etype = waves[currentWave].enemyDef[i].enemySpawner;
            GameObject prefab;
            for (int j = 0; j < waves[currentWave].enemyDef[i].quantity; ++j)
            {
                prefab = enemyPrefabs[(int)etype];
                var enemySpawn = enemySpawns[Random.Range(0, enemySpawns.Count)].transform;
                GameObject newEnemy = (GameObject)Instantiate(prefab, enemySpawn.position, enemySpawn.rotation);
                NetworkServer.Spawn(newEnemy);
                enemyCount++;

                foreach (Text text in enemiesRemainigText)
                {
                    if (text == null)
                    {
                        enemiesRemainigText.TrimExcess();
                        continue;
                    }
                    text.text = enemyCount.ToString();
                }

                yield return new WaitForSeconds(0.5f);
            }
        }
        forcedWaveSpawning = false;


    }


    public void HideGameOverPanel()
    {
        GameOverPanel.SetActive(false);
    }


    void ReWiredAwake()
    {
        // Listen for controller connection events
        ReInput.ControllerConnectedEvent += OnControllerConnected;

        // Assign each Joystick to a Player initially
        foreach (Joystick j in ReInput.controllers.Joysticks)
        {
            if (ReInput.controllers.IsJoystickAssigned(j)) continue; // Joystick is already assigned

            // Assign Joystick to first Player that doesn't have any assigned
            AssignJoystickToNextOpenPlayer(j);
        }
    }

    // This will be called when a controller is connected
    void OnControllerConnected(ControllerStatusChangedEventArgs args)
    {
        if (args.controllerType != ControllerType.Joystick) return; // skip if this isn't a Joystick

        // Assign Joystick to first Player that doesn't have any assigned
        AssignJoystickToNextOpenPlayer(ReInput.controllers.GetJoystick(args.controllerId));
    }

    
    void AssignJoystickToNextOpenPlayer(Joystick j)
    {
        foreach (Player p in ReInput.players.Players)
        {
            if (p.controllers.joystickCount > 0) continue; // player already has a joystick
            p.controllers.AddController(j, true); // assign joystick to player
            return;
        }
    }

    public void PostScoreToScoreTable(int playerID, int scoreToAdd)
    {
        if (isServer)
        {
            if(!players.Find(x => x.playerID == playerID))
                return;

            if (!scoreTable.ContainsKey(playerID))
                scoreTable.Add(playerID, scoreToAdd);
            else
                scoreTable[playerID] += scoreToAdd;

            RpcPostServerScoreToClients(playerID, scoreTable[playerID]);
        }
    }
    

    [ClientRpc]
    private void RpcPostServerScoreToClients(int id, int serverScore)
    {
        if (!scoreTable.ContainsKey(id))
            scoreTable.Add(id, serverScore);
        else
            scoreTable[id] = serverScore;

        scorePanels = FindObjectsOfType<ScorePanel>().ToList();

        for (var i = 0; i < scorePanels.Count; i++)
        {
            scorePanels[i].PostScoreToThisBoard(id, scoreTable[id]);
        }
    }

    private void GameOver()
    {
        controller.gameObject.SetActive(true);
        if (!isServer)
            return;
        
        List<NetworkedThirdPersonCharacter> sortedPlayers = players.OrderBy(x => scoreTable[x.playerID]).ToList();
		sortedPlayers.Reverse ();
        for (int i = 0; i < sortedPlayers.Count; ++i)
            RpcSetIdvGOControllerData(sortedPlayers[i].playerID, i);
    }


    private GameOverController controller;

    public void RegisterGameOVerController(GameOverController goc)
    {
        controller = goc;
        if (controller)
            controller.gameObject.SetActive(false);
    }

    [ClientRpc]
    private void RpcSetIdvGOControllerData(int playerID, int position)
    {
		if (position < 3)
			StartCoroutine (SetIDVAnimData (playerID, position));
		else {
			NetworkedThirdPersonCharacter player = players.Find(x => x.playerID == playerID);
			player.gameObject.SetActive (false);
		}
    }

	IEnumerator SetIDVAnimData(int playerID, int position)
	{
		NetworkedThirdPersonCharacter player = players.Find(x => x.playerID == playerID);
		player.GetComponent<Animator>().SetBool(position == 0 ? "Won" : "Lost", true);
		player.enabled = false;
		player.GetComponent<NetworkedThirdPersonUserControl>().enabled = false;
		player.GetComponent<PickUps>().enabled = false;
		player.GetComponent<Radar>().enabled = false;
		player.GetComponent<MakeRadarObject>().enabled = false;
		player.GetComponent<PlayerIKController>().enabled = false;
		player.GetComponent<CameraManager>().enabled = false;
		player.GetComponent<Rigidbody>().velocity = Vector3.zero;
		player.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		player.GetComponent<Rigidbody>().constraints  = RigidbodyConstraints.FreezeAll;
		player.m_MouseLook.weapon.localPosition = new Vector3 (-0.097f, 0.012f, 0.0928f);
		player.m_MouseLook.weapon.localRotation = Quaternion.Euler (new Vector3 (358.417f, 258.623f, 261.856f));
		player.weaponSpawnPoint.parent.gameObject.SetActive (false);
		Camera[] cams = player.GetComponentsInChildren<Camera>();
		for (int i = 0; i < cams.Length; ++i)
			cams[i].enabled = false;
		player.GetComponentInChildren<Canvas>().gameObject.SetActive(false);
		player.transform.position = position < 3
			? controller.dataContainers[position].position.position
			: new Vector3(0, 250, 0);
		player.transform.rotation = Quaternion.identity;
		controller.dataContainers[position].score.text = scoreTable[player.playerID].ToString();
		yield return null;
		player.GetComponent<Animator>().SetBool(position == 0 ? "Won" : "Lost", false);
	}

    public void RegisterRadarHelper(MakeRadarObject radarHelper)
    {
        if(!RadarHelper.Contains(radarHelper))
            RadarHelper.Add(radarHelper);
    }

    public void DeRegisterRadarHelper(MakeRadarObject radarHelper)
    {
        if (RadarHelper.Contains(radarHelper))
            RadarHelper.Remove(radarHelper);
        RadarHelper.TrimExcess();
    }

    public void RegisterEnemyToRadarHelper(GhostBehaviour enemy)
    {
        for(int i=0; i < RadarHelper.Count; ++i)
            RadarHelper[i].RegisterEnemy(enemy);
    }

    public void DeRegisterEnemyToRadarHelper(GhostBehaviour enemy)
    {
        for (int i = 0; i < RadarHelper.Count; ++i)
            RadarHelper[i].DeregisterEnemy(enemy);
    }

    public void RegisterPickupToRadarHelper(PickUpBase pickup)
    {
        for (int i = 0; i < RadarHelper.Count; ++i)
            RadarHelper[i].RegisterPickup(pickup);
    }

    public void DeRegisterPickupToRadarHelper(PickUpBase pickup)
    {
        for (int i = 0; i < RadarHelper.Count; ++i)
            RadarHelper[i].DeregisterPickup(pickup);
    }
}

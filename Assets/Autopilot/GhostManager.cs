using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VehiclePhysics;

public class GhostManager : MonoBehaviour
{
    //public PidController abc;

    //public VehicleBase target;
    //public VPReplayAsset vPReplayAsset;
    //public LapTimer lapTimer;

    public VPReplay vehicleVPReplay;
    public Rigidbody ghostVehicle;
    public VPReplay ghostVPReplay;
    public Rigidbody tempVehicle;
    public VPReplay tempVPReplay;
    public VPReplayController replayController;

    public bool showGui = true;
    public bool resetLapRecord = false;

    private List<VPReplay.Frame> currentReplay = new List<VPReplay.Frame>();
    //private List<VPReplay.Frame> recordedReplay = new List<VPReplay.Frame>();

    // Control box on bottom left corner
    void OnGUI()
    {
        if (showGui)
        {
            float paddings = 8;
            float rows = 2;
            float columns = 2;
            float btnHeight = 20;
            float btnWidth = 60;
            float boxHeight = rows * btnHeight + paddings * (rows + 2);
            float boxWidth = btnWidth * columns + paddings * (columns + 1);

            float posX = paddings;
            float posY = Screen.height - boxHeight - paddings;

            GUI.Box(new Rect(posX, posY, boxWidth, boxHeight), "Ghost Mode");
            if (GUI.Button(new Rect(posX + paddings, posY + boxHeight - (btnHeight * rows) - paddings - paddings / 2 * (rows - 1), btnWidth, btnHeight), "Start"))
            {
                PlayGhost();
            }

            if (GUI.Button(new Rect(posX + paddings * 2 + btnWidth, posY + boxHeight - (btnHeight * rows) - paddings - paddings / 2 * (rows - 1), btnWidth, btnHeight), "Stop"))
            {
                ghostVehicle.gameObject.SetActive(false);
            }

            if (GUI.Button(new Rect(posX + paddings, posY + boxHeight - btnHeight - 8, btnWidth, btnHeight), "Save"))
            {
                string path = EditorUtility.SaveFilePanel("SaveToAsset", "Assets/Replays/", "defaultName", "asset").Trim();
                string filename = Path.GetFileName(path);

                tempVehicle.gameObject.SetActive(true);
                VPReplayAsset replayAsset = tempVPReplay.SaveReplayToAsset();
                tempVehicle.gameObject.SetActive(false);
                AssetDatabase.CreateAsset(replayAsset, "Assets/Replays/" + filename);
                AssetDatabase.SaveAssets();

                Debug.Log("Temp Replay saved - " + tempVPReplay.recordedData.Count);
            }

            if (GUI.Button(new Rect(posX + paddings * 2 + btnWidth, posY + boxHeight - btnHeight - 8, btnWidth, btnHeight), "Load"))
            {
                ghostVehicle.gameObject.SetActive(true);
                string path = EditorUtility.OpenFilePanel("LoadFromAsset", "Assets/Replays/", "asset");
                string filename = Path.GetFileName(path);

                VPReplayAsset replayAsset = (VPReplayAsset)AssetDatabase.LoadAssetAtPath("Assets/Replays/" + filename, typeof(VPReplayAsset));
                ghostVPReplay.LoadReplayFromAsset(replayAsset);

                Debug.Log(ghostVPReplay.recordedData.Count);
                currentReplay = ghostVPReplay.recordedData;
                ghostVehicle.gameObject.SetActive(false);
            }
        }
    }

    void Start()
    {
        ghostVPReplay.LoadReplayFromAsset(replayController.predefinedReplay);
        currentReplay = ghostVPReplay.recordedData;
        ghostVehicle.gameObject.SetActive(false);
    }

    void ResetLapRecord()
    {
        PlayerPrefs.DeleteKey("BestLap");
        print("Best lap record reseted.");
        resetLapRecord = false;
    }

    public void AutoSaveReplay()
    {
        VPReplayAsset replayAsset = vehicleVPReplay.SaveReplayToAsset();
        UnityEditor.AssetDatabase.CreateAsset(replayAsset, "Assets/Replays/Ghost Best Lap" /*+ DateTime.Now.ToString("dd MMM yyyy")*/ + ".asset");
        UnityEditor.AssetDatabase.SaveAssets();

        print("Autosaved laptime: " + FormatLapTime(PlayerPrefs.GetFloat("BestLap")));
        //currentReplay = vehicleVPReplay.recordedData;
        //tempVPReplay = vehicleVPReplay;
        //PlayGhost();
    }

    public void SaveTemporaryReplay()
    {
        VPReplayAsset replayAsset = vehicleVPReplay.SaveReplayToAsset();
        tempVehicle.gameObject.SetActive(true);
        //tempVPReplay = vehicleVPReplay;
        tempVPReplay.LoadReplayFromAsset(replayAsset);
        tempVehicle.gameObject.SetActive(false);
        print("Save temporary replay " + tempVPReplay.recordedData.Count);
    }

    string FormatLapTime(float t)
    {
        int seconds = Mathf.FloorToInt(t);

        int m = seconds / 60;
        int s = seconds % 60;
        int mm = Mathf.RoundToInt(t * 1000.0f) % 1000;

        return string.Format("{0,3}:{1,2:00}:{2,3:000}", m, s, mm);
    }

    public void StartRecording()
    {
        vehicleVPReplay.recordedData.Clear();
    }

    void Update()
    {
        if (resetLapRecord) { ResetLapRecord(); }
        //Debug.Log(vehicleVPReplay.recordedData.Count);
    }

    void FixedUpdate()
    {
        //target.data.Set(Channel.Input, InputData.AutomaticGear, 4);
        //target.data.Set(Channel.Input, InputData.Throttle, 2000);
    }

    public void PlayGhost()
    {
        ghostVehicle.gameObject.SetActive(false);
        ghostVehicle.gameObject.SetActive(true);
        StartCoroutine(StartGhost());
    }

    IEnumerator StartGhost()
    {
        for (int i = 0; i < currentReplay.Count; i++)
        {
            ghostVehicle.MovePosition(currentReplay[i].position);
            ghostVehicle.MoveRotation(currentReplay[i].rotation);

            yield return new WaitForFixedUpdate();
        }
        ghostVehicle.gameObject.SetActive(false);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SFB;
using UnityEngine.UI;
using System.IO;
using System;
using JosephEngine;
public class Convert2JsonFolder : MonoBehaviour
{
    public Button btn_quit, btn_open, btn_select_json_folder, btn_save;
    public RawImage ri_black;
    public Text tt_warning;

    public Animator anim_warning;
    public GameObject go_loading_icon;
    string input_path = string.Empty, output_path = string.Empty;

    // Start is called before the first frame update
    void Start()
    {
        btn_quit.onClick.AddListener(Quit);
        btn_open.onClick.AddListener(Open);
        btn_select_json_folder.onClick.AddListener(SelectJsonFolder);
        btn_save.onClick.AddListener(Save);
    }
    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
    /// <summary>
    /// shows openfolderdialog for selecting folder that includes mses
    /// </summary>
    void Open()
    {
        StandaloneFileBrowser.OpenFolderPanelAsync("Select mse folder to input", Application.dataPath, false, (string[] paths) =>
        {
            if (paths.Length > 0)
                input_path = paths[0];
        });
    }
    /// <summary>
    /// shows openfolderdialog for selecting folder to be converted into json form
    /// </summary>
    void SelectJsonFolder()
    {
        StandaloneFileBrowser.OpenFolderPanelAsync("Select json folder to output", Application.dataPath, false, (string[] paths) =>
        {
            if (paths.Length > 0)
                output_path = paths[0];
        });
    }
    /// <summary>
    /// when clicking this, all of mses are converted into json form in the output directory
    /// </summary>
    void Save()
    {
        if (input_path == string.Empty)
        {
            ri_black.enabled = true;
            StartCoroutine(Wait());
            tt_warning.text = "Select mse folder to input!";
            anim_warning.enabled = true;
            anim_warning.Play("warning");
            return;
        }
        if (output_path == string.Empty)
        {
            ri_black.enabled = true;
            StartCoroutine(Wait());
            tt_warning.text = "Select json folder to output!";
            anim_warning.enabled = true;
            anim_warning.Play("warning");
            return;
        }
        go_loading_icon.SetActive(true);
        string[] urls = Directory.GetFiles(input_path);
        foreach (string url in urls)
        {
            string file_name = url.Replace(input_path, string.Empty).Remove(0, 1).Replace(".mse", string.Empty);
            Parse(url, file_name);
        }
        StartCoroutine(WaitingConvert());
    }
    /// <summary>
    /// coroutine for waiting conversion
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitingConvert()
    {
        yield return new WaitForSeconds(1f);
        go_loading_icon.SetActive(false);
    }
    /// <summary>
    /// coroutine for waiting error animation
    /// </summary>
    /// <returns></returns>
    IEnumerator Wait()
    {
        yield return new WaitForSeconds(1.5f);
        ri_black.enabled = false;
    }
    /// <summary>
    /// parse each particle's property in the imported json
    /// </summary>
    /// <param name="url"></param>
    /// <param name="file_name"></param>
    void Parse(string url, string file_name)
    {
        PlayerData player = new PlayerData();
        player.particles = new List<Particle>();
        using (StreamReader sr = File.OpenText(url))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.StartsWith("BoundingSphereRadius"))
                {
                    string[] split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    player.boundingSphereRadius = float.Parse(split[1]);
                }
                else if (line.StartsWith("BoundingSpherePosition"))
                {
                    string[] split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    player.boundingSpherePosition = new Vector3(float.Parse(split[1]), float.Parse(split[3]), float.Parse(split[2]));
                }
                if (line.StartsWith("Group Particle"))
                {
                    player.particles.Add(ParseParticleProperties(sr));
                }
            }
            string str_json = JsonUtility.ToJson(player);
            File.WriteAllText(output_path + "/" + file_name + ".json", str_json);
        }
    }
    /// <summary>
    /// parses main properties of the each particle using streamreader
    /// </summary>
    /// <param name="sr"></param>
    /// <returns>Particle</returns>
    Particle ParseParticleProperties(StreamReader sr)
    {
        Particle particle = new Particle();
        particle.timeEventPosition = new SerializableDictionary<float, Vector3>();
        particle.timeEventEmittingSize = new SerializableDictionary<float, float>();
        particle.timeEventEmittingAngularVelocity = new SerializableDictionary<float, float>();
        particle.timeEventEmittingDirectionX = new SerializableDictionary<float, float>();
        particle.timeEventEmittingDirectionY = new SerializableDictionary<float, float>();
        particle.timeEventEmittingDirectionZ = new SerializableDictionary<float, float>();
        particle.timeEventEmittingVelocity = new SerializableDictionary<float, float>();
        particle.timeEventEmissionCountPerSecond = new SerializableDictionary<float, float>();
        particle.timeEventLifeTime = new SerializableDictionary<float, float>();
        particle.timeEventSizeX = new SerializableDictionary<float, float>();
        particle.timeEventSizeY = new SerializableDictionary<float, float>();
        particle.timeEventScaleX = new SerializableDictionary<float, float>();
        particle.timeEventScaleY = new SerializableDictionary<float, float>();
        particle.timeEventColorRed = new SerializableDictionary<float, float>();
        particle.timeEventColorGreen = new SerializableDictionary<float, float>();
        particle.timeEventColorBlue = new SerializableDictionary<float, float>();
        particle.timeEventAlpha = new SerializableDictionary<float, float>();
        particle.timeEventRotation = new SerializableDictionary<float, float>();
        string line;

        while ((line = sr.ReadLine()) != null && !line.StartsWith("}"))
        {
            // start picking
            if (line.Contains("StartTime"))
            {
                string[] split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                particle.startTime = float.Parse(split[1]);
            }
            else if (line.Contains("MOVING_TYPE_DIRECT"))
            {
                string[] split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                particle.timeEventPosition.ToDictionary().Add(float.Parse(split[0]), new Vector3(float.Parse(split[2]), float.Parse(split[4]), float.Parse(split[3])));
            }

            #region for EmitterProperty
            else if (line.Contains("MaxEmissionCount"))
            {
                string[] split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                particle.maxEmissionCount = int.Parse(split[1]);
            }

            else if (line.Contains("CycleLength"))
            {
                string[] split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                particle.cycleLength = float.Parse(split[1]);
            }
            else if (line.Contains("CycleLoopEnable"))
            {
                string[] split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                particle.cycleLoopEnable = int.Parse(split[1]);
            }
            else if (line.Contains("LoopCount"))
            {
                string[] split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                particle.loopCount = int.Parse(split[1]);
            }

            else if (line.Contains("EmitterShape"))
            {
                string[] split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                particle.emitterShape = int.Parse(split[1]);
            }
            else if (line.Contains("EmitterEmitFromEdgeFlag"))
            {
                string[] split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                particle.emitterEmitFromEdgeFlag = int.Parse(split[1]);
            }

            else if (line.Contains("TimeEventEmittingSize"))
            {
                string _line = sr.ReadLine();
            J00001: _line = sr.ReadLine();

                if (!_line.Contains("}"))
                {
                    string[] split = _line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    particle.timeEventEmittingSize.ToDictionary().Add(float.Parse(split[0]), float.Parse(split[1]));
                    goto J00001;
                }
            }
            else if (line.Contains("TimeEventEmittingAngularVelocity"))
            {
                string _line = sr.ReadLine();
            J00002: _line = sr.ReadLine();

                if (!_line.Contains("}"))
                {
                    string[] split = _line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    particle.timeEventEmittingAngularVelocity.ToDictionary().Add(float.Parse(split[0]), float.Parse(split[1]));
                    goto J00002;
                }
            }
            else if (line.Contains("TimeEventEmittingDirectionX"))
            {
                string _line = sr.ReadLine();
            J00003: _line = sr.ReadLine();

                if (!_line.Contains("}"))
                {
                    string[] split = _line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    particle.timeEventEmittingDirectionX.ToDictionary().Add(float.Parse(split[0]), float.Parse(split[1]));
                    goto J00003;
                }
            }
            else if (line.Contains("TimeEventEmittingDirectionY"))
            {
                string _line = sr.ReadLine();
            J00004: _line = sr.ReadLine();

                if (!_line.Contains("}"))
                {
                    string[] split = _line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    particle.timeEventEmittingDirectionY.ToDictionary().Add(float.Parse(split[0]), float.Parse(split[1]));
                    goto J00004;
                }
            }
            else if (line.Contains("TimeEventEmittingDirectionZ"))
            {
                string _line = sr.ReadLine();
            J00005: _line = sr.ReadLine();

                if (!_line.Contains("}"))
                {
                    string[] split = _line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    particle.timeEventEmittingDirectionZ.ToDictionary().Add(float.Parse(split[0]), float.Parse(split[1]));
                    goto J00005;
                }
            }
            else if (line.Contains("TimeEventEmittingVelocity"))
            {
                string _line = sr.ReadLine();
            J00006: _line = sr.ReadLine();

                if (!_line.Contains("}"))
                {
                    string[] split = _line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    particle.timeEventEmittingVelocity.ToDictionary().Add(float.Parse(split[0]), float.Parse(split[1]));
                    goto J00006;
                }
            }
            else if (line.Contains("TimeEventEmissionCountPerSecond"))
            {
                string _line = sr.ReadLine();
            J00007: _line = sr.ReadLine();

                if (!_line.Contains("}"))
                {
                    string[] split = _line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    particle.timeEventEmissionCountPerSecond.ToDictionary().Add(float.Parse(split[0]), float.Parse(split[1]));
                    goto J00007;
                }
            }
            else if (line.Contains("TimeEventLifeTime"))
            {
                string _line = sr.ReadLine();
            J00008: _line = sr.ReadLine();

                if (!_line.Contains("}"))
                {
                    string[] split = _line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        particle.timeEventLifeTime.ToDictionary().Add(float.Parse(split[0]), float.Parse(split[1]));
                    }
                    catch { }
                    goto J00008;
                }
            }
            else if (line.Contains("TimeEventSizeX"))
            {
                string _line = sr.ReadLine();
            J00016: _line = sr.ReadLine();

                if (!_line.Contains("}"))
                {
                    string[] split = _line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    particle.timeEventSizeX.ToDictionary().Add(float.Parse(split[0]), float.Parse(split[1]));
                    goto J00016;
                }
            }
            else if (line.Contains("TimeEventSizeY"))
            {
                string _line = sr.ReadLine();
            J00017: _line = sr.ReadLine();

                if (!_line.Contains("}"))
                {
                    string[] split = _line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    particle.timeEventSizeY.ToDictionary().Add(float.Parse(split[0]), float.Parse(split[1]));
                    goto J00017;
                }
            }
            #endregion

            #region for ParticleProperty
            else if (line.Contains("BillboardType"))
            {
                string[] split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                particle.billboardType = int.Parse(split[1]);
            }
            else if (line.Contains("RotationType"))
            {
                string[] split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                particle.rotationType = int.Parse(split[1]);
            }
            else if (line.Contains("RotationSpeed"))
            {
                string[] split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                particle.rotationSpeed = float.Parse(split[1]);
            }
            else if (line.Contains("EmittingRadius"))
            {
                string[] split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                particle.emittingRadius = float.Parse(split[1]);
            }
            else if (line.Contains(" EmittingSize "))
            {
                string[] split = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                particle.emittingSize = new Vector3(float.Parse(split[1]), float.Parse(split[3]), float.Parse(split[2]));
            }

            else if (line.Contains("TimeEventScaleX"))
            {
                string _line = sr.ReadLine();
            J00009: _line = sr.ReadLine();

                if (!_line.Contains("}"))
                {
                    string[] split = _line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        particle.timeEventScaleX.ToDictionary().Add(float.Parse(split[0]), float.Parse(split[1]));
                    }
                    catch { }
                    goto J00009;
                }
            }
            else if (line.Contains("TimeEventScaleY"))
            {
                string _line = sr.ReadLine();
            J00010: _line = sr.ReadLine();

                if (!_line.Contains("}"))
                {
                    string[] split = _line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    particle.timeEventScaleY.ToDictionary().Add(float.Parse(split[0]), float.Parse(split[1]));
                    goto J00010;
                }
            }
            else if (line.Contains("TimeEventColorRed"))
            {
                string _line = sr.ReadLine();
            J00011: _line = sr.ReadLine();

                if (!_line.Contains("}"))
                {
                    string[] split = _line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        particle.timeEventColorRed.ToDictionary().Add(float.Parse(split[0]), float.Parse(split[1]));
                    }
                    catch { }
                    goto J00011;
                }
            }
            else if (line.Contains("TimeEventColorGreen"))
            {
                string _line = sr.ReadLine();
            J00012: _line = sr.ReadLine();

                if (!_line.Contains("}"))
                {
                    string[] split = _line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        particle.timeEventColorGreen.ToDictionary().Add(float.Parse(split[0]), float.Parse(split[1]));
                    }
                    catch { }
                    goto J00012;
                }
            }
            else if (line.Contains("TimeEventColorBlue"))
            {
                string _line = sr.ReadLine();
            J00013: _line = sr.ReadLine();

                if (!_line.Contains("}"))
                {
                    string[] split = _line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    try
                    {
                        particle.timeEventColorBlue.ToDictionary().Add(float.Parse(split[0]), float.Parse(split[1]));
                    }
                    catch { }
                    goto J00013;
                }
            }
            else if (line.Contains("TimeEventAlpha"))
            {
                string _line = sr.ReadLine();
            J00014: _line = sr.ReadLine();

                if (!_line.Contains("}"))
                {
                    string[] split = _line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    particle.timeEventAlpha.ToDictionary().Add(float.Parse(split[0]), float.Parse(split[1]));
                    goto J00014;
                }
            }
            else if (line.Contains("TimeEventRotation"))
            {
                string _line = sr.ReadLine();
            J00015: _line = sr.ReadLine();

                if (!_line.Contains("}"))
                {
                    string[] split = _line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    particle.timeEventRotation.ToDictionary().Add(float.Parse(split[0]), float.Parse(split[1]));
                    goto J00015;
                }
            }
            else if (line.Contains("TextureFiles"))
            {
                string _line = sr.ReadLine();
                _line = sr.ReadLine().Replace(".dds", string.Empty).Replace("  ", "").Replace(@"D:\YMIR WORK\pc\sura\effect\", string.Empty).Replace(@"D:\Ymir Work\pc\sura\effect\", string.Empty).
                    Replace(@"D:\Ymir Work\effect\monster2\", string.Empty).Replace(@"D:\Ymir Work\pc\assassin\effect\", string.Empty).Replace(@"D:\Ymir Work\effect\monster\", string.Empty).
                    Replace(@"D:\Ymir work\effect\affect\", string.Empty).Replace(@"D:\Ymir Work\pc\shaman\effect\", string.Empty);
                particle.textureFiles = _line.Remove(0, 1);
                particle.textureFiles = particle.textureFiles.Remove(particle.textureFiles.Length - 1, 1);
            }
            #endregion
        }
        return particle;
    }
}

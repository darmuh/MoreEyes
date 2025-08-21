// Slightly modified version of this scriptable object provided by REPOLib-SDK
// https://github.com/ZehsTeam/REPOLib/blob/main/REPOLib/Objects/Sdk/Mod.cs
using System.Collections.Generic;
using UnityEngine;

namespace MoreEyes.SDK;
/// <summary>
/// A MoreEyes content mod.
/// This should only be used internally by this mod or by editor scripts within unity
/// </summary>
[CreateAssetMenu(menuName = "MoreEyes/Mod", order = 0, fileName = "New MoreEyes Mod")]
public class MoreEyesMod : ScriptableObject
{
    [SerializeField]
    private string _name = null!;

    [SerializeField]
    private string _author = null!;

    [SerializeField]
    private string _version = "1.0.0";

    /// <summary>
    /// The name of this mod.
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// The author of this mod.
    /// </summary>
    public string Author => _author;

    /// <summary>
    /// The version of this mod.
    /// </summary>
    public string Version => _version;

    /// <summary>
    /// Unqique identifier of this mod.<br/>
    /// Format is $"{<see cref="Author"/>}-{<see cref="Name"/>}-{<see cref="Version"/>}".
    /// </summary>
    public string Identifier => $"{Author}-{Name}-{Version}";

    [SerializeField]
    private List<GameObject> _prefabs = new();

    /// <summary>
    /// This mods' specific prefabs for custom iris/pupils
    /// </summary>
    public List<GameObject> Prefabs => _prefabs;


    /// <summary>
    /// Used to set name of mod, public for editor script ONLY
    /// </summary>
    /// <param name="value"></param>
    public void SetName(string value) => _name = value;
    /// <summary>
    /// Used to set author of mod, public for editor script ONLY
    /// </summary>
    /// <param name="value"></param>
    public void SetAuthor(string value) => _author = value;
    /// <summary>
    /// Used to set version of mod, public for editor script ONLY
    /// </summary>
    /// <param name="value"></param>
    public void SetVersion(string value) => _version = value;
    /// <summary>
    /// Used to set prefabs of mod, public for editor script ONLY
    /// </summary>
    /// <param name="value"></param>
    public void SetPrefabs(List<GameObject> value) => _prefabs = value;
}
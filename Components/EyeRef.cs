using MoreEyes.Collections;
using MoreEyes.Core;
using MoreEyes.Managers;
using System.Collections.Generic;
using UnityEngine;

namespace MoreEyes.Components;
internal class EyeRef : MonoBehaviour
{
    internal Transform EyePlayerPos { get; set; }
    internal List<Transform> EyeMenuPos { get; set; } = [];

    //Menu is list because there can be multiple menu visuals
    internal List<GameObject> PupilMenu { get; private set; } = [];
    internal GameObject PupilActual { get; private set; } = null!;

    internal List<GameObject> IrisMenu { get; private set; } = [];
    internal GameObject IrisActual { get; private set; } = null!;

    internal void PlayerSetup(GameObject pupil)
    {
        if (pupil == null)
            Loggers.Error("Null pupil at PlayerSetup!!!");

        SetFirstPupilActual(pupil);
    }

    internal void SetFirstPupilActual(GameObject pupil)
    {
        PupilActual = pupil;
    }

    internal void SetFirstPupilMenu(GameObject pupil)
    {
        PupilMenu.RemoveAll(x => x == null);
        PupilMenu.Add(pupil);
    }

    internal void SetColorPupil(Color color)
    {
        PupilMenu.RemoveAll(x => x == null);
        if (PupilMenu.Count > 0)
        {
            foreach(var pupil in PupilMenu)
            {
                if(pupil != null)
                    pupil.GetComponentInChildren<MeshRenderer>()?.material.SetColor("_EmissionColor", color);
            }
        }

        if(PupilActual != null)
            PupilActual.GetComponentInChildren<MeshRenderer>()?.material.SetColor("_EmissionColor", color);

        FileManager.UpdateWrite = true;
    }

    internal void SetColorIris(Color color)
    {
        IrisMenu.RemoveAll(x => x == null);
        if (IrisMenu.Count > 0)
        {
            foreach (var iris in IrisMenu)
            {
                iris.GetComponentInChildren<MeshRenderer>()?.material.SetColor("_EmissionColor", color);
            }
        }

        if (IrisActual != null)
            IrisActual.GetComponentInChildren<MeshRenderer>()?.material.SetColor("_EmissionColor", color);

        FileManager.UpdateWrite = true;
    }

    internal void RemovePupil()
    {
        for(int i = PupilMenu.Count - 1; i >= 0; i--)
        {
            Destroy(PupilMenu[i]);
        }

        PupilMenu.RemoveAll(x => x == null);

        if (PupilActual != null)
            Destroy(PupilActual);
    }

    internal void CreatePupil(CustomPupilType selection)
    {
        if (selection.Prefab == null)
        {
            Loggers.Warning("Cannot create NULL Pupil!!!");
            return;
        }

        
        foreach(var eye in EyeMenuPos)
        {
            if(eye == null)
                continue;
            var pupil = Instantiate(selection.Prefab, eye);
            PupilMenu.Add(pupil);
            SetTransformAndActive(pupil, selection.isVanilla);
        }

        EyeMenuPos.RemoveAll(x => x == null);

        if (EyePlayerPos != null)
            PupilActual = Instantiate(selection.Prefab, EyePlayerPos);

        
        SetTransformAndActive(PupilActual, selection.isVanilla);
    }

    private void SetTransformAndActive(GameObject gameObject, bool isVanilla)
    {
        if (gameObject == null)
            return;

        Vector3 matchVanilla = new(0, 0, -0.1067f); //matching vanila pupil localPosition if not vanilla

        gameObject.SetActive(true);
        if (!isVanilla)
            gameObject.transform.localPosition = matchVanilla;
    }

    internal void RemoveIris()
    { 
        for (int i = IrisMenu.Count - 1; i >= 0; i--)
        {
            Destroy(IrisMenu[i]);
        }
        IrisMenu.RemoveAll(x => x == null);

        if (IrisActual != null)
            Destroy(IrisActual);
    }

    internal void CreateIris(CustomIrisType selection)
    {
        if (selection.isVanilla)
            return;

        //Non-vanilla custom iris with null prefab should not exist
        if (selection.Prefab == null)
        {
            Loggers.Warning("Cannot create NULL Iris!!!");
            return;
        }

        //creating the iris as child of static objects that will NOT be deleted
        EyeMenuPos.RemoveAll(x => x == null);
        foreach (var eye in EyeMenuPos)
        {
            var iris = Instantiate(selection.Prefab, eye);
            IrisMenu.Add(iris);
            SetTransformAndActive(iris, selection.isVanilla);
        }

        if (EyePlayerPos != null)
            IrisActual = Instantiate(selection.Prefab, EyePlayerPos);

        SetTransformAndActive(IrisActual, selection.isVanilla);
    }
}

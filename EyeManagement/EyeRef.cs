using MoreEyes.Core;
using UnityEngine;

namespace MoreEyes.EyeManagement
{
    internal class EyeRef : MonoBehaviour
    {
        internal Transform eyePlayerPos;
        internal Transform eyeMenuPos;

        internal Vector3 pupilPlayerOrigPos;
        internal Quaternion pupilPlayerOrigRot;
        internal Vector3 pupilMenuOrigPos;
        internal Quaternion pupilMenuOrigRot;

        internal GameObject PupilMenu { get; private set; } = null!;
        internal GameObject PupilActual { get; private set; } = null!;

        internal GameObject IrisMenu { get; private set; } = null!;
        internal GameObject IrisActual { get; private set; } = null!;

        internal void SetFirstPupilActual(GameObject pupil)
        {
            PupilActual = pupil;
            pupilPlayerOrigPos = pupil.transform.position;
            pupilPlayerOrigRot = pupil.transform.rotation;
        }

        internal void SetFirstPupilMenu(GameObject pupil)
        {
            PupilMenu = pupil;
            pupilMenuOrigPos = pupil.transform.position;
            pupilMenuOrigRot = pupil.transform.rotation;
        }

        internal void SetColorPupil(Color color)
        {
            //can't use null coalescing operator as Unity Objects do not return null with this operator when they are destroyed 
            //https://discussions.unity.com/t/c-null-coalescing-operator-does-not-work-for-unityengine-object-types/710219
            if (PupilMenu != null)
                PupilMenu.GetComponentInChildren<MeshRenderer>()?.material.SetColor("_EmissionColor", color);

            if(PupilActual != null)
                PupilActual.GetComponentInChildren<MeshRenderer>()?.material.SetColor("_EmissionColor", color);

            FileManager.UpdateWrite = true;
        }

        internal void SetColorIris(Color color)
        {
            //can't use null coalescing operator as Unity Objects do not return null with this operator when they are destroyed 
            //https://discussions.unity.com/t/c-null-coalescing-operator-does-not-work-for-unityengine-object-types/710219
            if (IrisMenu != null)
                IrisMenu.GetComponentInChildren<MeshRenderer>()?.material.SetColor("_EmissionColor", color);
            if(IrisActual != null)
                IrisActual.GetComponentInChildren<MeshRenderer>()?.material.SetColor("_EmissionColor", color);

            FileManager.UpdateWrite = true;
        }

        internal void RemovePupil()
        {
            if(PupilMenu != null)
                Destroy(PupilMenu);

            if(PupilActual != null)
                Destroy(PupilActual);
        }

        internal void CreatePupil(CustomPupilType selection)
        {
            if (selection.Prefab == null)
            {
                Plugin.WARNING("Cannot create NULL Pupil!!!");
                return;
            }

            if (eyeMenuPos != null)
                PupilMenu = Instantiate(selection.Prefab, eyeMenuPos);  

            if (eyePlayerPos != null)
                PupilActual = Instantiate(selection.Prefab, eyePlayerPos);

            SetTransformAndActive(PupilMenu, selection.isVanilla);
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
            if (IrisMenu != null)
                Destroy(IrisMenu);

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
                Plugin.WARNING("Cannot create NULL Iris!!!");
                return;
            }

            //creating the iris as child of static objects that will NOT be deleted
            if (eyeMenuPos != null)
                IrisMenu = Instantiate(selection.Prefab, eyeMenuPos);

            if (eyePlayerPos != null)
                IrisActual = Instantiate(selection.Prefab, eyePlayerPos);

            //Show new iris!
            SetTransformAndActive(IrisMenu, selection.isVanilla);
            SetTransformAndActive(IrisActual, selection.isVanilla);
        }
    }
}

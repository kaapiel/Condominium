using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;

namespace StarterAssets
{
    public class UICanvasControllerInput : MonoBehaviour
    {

        [Header("Output")]
        public StarterAssetsInputs starterAssetsInputs;

        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            starterAssetsInputs.MoveInput(virtualMoveDirection);
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            starterAssetsInputs.LookInput(virtualLookDirection);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            starterAssetsInputs.JumpInput(virtualJumpState);
        }

        public void VirtualSprintInput(bool virtualSprintState)
        {
            starterAssetsInputs.SprintInput(virtualSprintState);
        }

        public void VirtualExitInput()
        {
            AssetBundle.UnloadAllAssetBundles(true);
            SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        }

        public void VirtualPrintScreenInput()
        {
            ScreenCapture.CaptureScreenshot("PrintScreen-" + System.DateTime.UtcNow.ToString("HH:mm:ss - dd_MMMM_yyyy") + ".png");
        }

        public void VirtualSwitchPOVInput()
        {
            //3rd
            GameObject pov = GameObject.Find("PlayerFollowCamera");
            CinemachineVirtualCamera cinemachineVirtualCamera = pov.GetComponent<CinemachineVirtualCamera>();
            Cinemachine3rdPersonFollow cinemachine3rdPersonFollow = cinemachineVirtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            LensSettings m_Lens = cinemachineVirtualCamera.m_Lens;

            //If 3rd view set 1st view
            if (m_Lens.FieldOfView == 70f)
            {
                m_Lens.FieldOfView = 120;

                cinemachine3rdPersonFollow.ShoulderOffset.x = 1;
                cinemachine3rdPersonFollow.ShoulderOffset.y = 0.3f;
                cinemachine3rdPersonFollow.ShoulderOffset.z = 0;

                cinemachine3rdPersonFollow.CameraDistance = -0.5f;

                cinemachineVirtualCamera.m_Lens = m_Lens;
            }
            else
            {
                m_Lens.FieldOfView = 70;

                cinemachine3rdPersonFollow.ShoulderOffset.x = 1;
                cinemachine3rdPersonFollow.ShoulderOffset.y = 0;
                cinemachine3rdPersonFollow.ShoulderOffset.z = 0;

                cinemachine3rdPersonFollow.CameraDistance = 4f;

                cinemachineVirtualCamera.m_Lens = m_Lens;
            }

        }

        public void VirtualFlashLightInput()
        {
            GameObject mainCamera = GameObject.Find("MainCamera");
            foreach (Transform t in mainCamera.transform)
            {
                if (t.name == "FlashLight")
                {
                    if (t.gameObject.active == false)
                    {
                        t.gameObject.SetActive(true);
                    }
                    else
                    {
                        t.gameObject.SetActive(false);
                    }
                    break;
                }
            }
        }

        public void VirtualCustomInput()
        {
            FindIncludingInactive("ImportCanvas").active = true;
            GameObject.Find("JoystickCanvas").active = false;
        }

        public void VirtualCustomCloseInput()
        {
            GameObject.Find("ImportCanvas").active = false;
            FindIncludingInactive("JoystickCanvas").active = true;

        }

        public static GameObject FindIncludingInactive(string name)
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.isLoaded)
            {
                //no scene loaded
                return null;
            }

            var game_objects = new List<GameObject>();
            scene.GetRootGameObjects(game_objects);

            foreach (GameObject obj in game_objects)
            {
                if (obj.transform.name == name) return obj;

                GameObject found = FindInChildrenIncludingInactive(obj, name);
                if (found) return found;
            }

            return null;
        }

        private static GameObject FindInChildrenIncludingInactive(GameObject go, string name)
        {

            for (int i = 0; i < go.transform.childCount; i++)
            {
                if (go.transform.GetChild(i).gameObject.name == name) return go.transform.GetChild(i).gameObject;
                GameObject found = FindInChildrenIncludingInactive(go.transform.GetChild(i).gameObject, name);
                if (found != null) return found;
            }

            return null;  //couldn't find crap
        }
    }

}

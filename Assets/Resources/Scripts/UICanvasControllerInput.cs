using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;

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

    }

}

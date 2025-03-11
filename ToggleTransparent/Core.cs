using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(ToggleTransparent.Core), "ToggleTransparent", "1.0.0", "wit117", null)]
[assembly: MelonGame("infiniteloop", "DesktopMate")]

namespace ToggleTransparent
{
    public class Core : MelonMod
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_TOPMOST = 0x8;
        private const int WS_EX_LAYERED = 0x80000;

        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_F8 = 0x78;  // key code for F8

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private IntPtr hwnd;
        private bool isInteractable = false;
        private IntPtr hwndHandle;
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("ToggleTransparent Initialized.");
        }

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            // Unity window handle
            hwnd = FindWindow(null, "DesktopMate");
            if (hwnd == IntPtr.Zero)
            {
                MelonLogger.Error("Failed to find the game window!");
                return;
            }

            // Register the F8 hotkey to toggle the interaction state, does not work apparently
            //hwndHandle = Process.GetCurrentProcess().MainWindowHandle;
            //RegisterHotKey(hwndHandle, 1, 0, HOTKEY_F8);

            SetInteractable(true); // Start in interactable mode
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            // Toggle interaction mode
            if (Input.GetKeyDown(KeyCode.F8)) // Change the key if needed
            {
                isInteractable = !isInteractable;
                SetInteractable(isInteractable);
                MelonLogger.Msg($"Window interactable: {isInteractable}");
            }

            // Reapply the transparent state to block input
            if (!isInteractable)
            {
                SetInteractable(false);
            }
        }

        private void SetInteractable(bool interactable)
        {
            int styles = (int)GetWindowLong(hwnd, GWL_EXSTYLE);

            if (interactable)
            {
                // Re-enable normal interaction
                SetWindowLong(hwnd, GWL_EXSTYLE, styles & ~WS_EX_TRANSPARENT);
                EnableUnityInput(true);
            }
            else
            {
                // Apply transparent mode to block input
                SetWindowLong(hwnd, GWL_EXSTYLE, styles | WS_EX_TRANSPARENT);
                EnableUnityInput(false);
            }
        }

        private void EnableUnityInput(bool enable)
        {
            foreach (var canvas in GameObject.FindObjectsOfType<Canvas>())
            {
                var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                if (raycaster != null)
                    raycaster.enabled = enable;
            }

            foreach (var collider in GameObject.FindObjectsOfType<Collider>())
            {
                collider.enabled = enable;
            }
            foreach (var collider2D in GameObject.FindObjectsOfType<Collider2D>())
            {
                collider2D.enabled = enable;
            }

            var eventSystem = GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem != null)
            {
                eventSystem.enabled = enable;
            }
        }

    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace angelTool
{
    public static class AudioSessionManager
    {
        // ============================================================
        // Windows Core Audio
        // ============================================================

        private enum EDataFlow
        {
            eRender = 0,
            eCapture = 1,
            eAll = 2
        }

        private enum ERole
        {
            eConsole = 0,
            eMultimedia = 1,
            eCommunications = 2
        }

        private enum CLSCTX
        {
            CLSCTX_INPROC_SERVER = 0x1,
            CLSCTX_INPROC_HANDLER = 0x2,
            CLSCTX_LOCAL_SERVER = 0x4,
            CLSCTX_ALL = 0x17
        }

        private enum STGM
        {
            STGM_READ = 0x00000000
        }

        // ------------------------------------------------------------
        // COM GUID
        // ------------------------------------------------------------

        private static readonly Guid CLSID_MMDeviceEnumerator =
            new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E");

        private static readonly Guid IID_IMMDeviceEnumerator =
            new Guid("A95664D2-9614-4F35-A746-DE8DB63617E6");

        private static readonly Guid IID_IAudioSessionManager2 =
            new Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");

        private static readonly Guid IID_IAudioSessionControl2 =
            new Guid("BFB7FF88-7239-4FC9-8FA2-07C950BE9C6D");

        private static readonly Guid IID_ISimpleAudioVolume =
            new Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8");

        // ------------------------------------------------------------
        // COM Interfaces
        // ------------------------------------------------------------

        [ComImport]
        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceEnumerator
        {
            int EnumAudioEndpoints(
                EDataFlow dataFlow,
                uint stateMask,
                out IMMDeviceCollection devices);

            int GetDefaultAudioEndpoint(
                EDataFlow dataFlow,
                ERole role,
                out IMMDevice endpoint);

            int GetDevice(
                [MarshalAs(UnmanagedType.LPWStr)] string id,
                out IMMDevice device);

            int RegisterEndpointNotificationCallback(
                IntPtr client);

            int UnregisterEndpointNotificationCallback(
                IntPtr client);
        }

        [ComImport]
        [Guid("0BD7A1BE-7A1A-44DB-8397-C0E7F4A6F3B1")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceCollection
        {
            int GetCount(out uint count);

            int Item(
                uint index,
                out IMMDevice device);
        }

        [ComImport]
        [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDevice
        {
            int Activate(
                ref Guid iid,
                CLSCTX clsCtx,
                IntPtr activationParams,
                [MarshalAs(UnmanagedType.IUnknown)] out object interfacePointer);

            int OpenPropertyStore(
                STGM access,
                out IPropertyStore properties);

            int GetId(
                [MarshalAs(UnmanagedType.LPWStr)] out string id);

            int GetState(out uint state);
        }

        [ComImport]
        [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionManager2
        {
            int GetAudioSessionControl(
                ref Guid audioSessionGuid,
                uint streamFlags,
                out IAudioSessionControl sessionControl);

            int GetSimpleAudioVolume(
                ref Guid audioSessionGuid,
                uint streamFlags,
                out ISimpleAudioVolume audioVolume);

            int GetSessionEnumerator(
                out IAudioSessionEnumerator sessionEnum);

            int RegisterSessionNotification(
                IntPtr sessionNotification);

            int UnregisterSessionNotification(
                IntPtr sessionNotification);

            int RegisterDuckNotification(
                [MarshalAs(UnmanagedType.LPWStr)] string sessionId,
                IntPtr duckNotification);

            int UnregisterDuckNotification(
                [MarshalAs(UnmanagedType.LPWStr)] string sessionId);
        }

        [ComImport]
        [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionEnumerator
        {
            int GetCount(out int count);

            int GetSession(
                int index,
                out IAudioSessionControl session);
        }

        [ComImport]
        [Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionControl
        {
            int GetState(out int state);

            int GetDisplayName(
                [MarshalAs(UnmanagedType.LPWStr)] out string displayName);

            int SetDisplayName(
                [MarshalAs(UnmanagedType.LPWStr)] string displayName,
                ref Guid eventContext);

            int GetIconPath(
                [MarshalAs(UnmanagedType.LPWStr)] out string iconPath);

            int SetIconPath(
                [MarshalAs(UnmanagedType.LPWStr)] string iconPath,
                ref Guid eventContext);

            int GetGroupingParam(
                out Guid groupingId);

            int SetGroupingParam(
                ref Guid groupingId,
                ref Guid eventContext);

            int RegisterAudioSessionNotification(
                IntPtr client);

            int UnregisterAudioSessionNotification(
                IntPtr client);
        }

        [ComImport]
        [Guid("BFB7FF88-7239-4FC9-8FA2-07C950BE9C6D")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioSessionControl2
        {
            int GetState(out int state);

            int GetDisplayName(
                [MarshalAs(UnmanagedType.LPWStr)] out string displayName);

            int SetDisplayName(
                [MarshalAs(UnmanagedType.LPWStr)] string displayName,
                ref Guid eventContext);

            int GetIconPath(
                [MarshalAs(UnmanagedType.LPWStr)] out string iconPath);

            int SetIconPath(
                [MarshalAs(UnmanagedType.LPWStr)] string iconPath,
                ref Guid eventContext);

            int GetGroupingParam(
                out Guid groupingId);

            int SetGroupingParam(
                ref Guid groupingId,
                ref Guid eventContext);

            int RegisterAudioSessionNotification(
                IntPtr client);

            int UnregisterAudioSessionNotification(
                IntPtr client);

            int GetSessionIdentifier(
                [MarshalAs(UnmanagedType.LPWStr)] out string sessionIdentifier);

            int GetSessionInstanceIdentifier(
                [MarshalAs(UnmanagedType.LPWStr)] out string sessionInstanceIdentifier);

            int GetProcessId(out uint processId);

            int IsSystemSoundsSession();

            int SetDuckingPreference(bool optOut);
        }

        [ComImport]
        [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ISimpleAudioVolume
        {
            int SetMasterVolume(
                float level,
                ref Guid eventContext);

            int GetMasterVolume(
                out float level);

            int SetMute(
                bool mute,
                ref Guid eventContext);

            int GetMute(
                out bool mute);
        }

        [ComImport]
        [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPropertyStore
        {
            int GetCount(out uint count);

            int GetAt(
                uint index,
                out PROPERTYKEY key);

            int GetValue(
                ref PROPERTYKEY key,
                out PROPVARIANT value);

            int SetValue(
                ref PROPERTYKEY key,
                ref PROPVARIANT value);

            int Commit();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROPERTYKEY
        {
            public Guid fmtid;
            public uint pid;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROPVARIANT
        {
            public ushort vt;
            public ushort wReserved1;
            public ushort wReserved2;
            public ushort wReserved3;
            public IntPtr p;
            public int p2;
        }

        // ============================================================
        // Public API
        // ============================================================

        /// <summary>
        /// 將指定 PID 的所有 Audio Session 設為 Mute / Unmute。
        /// 不修改原本的音量百分比。
        /// </summary>
        public static bool SetProcessMute(int processId, bool mute)
        {
            bool found = false;

            IMMDeviceEnumerator enumerator = null;
            IMMDevice device = null;
            IAudioSessionManager2 manager = null;
            IAudioSessionEnumerator sessions = null;

            try
            {
                enumerator =
                    (IMMDeviceEnumerator)Activator.CreateInstance(
                        Type.GetTypeFromCLSID(CLSID_MMDeviceEnumerator));

                int hr = enumerator.GetDefaultAudioEndpoint(
                    EDataFlow.eRender,
                    ERole.eMultimedia,
                    out device);

                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);

                Guid iid = IID_IAudioSessionManager2;

                object obj;

                hr = device.Activate(
                    ref iid,
                    CLSCTX.CLSCTX_ALL,
                    IntPtr.Zero,
                    out obj);

                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);

                manager = (IAudioSessionManager2)obj;

                hr = manager.GetSessionEnumerator(out sessions);

                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);

                hr = sessions.GetCount(out int count);

                if (hr < 0)
                    Marshal.ThrowExceptionForHR(hr);

                for (int i = 0; i < count; i++)
                {
                    IAudioSessionControl session = null;

                    try
                    {
                        hr = sessions.GetSession(i, out session);

                        if (hr < 0 || session == null)
                            continue;

                        IAudioSessionControl2 session2 =
                            session as IAudioSessionControl2;

                        if (session2 == null)
                            continue;

                        hr = session2.GetProcessId(out uint pid);

                        if (hr < 0)
                            continue;

                        if (pid != processId)
                            continue;

                        ISimpleAudioVolume volume =
                            session as ISimpleAudioVolume;

                        if (volume == null)
                            continue;

                        Guid context = Guid.NewGuid();

                        volume.SetMute(mute, ref context);

                        found = true;
                    }
                    finally
                    {
                        if (session != null)
                            Marshal.ReleaseComObject(session);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"SetProcessMute({processId}, {mute}) failed: {ex}");
            }
            finally
            {
                if (sessions != null)
                    Marshal.ReleaseComObject(sessions);

                if (manager != null)
                    Marshal.ReleaseComObject(manager);

                if (device != null)
                    Marshal.ReleaseComObject(device);

                if (enumerator != null)
                    Marshal.ReleaseComObject(enumerator);
            }

            return found;
        }

        /// <summary>
        /// 將遊戲 PID 靜音。
        /// </summary>
        public static bool Mute(int processId)
        {
            return SetProcessMute(processId, true);
        }

        /// <summary>
        /// 恢復遊戲原本的音量。
        /// 注意：這裡只是取消 Mute，不會把音量改成 100%。
        /// </summary>
        public static bool Unmute(int processId)
        {
            return SetProcessMute(processId, false);
        }
    }
}

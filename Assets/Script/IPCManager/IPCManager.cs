using System.Collections;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Security.Principal;
using UnityEngine;

public class IPCManager : MonoBehaviour
{
    public static MemoryMappedFile sharedBuffer;
    public static MemoryMappedViewAccessor sharedBufferAccessor;
    public static bool isInitialized = false;

    private void Awake() 
    {
        EnsureInitialization();
    }
    private static void EnsureInitialization()
    {
        if (!isInitialized)
            InitializeIPC("Local\\WACVR_SHARED_BUFFER", 2164);
    }

    private IEnumerator ReconnectWait()
    {
        yield return new WaitForSeconds(5);
        InitializeIPC("Local\\WACVR_SHARED_BUFFER", 2164);
    }

    private void Reconnect()
    {
        InitializeIPC("Local\\WACVR_SHARED_BUFFER", 2164);
    }

    private static void InitializeIPC(string sharedMemoryName, int sharedMemorySize)
    {
        MemoryMappedFileSecurity CustomSecurity = new MemoryMappedFileSecurity();
        SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
        var acct = sid.Translate(typeof(NTAccount)) as NTAccount;
        CustomSecurity.AddAccessRule(new System.Security.AccessControl.AccessRule<MemoryMappedFileRights>(acct.ToString(), MemoryMappedFileRights.FullControl, System.Security.AccessControl.AccessControlType.Allow));
        sharedBuffer = MemoryMappedFile.CreateOrOpen(sharedMemoryName, sharedMemorySize, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.None, CustomSecurity, System.IO.HandleInheritability.Inheritable);
        sharedBufferAccessor = sharedBuffer.CreateViewAccessor();
        isInitialized = true;
    }

    public static byte[] GetLightData()
    {
        EnsureInitialization();
        byte[] bytes = new byte[1920];
        IPCManager.sharedBufferAccessor.ReadArray<byte>(244, bytes, 0, 1920);
        return bytes;
    }

    public static void SetTouchData(bool[] bytes)
    {
        EnsureInitialization();
        IPCManager.sharedBufferAccessor.WriteArray<bool>(4, bytes, 0, 240);
    }

    public static void SetTouch(int index, bool value)
    {
        EnsureInitialization();
        if (value)
            IPCManager.sharedBufferAccessor.Write(4 + index, 1);
        else
            IPCManager.sharedBufferAccessor.Write(4 + index, 0);
    }
}

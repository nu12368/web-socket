using System;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;


public enum ENUM_LIBWFX_ERRCODE
{
    LIBWFX_ERRCODE_SUCCESS = 0,
    LIBWFX_ERRCODE_FAIL,
    LIBWFX_ERRCODE_NO_INIT,
    LIBWFX_ERRCODE_NO_AVI_OCR,
    LIBWFX_ERRCODE_NO_DOC_OCR,
    LIBWFX_ERRCODE_NO_OCR,
    LIBWFX_ERRCODE_NO_DEVICES,
    LIBWFX_ERRCODE_FORMAT_ERROR,
    LIBWFX_ERRCODE_NO_DEVICE_NAME,
    LIBWFX_ERRCODE_NO_SOURCE,
    LIBWFX_ERRCODE_FILE_NO_EXIST,
    LIBWFX_ERRCODE_PAPER_NOT_READY,
    LIBWFX_ERRCODE_INVALID_SERIALNUM,
}

public enum ENUM_LIBWFX_EVENT_CODE
{
    LIBWFX_EVENT_PAPER_DETECTED = 0,
    LIBWFX_EVENT_NO_PAPER,
    LIBWFX_EVENT_PAPER_JAM,
    LIBWFX_EVENT_MULTIFEED,
    LIBWFX_EVENT_NO_CALIBRATION_DATA,
    LIBWFX_EVENT_WARMUP_COUNTDOWN,
    LIBWFX_EVENT_SCAN_PROGRESS,
    LIBWFX_EVENT_BUTTON_DETECTED,
    LIBWFX_EVENT_SCANNING,
    LIBWFX_EVENT_PAPER_FEEDING_ERROR,
    LIBWFX_EVENT_COVER_OPEN,
    LIBWFX_EVENT_LEFT_SENSOR_DETECTED,
    LIBWFX_EVENT_RIGHT_SENSOR_DETECTED,
    LIBWFX_EVENT_ALL_SENSOR_DETECTED,
    LIBWFX_EVENT_UVSECURITY_DETECTED
}

public enum ENUM_LIBWFX_EXCEPTION_CODE
{
    LIBWFX_EXC_OTHER = 0,
    LIBWFX_EXC_TIFF_SAVE_FINSIHED,
    LIBWFX_EXC_PDF_SAVE_FINSIHED,
}

public enum ENUM_LIBWFX_NOTIFY_CODE
{
    LIBWFX_NOTIFY_IMAGE_DONE = 0,
    LIBWFX_NOTIFY_END,
    LIBWFX_NOTIFY_EXCEPTION,
}

public enum ENUM_LIBWFX_EJECT_DIRECTION
{
    LIBWFX_EJECT_FORWARDING = 1,
    LIBWFX_EJECT_BACKWARDING,
}

public enum ENUM_LIBWFX_COLOR_MODE
{
    LIBWFX_COLOR_MODE_BW = 0,
    LIBWFX_COLOR_MODE_GRAY,
    LIBWFX_COLOR_MODE_COLOR,
}

[StructLayout(LayoutKind.Sequential)]
public struct ST_IMAGE_INFO
{
    public ENUM_LIBWFX_COLOR_MODE enColorMode;
    public uint ulPixel;
    public uint ulPerLawByte;
    public uint ulLine;
    public IntPtr pRawDate;
};

[StructLayout(LayoutKind.Sequential)]
class DeviceWrapper
{
    public const String LIBWFX_DLLNAME = @"LibWebFXScan.dll";

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LIBWFXEVENTCB(ENUM_LIBWFX_EVENT_CODE enEventCode, int nParam, IntPtr pUserDef);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LIBWFXCB(ENUM_LIBWFX_NOTIFY_CODE enNotifyCode, IntPtr pUserDef, IntPtr pParam1, IntPtr pParam2);

    [DllImport(LIBWFX_DLLNAME, EntryPoint = "LibWFX_Init", CallingConvention = CallingConvention.StdCall)]
    public static extern ENUM_LIBWFX_ERRCODE LibWFX_Init();

    [DllImport(LIBWFX_DLLNAME, EntryPoint = "LibWFX_DeInit", CallingConvention = CallingConvention.StdCall)]
    public static extern ENUM_LIBWFX_ERRCODE LibWFX_DeInit();

    [DllImport(LIBWFX_DLLNAME, EntryPoint = "LibWFX_GetDeviesList", CallingConvention = CallingConvention.StdCall)]
    public static extern ENUM_LIBWFX_ERRCODE LibWFX_GetDeviesList(out IntPtr szDevicesListOut);

    [DllImport(LIBWFX_DLLNAME, EntryPoint = "LibWFX_GetFileList", CallingConvention = CallingConvention.StdCall)]
    public static extern ENUM_LIBWFX_ERRCODE LibWFX_GetFileList(out IntPtr szFileListOut);

    [DllImport(LIBWFX_DLLNAME, CharSet = CharSet.Unicode, EntryPoint = "LibWFX_RemoveFile", CallingConvention = CallingConvention.StdCall)]
    public static extern ENUM_LIBWFX_ERRCODE LibWFX_RemoveFile(String szFileNameIn);

    [DllImport(LIBWFX_DLLNAME, CharSet = CharSet.Unicode, EntryPoint = "LibWFX_SetProperty", CallingConvention = CallingConvention.StdCall)]
    public static extern ENUM_LIBWFX_ERRCODE LibWFX_SetProperty(String szRequestCmdIn, [MarshalAs(UnmanagedType.FunctionPtr)] LIBWFXEVENTCB pfnLibWFXEVENTCBIn, IntPtr pUserDefIn);

    [DllImport(LIBWFX_DLLNAME, EntryPoint = "LibWFX_StartScan", CallingConvention = CallingConvention.StdCall)]
    public static extern ENUM_LIBWFX_ERRCODE LibWFX_StartScan([MarshalAs(UnmanagedType.FunctionPtr)] LIBWFXCB pfnLibWFXCBIn, IntPtr pUserDefIn);

    [DllImport(LIBWFX_DLLNAME, EntryPoint = "LibWFX_Calibrate", CallingConvention = CallingConvention.StdCall)]
    public static extern ENUM_LIBWFX_ERRCODE LibWFX_Calibrate();

    [DllImport(LIBWFX_DLLNAME, EntryPoint = "LibWFX_ECOControl", CallingConvention = CallingConvention.StdCall)]
    public static extern ENUM_LIBWFX_ERRCODE LibWFX_ECOControl(out uint pulTime, int nSetIn);

    [DllImport(LIBWFX_DLLNAME, EntryPoint = "LibWFX_PaperReady", CallingConvention = CallingConvention.StdCall)]
    public static extern ENUM_LIBWFX_ERRCODE LibWFX_PaperReady();

    [DllImport(LIBWFX_DLLNAME, EntryPoint = "LibWFX_CloseDevice", CallingConvention = CallingConvention.StdCall)]
    public static extern ENUM_LIBWFX_ERRCODE LibWFX_CloseDevice();

    [DllImport(LIBWFX_DLLNAME, EntryPoint = "LibWFX_EjectPaperControl", CallingConvention = CallingConvention.StdCall)]
    public static extern ENUM_LIBWFX_ERRCODE LibWFX_EjectPaperControl(ENUM_LIBWFX_EJECT_DIRECTION enEjectDirectIn);

    [DllImport(LIBWFX_DLLNAME, EntryPoint = "LibWFX_GetPaperStatus", CallingConvention = CallingConvention.StdCall)]
    public static extern ENUM_LIBWFX_ERRCODE LibWFX_GetPaperStatus(out ENUM_LIBWFX_EVENT_CODE penStatusOut);

    [DllImport(LIBWFX_DLLNAME, EntryPoint = "LibWFX_CameraCalibrate", CallingConvention = CallingConvention.StdCall)]   //////////
    public static extern ENUM_LIBWFX_ERRCODE LibWFX_CameraCalibrate();
}


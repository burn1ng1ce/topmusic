using System;

namespace BurningICE.WebBrowserEx
{
	using System;
	using System.Runtime.InteropServices;
	[StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] 												
	public struct OLECMDTEXT
	{
		public uint cmdtextf;
		public uint cwActual;
		public uint cwBuf;
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=100)]public char rgwz;
	}

	[StructLayout(LayoutKind.Sequential)] 
	public struct OLECMD
	{
		public uint cmdID;
		public uint cmdf;
	}

	// Interop definition for IOleCommandTarget. 
	[ComImport, 
	Guid("b722bccb-4e68-101b-a2bc-00aa00404770"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IOleCommandTarget
	{ 
		//IMPORTANT: The order of the methods is critical here. You
		//perform early binding in most cases, so the order of the methods
		//here MUST match the order of their vtable layout (which is determined
		//by their layout in IDL). The interop calls key off the vtable ordering,
		//not the symbolic names. Therefore, if you switched these method declarations
		//and tried to call the Exec method on an IOleCommandTarget interface from your
		//application, it would translate into a call to the QueryStatus method instead.
		void QueryStatus(ref Guid pguidCmdGroup, UInt32 cCmds, 
			[MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] 
			OLECMD[] prgCmds, ref OLECMDTEXT CmdText);
		void Exec(ref Guid pguidCmdGroup, 
			uint nCmdId, uint nCmdExecOpt, ref object pvaIn, ref object pvaOut);
	}


	/// <summary>
	/// OLECommand 的摘要说明。
	/// </summary>
	public class OLECommand
	{
		public OLECommand()
		{
			//
			// TODO: 在此处添加构造函数逻辑
			//
		}
	}
}

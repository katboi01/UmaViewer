using System;
using System.Runtime.InteropServices;

namespace CriWare {

public class CriAudioReadStream
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate uint InternalDelegate(IntPtr cbobj, IntPtr data, uint numSamples);
	private InternalDelegate internalDelegate;

	public delegate uint Delegate(float[][] buffer, uint numSamples);

	public IntPtr callbackFunction { get; private set; }
	public IntPtr callbackPointer { get; private set; }

	public CriAudioReadStream(IntPtr callbackFunction, IntPtr callbackPointer)
	{
		this.callbackFunction = callbackFunction;
		this.callbackPointer = callbackPointer;
	}

	public CriAudioReadStream(Delegate callback, int numChannels, int bufferSize = 256)
	{
		float[][] buffer = new float[numChannels][];
		for (int channel = 0; channel < numChannels; channel++) {
			buffer[channel] = new float[bufferSize];
		}

		this.internalDelegate = (IntPtr cbobj, IntPtr data, uint numSamples) => {
			if (numSamples > bufferSize) {
				numSamples = (uint)bufferSize;
			}
			numSamples = callback(buffer, numSamples);
			for (int channel = 0; channel < numChannels; channel++) {
				Marshal.Copy(buffer[channel], 0, Marshal.ReadIntPtr(data, IntPtr.Size * channel), (int)numSamples);
			}
			return numSamples;
		};
		this.callbackFunction = Marshal.GetFunctionPointerForDelegate(this.internalDelegate);
	}
}

public class CriAudioWriteStream
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate uint InternalDelegate(IntPtr cbobj, IntPtr data, uint numSamples);
	private InternalDelegate internalDelegate;

	public delegate uint Delegate(float[][] buffer, uint numSamples);

	public IntPtr callbackFunction { get; private set; }
	public IntPtr callbackPointer { get; private set; }

	public CriAudioWriteStream(IntPtr callbackFunction, IntPtr callbackPointer)
	{
		this.callbackFunction = callbackFunction;
		this.callbackPointer = callbackPointer;
	}

	public CriAudioWriteStream(Delegate callback, int numChannels, int bufferSize = 256)
	{
		float[][] buffer = new float[numChannels][];
		for (int channel = 0; channel < numChannels; channel++) {
			buffer[channel] = new float[bufferSize];
		}

		this.internalDelegate = (IntPtr cbobj, IntPtr data, uint numSamples) => {
			if (numSamples > bufferSize) {
				numSamples = (uint)bufferSize;
			}
			for (int channel = 0; channel < numChannels; channel++) {
				Marshal.Copy(Marshal.ReadIntPtr(data, IntPtr.Size * channel), buffer[channel], 0, (int)numSamples);
			}
			numSamples = callback(buffer, numSamples);
			return numSamples;
		};
		this.callbackFunction = Marshal.GetFunctionPointerForDelegate(this.internalDelegate);
	}
}

} //namespace CriWare
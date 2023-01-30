// ReSharper disable once CheckNamespace

using NProbe.JitDebugger;

public class StartupHook
{
	public static void Initialize() {
		var debugger = new JitDebugger();
		debugger.Init();
	}
}

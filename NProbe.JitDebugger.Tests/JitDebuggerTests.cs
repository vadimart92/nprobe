namespace NProbe.JitDebugger.Tests;

public class JitDebuggerTests
{
	[Test]
	public void Init() {
		using var debugger = new JitDebugger();
		Environment.SetEnvironmentVariable("NPROBE_SMARTDEBUGGER_EXCEPTIONBREAKPOINT_1", """
			type:MyNamespace.MyException;StackTrace:Test
		""");
		debugger.Init();
		new MyClass().SomeMethod();
	}
}

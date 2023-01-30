using NSubstitute;

namespace NProbe.JitDebugger.Tests;

public class JitDebuggerTests
{
	[Test]
	public void Init() {
		var onBreakpoint = Substitute.For<OnBreakpointHit>();
		using var debugger = new JitDebugger(onBreakpoint);
		Environment.SetEnvironmentVariable("NPROBE_JITDEBUGGER_EXCEPTIONBREAKPOINT_1", """
			type:MyNamespace.MyException;StackTrace:Test(.*)
		""");
		debugger.Init();
		new MyClass().SomeMethodWithCaughtError();
		onBreakpoint.Received(1).Invoke();
	}
}

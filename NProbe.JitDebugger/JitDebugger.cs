using System.Collections;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;


namespace NProbe.JitDebugger;

public delegate void OnBreakpointHit();

public class JitDebugger : IDisposable
{
	private readonly OnBreakpointHit _breakpointHit;

	public JitDebugger(OnBreakpointHit? breakpointHit = null) {
		_breakpointHit = breakpointHit ?? HitBreakpoint;
		_handler = (_, args) =>  OnException(args.Exception);
	}

	public void Init() {
		SetExceptionBreakpoints();
		AppDomain.CurrentDomain.FirstChanceException += _handler;
	}

	private readonly EventHandler<FirstChanceExceptionEventArgs> _handler;

	class ExceptionFilter
	{
		public Regex? Type { get; set; }
		public Regex? Message { get; set; }
		public Regex? StackTrace { get; set; }
		public bool IsMatch(Exception exception) {
			if (!Type?.IsMatch(exception.GetType().FullName) ?? false) {
				return false;
			}
			if (!Message?.IsMatch(exception.Message) ?? false) {
				return false;
			}
			if (!StackTrace?.IsMatch(exception.StackTrace) ?? false) {
				return false;
			}
			return true;
		}
	}

	private List<ExceptionFilter> Filters { get; } = new();
	void SetExceptionBreakpoints() {
		var envVariables = Environment.GetEnvironmentVariables();
		foreach (DictionaryEntry item in envVariables) {
			var key = item.Key.ToString().Trim();
			if (!key.StartsWith("NPROBE")) {
				continue;
			}
			if (Regex.IsMatch(key, "NPROBE_JITDEBUGGER_EXCEPTIONBREAKPOINT_(\\d+)")) {
				var value = item.Value.ToString().Trim()
					.Split(';')
					.Select(x => x.Split(':'))
					.Where(a=>a.Length == 2)
					.ToDictionary(a => a[0].ToLowerInvariant(), a => a[1]);
				var exceptionFilter = new ExceptionFilter {
					Type = value?.TryGetValue("type", out var p) == true ? new Regex(p) : null,
					Message = value?.TryGetValue("message", out var m) == true ? new Regex(m, RegexOptions.Singleline) : null,
					StackTrace = value?.TryGetValue("stacktrace", out var s) == true ? new Regex(s, RegexOptions.Singleline) : null,
				};
				Filters.Add(exceptionFilter);
			}
		}
		
	}

	void OnException(Exception e) {
		if (!Filters.Any(filter => filter.IsMatch(e))) return;
		_breakpointHit();
	}

	private static void HitBreakpoint() {
		Console.WriteLine(
			"Exception breakpoint hit. Will attach now system JIT debugger to process " +
			$"{Process.GetCurrentProcess().Id}");
		if (Debugger.IsAttached) {
			Debugger.Break();
		} else {
			Debugger.Launch();
		}
	}

	public void Dispose() {
		AppDomain.CurrentDomain.FirstChanceException -= _handler;
	}
}

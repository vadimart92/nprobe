// See https://aka.ms/new-console-template for more information

using MyNamespace;

new MyClass().SomeMethodWithCaughtError();
Console.WriteLine("Hello, World!");

public class MyClass
{
	public void SomeMethodWithCaughtError() {
		try {
			AnotherMethod();
		} catch (Exception e) {
			Console.WriteLine(e);
		}
	}

	public string AnotherMethod() {
		throw new MyException("Test");
	}
}

namespace MyNamespace
{
	public class MyException : Exception
	{
		public MyException(string msg) : base(msg){
			
		}
	}
}

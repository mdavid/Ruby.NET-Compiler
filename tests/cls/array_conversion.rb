require 'mscorlib'
require 'System'

l = System::Collections::Generic::List[System::String].new
l.Add <<"EOF";
public class Test
{
  public static int Sum(int[] a)
  {
    int n = 0;
    foreach (int i in a)
      n += i;
    return n;
  }
  
  public static object First(object[] a)
  {
    return a[0];
  }
}
EOF

compiler = Microsoft::CSharp::CSharpCodeProvider.new
cp = System::CodeDom::Compiler::CompilerParameters.new
cp.TempFiles = System::CodeDom::Compiler::TempFileCollection.new(".")
cp.OutputAssembly = "array_conversion.dll"
require compiler.CompileAssemblyFromSource(cp,l.ToArray).PathToAssembly.to_s

puts (Test.Sum([1,2,3,4]) == 10)
puts (Test.First([1,2,3]) == 1)
puts (Test.First(["test",1,1]) == "test")

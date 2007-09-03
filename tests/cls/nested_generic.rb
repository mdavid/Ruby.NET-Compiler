require 'mscorlib'
require 'System'

l = System::Collections::Generic::List[System::String].new
l.Add <<"EOF";
public class Test<T>
{
  public class Inner{}
  public class Inner<V> {}
  public class Inner<V,V2> {}
}

public class Test2
{
  public class Inner {}
  public class Inner<V> {}
  public class Inner<V,V2> {}
}
EOF

compiler = Microsoft::CSharp::CSharpCodeProvider.new
cp = System::CodeDom::Compiler::CompilerParameters.new
cp.TempFiles = System::CodeDom::Compiler::TempFileCollection.new(".")
cp.OutputAssembly = "nested_generic.dll"
require compiler.CompileAssemblyFromSource(cp,l.ToArray).PathToAssembly.to_s

puts Test[System::Int32].Inner
puts Test[System::Int32].Inner[System::Int32]
puts Test[System::Int32].Inner[System::Int32, System::Int32]

puts Test2.Inner
puts Test2.Inner[System::Int32]
puts Test2.Inner[System::Int32, System::Int32]

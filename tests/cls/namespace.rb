require 'mscorlib'
require 'System'

l = System::Collections::Generic::List[System::String].new
l.Add <<"EOF";
namespace test_lower_namespace
{
  public class test_lower_class
  {
    public static int test_lower_method()
    {
      return 1;
    }
  }
}

namespace Test_upper_namespace
{
  public class Test_upper_class
  {
    public static int Test_upper_method()
    {
      return 2;
    }
  }
}
EOF

compiler = Microsoft::CSharp::CSharpCodeProvider.new
cp = System::CodeDom::Compiler::CompilerParameters.new
cp.TempFiles = System::CodeDom::Compiler::TempFileCollection.new(".")
cp.OutputAssembly = "namespace.cs.dll"
require compiler.CompileAssemblyFromSource(cp,l.ToArray).PathToAssembly.to_s


require 'test/unit'

class TestNamespace < Test::Unit::TestCase
  def test_lower
    assert_equal Test_lower_namespace::Test_lower_class.test_lower_method, 1
  end

  def test_upper
    assert_equal Test_upper_namespace::Test_upper_class.Test_upper_method, 2
  end
end

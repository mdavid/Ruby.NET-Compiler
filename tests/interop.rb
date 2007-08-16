$KCODE = "none"
$testnum=0
$ntest=0
$failed = 0

def test_check(what)
  printf "%s\n", what
  $what = what
  $testnum = 0
end

def test_ok(cond,n=1)
  $testnum+=1
  $ntest+=1
  if cond
    printf "ok %d\n", $testnum
  else
    where = caller(n)[0]
    printf "not ok %s %d -- %s\n", $what, $testnum, where
    $failed+=1 
  end
end

require "mscorlib"
require "system"
mscorlib = System::Reflection::Assembly::Load("mscorlib")

def clr_string(s)
  System::String.Concat(s, "")
end

##########################################################################
test_check "constructors"
test_ok(System::Object.new != nil)

##########################################################################
test_check "methods"
# static method
# valuetype arguments
test_ok(System::Math.Min(1, 2) == 1)
# object arguments
test_ok(System::Object.ReferenceEquals(
  System::Object.new,
  System::Object.new) == false)
# null arguments
test_ok(System::Object.ReferenceEquals(nil, nil))
  
# instance method
test_ok(System::String.Concat("a", "b").
  Equals(System::String.Concat("a", "b")))

##########################################################################
test_check "properties"

# static property
now = System::DateTime.Now
test_ok(now.GetType.Equals(mscorlib.GetType("System.DateTime")))

# instance property
list = System::Collections::ArrayList.new
test_ok(list.Count == 0)

# "indexer" setter/getter
list.Add(0)
list[0] = 123
test_ok(list[0] == 123)

# overloaded "indexer"
match = System::Text::RegularExpressions::Regex.
  Match("abc", "(?<group>.)..")

test_ok(match.Groups[0].Value.Length == 3)
test_ok(match.Groups["group"].Value.Length == 1)

##########################################################################
test_check "fields"

# static field
test_ok(System::Reflection::Emit::OpCodes.Nop.Name.
  Equals(clr_string("nop")))

# instance field
# what mscorlib type has a public instance field??

##########################################################################
test_check "delegates"

# invoke
handler = System::EventHandler.new { |sender,e| sender.upcase! }
input = "abc"
handler.Invoke(input, System::EventArgs.Empty)
test_ok(input == "ABC")

# return value
test_ok(
  System::Text::RegularExpressions::Regex.Replace("abc", ".",
    System::Text::RegularExpressions::MatchEvaluator.new do
      |match| match.Value.ToUpper
    end
  ).Equals(clr_string("ABC")))

# null, value type arguments
(System::Threading::WaitOrTimerCallback.new do
  |state,timedout| test_ok(timedout)
end).Invoke(nil, true)

##########################################################################
if $failed > 0
  printf "test: %d failed %d\n", $ntest, $failed
else
  printf "end of test(test: %d)\n", $ntest
end
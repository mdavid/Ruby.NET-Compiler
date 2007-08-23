require 'test/unit'

# TODO: exit! does not raise SystemExit, so the only way to test is by calling
# another ruby process.

#~ Not yet tested:
#~ => ["`", "at_exit", "binding",
#~ "callcc", "catch", "chomp", "chomp!", "chop", "chop!",
#~ "eval", "exec", "exit!", "fork", "format", "getc", "gets",
#~ "gsub", "gsub!", "lambda", "load",
#~ "method_missing", "open", "print", "printf", "proc", "putc", "put
#~ s", "readline", "readlines", "require", "scan", "select", "set_
#~ trace_func", "sleep", "split", "sprintf", "sub", "sub!", "syscall", "sy
#~ stem", "test", "throw", "trace_var", "trap", "untrace_var", "warn"]

class TestKernel < Test::Unit::TestCase
  def test_self_Array
    # return arg if arg.kind_of?(Array)
    a = ArrayInvalidToAInvalidToAry.new
    aa = Array(a)
    assert_instance_of(ArrayInvalidToAInvalidToAry, aa)
    assert_same(a, aa)

    # return arg.to_ary if arg.respond_to(:to_ary)
    assert_equal([6], Array(InvalidToAValidToAry.new))
    assert_raise(TypeError) { Array(ValidToAInvalidToAry.new) }

    # return arg.to_a if arg.respond_to(:to_a)
    # # Note: the to_a inherited from Kernel is excluded in the search.
    assert_equal([5], Array(ValidToANoToAry.new))
    assert_raise(TypeError) { Array(InvalidToANoToAry.new) }

    # return [] if arg.nil?
    assert_equal([], Array(nil))

    # return [arg]
    o = Object
    assert_equal([o], Array(o))
  end

  def test_self_Float
    f = Float(5)
    assert_equal(5.0, f)
    assert_instance_of(Float, f)

    f = Float(5.0)
    assert_equal(5.0, f)

    b = 10 ** 30
    f = Float(b)
    assert_equal(b, f)
    assert_instance_of(Float, f)

    f = Float('5')
    assert_equal(5.0, f)
    assert_instance_of(Float, f)

    assert_raise(ArgumentError) { Float('.') }

    # "As of Ruby 1.8, converting nil generates a TypeError."
    assert_raise(TypeError) { Float(nil) }

    assert_raise(TypeError) { Float(Empty.new) }
    assert_raise(TypeError) { Float(NumericEmpty.new) }
    assert_raise(TypeError) { Float(NumericStringToF.new) }
    assert_raise(TypeError) { Float(NumericIntegerToF.new) }

    f = Float(FloatToF.new)
    assert_equal(5.0, f)
    assert_instance_of(Float, f)

    assert_raise(ArgumentError) { Float(NaNToF.new) }
  end

  def test_self_Integer
    # TODO: big float

    i = Integer(5)
    assert_equal(5, i)
    assert_instance_of(Fixnum, i)

    # TODO: big int

    i = Integer('5')
    assert_equal(5, i)
    assert_instance_of(Fixnum, i)

    # TODO: big string

    assert_raise(ArgumentError) { Integer('') }
    assert_raise(ArgumentError) { Integer('-') }
    assert_raise(ArgumentError) { Integer('5.0') }

    i = Integer(5.8)
    assert_equal(5, i)
    assert_instance_of(Fixnum, i)

    assert_raise(TypeError) { Integer(Empty.new) }
    assert_raise(NoMethodError) { Integer(NumericEmpty.new) }
    assert_raise(TypeError) { Integer(StringToI.new) }
    assert_raise(TypeError) { Integer(FloatToI.new) }

    i = Integer(IntegerToI.new)
    assert_equal(5, i)
    assert_instance_of(Fixnum, i)
  end

  def test_self_String
    s = 'Hello'
    assert_same(s, String(s))

    o = Empty.new
    assert_equal(o.inspect, String(o))

    assert_raise(TypeError) { String(IntegerToS.new) }

    assert_equal('Hello', String(StringToS.new))
  end

  def test_self_abort
    # TODO: Returns exit status of 1 to OS.
    # TODO: May have a string argument to be printed to STDERR.
    # TODO: Check that at_exit functions are run.
    assert_raise(SystemExit) { abort }
  end

  def test_self_block_given_eh
    # Almost identical to iterator? test, but can't be merged.
    def util_block_given_eh(*args)
      block_given?
    end
    assert_equal(true, util_block_given_eh {})
    assert_equal(false, util_block_given_eh(5))
  end

  def test_self_caller
    re = /^.+:\d+(:in `.+')?$/
    caller.each {|call| assert_match(re, call) }
  end

  def test_self_exit
    # TODO: Check exit status?
    # TODO: Check that at_exit functions are run.
    assert_raise(SystemExit) { exit }
    assert_raise(SystemExit) { exit(-1) }
    assert_raise(SystemExit) { exit(0) }
    assert_raise(SystemExit) { exit(1) }
  end

  def test_self_fail
    util_raise(:fail)
  end

  def test_self_global_variables
    # TODO: What else can be checked?
    assert_nil(global_variables.uniq!)
    re = /^\$/
    global_variables.each {|var| assert_match(re, var) }
  end

  def test_self_iterator_eh
    # Almost identical to block_given? test, but can't be merged.
    def util_iterator_eh(*args)
      iterator?
    end
    assert_equal(true, util_iterator_eh {})
    assert_equal(false, util_iterator_eh(5))
  end

  def test_self_local_variables
    assert_nil(local_variables.uniq!)
    assert_equal(['a', 'b'], local_variables.sort)
    a = 5
    b = a
    a = 6
  end

  def test_self_loop
    n = 0
    loop {
      n += 1
      break if n == 3
    }
    assert_equal(3, n)
  end

  def test_self_p
    lines = util_temp_file { p 1, 2 }
    assert_equal(["1\n", "2\n"], lines)
  end

  def test_self_raise
    util_raise(:raise)
  end

  def test_self_rand
    # NOTE: Cannot reliably test that result is never equal to upper bound.

    util_repeat_in_range(1000, 0.0, 1.0) { rand }
    util_repeat_in_range(1000, 0.0, 1.0) { rand(0) }
    util_repeat_in_range(1000, 0.0, 1.0) { rand(nil) }

    util_repeat_in_range(1000, 0.0, 3.0) { rand(3) }
    util_repeat_in_range(1000, 0.0, 3.0) { rand(3.9) }
    util_repeat_in_range(1000, 0.0, 3.0) { rand('3') }
    util_repeat_in_range(1000, 0.0, 3.0) { rand(-3) }
    util_repeat_in_range(1000, 0.0, 3.0) { rand(-3.9) }
    util_repeat_in_range(1000, 0.0, 3.0) { rand('-3') }

    assert_raise(TypeError) { rand(1..2) }
  end

  def test_self_srand
    srand; rand1 = util_repeat_and_record(1000) { rand }
    srand; rand2 = util_repeat_and_record(1000) { rand }
    assert(rand1 != rand2, "either your seed function is very bad " + \
        "or you're very, very unlucky")

    # Try very small time difference. Seed function should/must(?) be based on
    # "a combination of the time, the process id, and a sequence number",
    # so subsequent random numbers should not be the same.
    srand; rand1 = util_repeat_and_record(10) { rand }
    srand; rand2 = util_repeat_and_record(10) { rand }
    assert(rand1 != rand2, "either your seed function is bad " + \
        "or you're very unlucky")

    # NOTE: MRI bug or documentation bug? The doc says that srand(0) is equivalent to
    #       srand(). This code tests the MRI behaviour.
    srand(0); rand1 = util_repeat_and_record(1000) { rand }
    srand(0); rand2 = util_repeat_and_record(1000) { rand }
    assert(rand1 == rand2)

    srand(123); rand1 = util_repeat_and_record(1000) { rand }
    srand(123); rand2 = util_repeat_and_record(1000) { rand }
    assert(rand1 == rand2)

    srand(123); rand1 = util_repeat_and_record(1000) { rand }
    srand(123.9); rand2 = util_repeat_and_record(1000) { rand }
    assert(rand1 == rand2)

    srand(-123); rand1 = util_repeat_and_record(1000) { rand }
    srand(-123.9); rand2 = util_repeat_and_record(1000) { rand }
    assert(rand1 == rand2)

    assert_raise(TypeError) { srand(nil) }
    assert_raise(TypeError) { srand('123') }
  end

  # Utilities

  class ArrayInvalidToAInvalidToAry < Array
    def initialize
      self << 1
    end
    def to_a
      5
    end
    def to_ary
      6
    end
  end

  class InvalidToAValidToAry
    def to_a
      5
    end
    def to_ary
      [6]
    end
  end

  class ValidToAInvalidToAry
    def to_a
      [5]
    end
    def to_ary
      6
    end
  end

  class ValidToANoToAry
    def to_a
      [5]
    end
  end

  class InvalidToANoToAry
    def to_a
      5
    end
  end

  class Empty
  end

  class NumericEmpty < Numeric
    # Kernel::Float throws TypeError (instead of NoMethodError)
    # when given a Numeric class without to_f.
  end

  class NumericStringToF < Numeric
    def to_f
      '5.0'
    end
  end

  class NumericIntegerToF < Numeric
    def to_f
      5
    end
  end

  class FloatToF
    def to_f
      5.0
    end
  end

  class NaNToF
    def to_f
      0.0 / 0.0
    end
  end

  class StringToI
    def to_i
      '5'
    end
  end

  class FloatToI
    def to_i
      5.0
    end
  end

  class IntegerToI
    def to_i
      5
    end
  end

  class IntegerToS
    def to_s
      5
    end
  end

  class StringToS
    def to_s
      'Hello'
    end
  end

  class MyException < Exception
  end

  class MyExceptionFactory
    def self.exception
      MyException.new
    end
    def exception
      MyException.new
    end
  end

  class StringFactory
    def self.exception
      'error'
    end
    def exception
      'error'
    end
  end

  class StringFactoryException < Exception
    def self.exception
      'error'
    end
    def exception
      'error'
    end
  end

  def util_raise(meth)
    # TODO: Check that raise/fail modifies $!.
    # TODO: Check the messages.

    # raise $! || RuntimeError if args == []
    $! = ZeroDivisionError.new
    assert_raise(ZeroDivisionError) { Kernel.send(meth) }
    $! = nil
    assert_raise(RuntimeError) { Kernel.send(meth) }

    # raise RuntimeError.new(args[0]) if args[0].kind_of?(String)
    $! = ZeroDivisionError.new
    assert_raise(RuntimeError) { Kernel.send(meth, 'error') }

    # assert(args[0].exception.kind_of?(Exception))
    assert_raise(MyException) { Kernel.send(meth, MyExceptionFactory) }
    assert_raise(MyException) { Kernel.send(meth, MyExceptionFactory.new) }
    assert_raise(TypeError) { Kernel.send(meth, nil) }
    assert_raise(TypeError) { Kernel.send(meth, StringFactory) }
    assert_raise(TypeError) { Kernel.send(meth, StringFactory.new) }
    assert_raise(TypeError) { Kernel.send(meth, StringFactoryException) }
    assert_raise(TypeError) { Kernel.send(meth, StringFactoryException.new) }
  end

  def util_repeat_and_record(times)
    res = []
    times.times do
      res << yield
    end
    res
  end

  def util_repeat_in_range(times, from, to)
    # Tests that from <= yield < to.
    times.times do
      res = yield
      assert_operator(from, :<=, res)
      assert_operator(to, :>, res)
    end
  end

  def util_temp_file
    # Simple replacement for StringIO (which is not builtin).

    tmp = '~~tmp'

    stdout = File.new(tmp, 'w')
    old_stdout = $stdout
    $stdout = stdout
    yield
    $stdout = old_stdout
    stdout.close

    fIn = File.new(tmp)
    lines = fIn.readlines
    fIn.close

    File.delete(tmp)

    return lines
  end

end

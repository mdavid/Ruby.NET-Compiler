require 'test/unit'

class TestProc < Test::Unit::TestCase

  def setup
    @proc = Proc.new { 5 }
  end

  def test_aref
    util_invoke(Proc.method(:new), :[])
    util_invoke_non_lambda(Proc.method(:new), :[])
    util_invoke(Kernel.method(:lambda), :[])
    util_invoke_lambda(Kernel.method(:lambda), :[])
    util_invoke(Kernel.method(:proc), :[])
    util_invoke_lambda(Kernel.method(:proc), :[])
  end

  def test_arity
    assert_equal(-1, Proc.new {}.arity)
    assert_equal(0, Proc.new {||}.arity)
    assert_equal(1, Proc.new {|a|}.arity)
    assert_equal(2, Proc.new {|a, b|}.arity)
    assert_equal(-1, Proc.new {|*a|}.arity)
    assert_equal(-2, Proc.new {|a, *b|}.arity)
  end

  def test_binding
    # Follows the test in the doc.
    # Requires Kernel::eval.

    def fred(param)
      Proc.new {}
    end

    b = fred(99)
    assert_equal(99, eval("param", b.binding))
  end

  def test_call
    util_invoke(Proc.method(:new), :call)
    util_invoke_non_lambda(Proc.method(:new), :call)
    util_invoke(Kernel.method(:lambda), :call)
    util_invoke_lambda(Kernel.method(:lambda), :call)
    util_invoke(Kernel.method(:proc), :call)
    util_invoke_lambda(Kernel.method(:proc), :call)
  end

  def test_clone
    a = [1, 2, 3]
    proc = MyProc.new(a) { 5 }

    # When cloned, procs are equal but not the same.
    clone = proc.clone
    assert_equal(proc, clone)
    assert_not_same(proc, clone)

    # Their attributes are copied as well and share instances.
    assert_same(a, clone.a)
  end

  def test_dup
    a = [1, 2, 3]
    proc = MyProc.new(a) { 5 }

    # When duplicated, procs are equal but not the same.
    dup = proc.dup
    assert_equal(proc, dup)
    assert_not_same(proc, dup)

    # Their attributes are NOT copied.
    assert_nil(dup.a)
  end

  def test_equals2
    assert_equal(Proc.new {}, Proc.new {})
    assert_equal(Proc.new {||}, Proc.new {||})

    assert_not_equal(Proc.new {}, Proc.new { 5 })
    assert_not_equal(Proc.new {}, Proc.new {||})
    assert_not_equal(Proc.new {}, Proc.new {|*a|})
    assert_not_equal(Proc.new { 5 }, Proc.new { 5 })
    assert_not_equal(Proc.new {||}, Proc.new {|a|})
    assert_not_equal(Proc.new {||}, Proc.new {|*a|})
    assert_not_equal(Proc.new {|a|}, Proc.new {|a|})
    assert_not_equal(Proc.new {|a|}, Proc.new {|a| 5 })
    assert_not_equal(Proc.new {|a| 5 }, Proc.new {|a| 5 })
    assert_not_equal(Proc.new {|*a|}, Proc.new {|*a|})

    def create_proc
      Proc.new {}
    end
    def create_proc0
      Proc.new {||}
    end
    assert_not_equal(create_proc, create_proc)
    assert_not_equal(create_proc0, create_proc0)
  end

  def test_initialize
    # Arity == 0.
    assert_raise(ArgumentError) { Proc.new(1) {} }
    assert_raise(ArgumentError) { Proc.new(1, 2) {} }

    def util_no_block
      Proc.new
    end

    # Must have block ...
    assert_raise(ArgumentError) { Proc.new }
    # ... unless it's in a method called with a block.
    assert_nothing_raised { util_no_block {} }
  end

  def test_self_new
    # Subclass may have different arity.
    assert_raise(ArgumentError) { MyProc.new {} }
    assert_nothing_raised { MyProc.new(1) {} }
  end

  def test_self_lambda
    # Further tests are in test_aref and test_call.

    # Proc::lambda is private, we have to use Kernel::lambda.
    def util_kernel_lambda_no_block
      Kernel.lambda
    end

    # Should only warn, not raise exception.
    old_w = $-w
    $-w = nil # Suppress warning.
    assert_nothing_raised { util_kernel_lambda_no_block {} }
    $-w = old_w
  end

  def test_self_proc
    # Further tests are in test_aref and test_call.

    # Proc::proc is private, we have to use Kernel::proc.
    def util_kernel_proc_no_block
      Kernel.proc
    end

    # Should only warn, not raise exception.
    old_w = $-w
    $-w = nil # Suppress warning.
    assert_nothing_raised { util_kernel_proc_no_block {} }
    $-w = old_w
  end

  def test_to_proc
    # Return self.
    assert_same(@proc, @proc.to_proc)
    my_proc = MyProc.new(2) { 5 }
    my_to_proc = my_proc.to_proc
    assert_same(my_proc, my_to_proc)
  end

  def test_to_s
    # Requires Regexp.
    # As specified in the doc, this must contain the ID *and* where
    # the proc was created.
    assert_match(/^#<Proc:.+@.+:\d+>$/, @proc.to_s)
  end

  # Utilities

  class MyProc < Proc
    attr_reader :a
    def initialize(a)
      @a = a
    end
  end

  def util_invoke(creator, meth)
    assert_equal(5, Proc.new { 5 }.send(meth))

    # Proc#[] and Proc#call ignore the arity.
    # TODO: Maybe test whether the values are assigned correctly.

    proc = creator.call {}
    assert_nothing_raised { proc.send(meth) }
    assert_nothing_raised { proc.send(meth, 1) }

    proc1 = creator.call {|a|}
    assert_nothing_raised { proc1.send(meth, 1) }
    # For some reason, when using lambda, these also only generate warnings.
    old_w = $-w
    $-w = nil # Suppress warning.
    assert_nothing_raised { proc1.send(meth) }
    assert_nothing_raised { proc1.send(meth, 1, 2) }
    $-w = old_w

    proc_1 = creator.call {|*a|}
    assert_nothing_raised { proc_1.send(meth) }
    assert_nothing_raised { proc_1.send(meth, 1) }
    assert_nothing_raised { proc_1.send(meth, 1, 2) }
  end

  def util_invoke_lambda(creator, meth)
    # Extension to util_invoke for creator(s) that use lambda
    # (Kernel::lambda and Kernel::proc).
    # These creators enforce the proc arity.

    proc0 = creator.call {||}
    assert_nothing_raised { proc0.send(meth) }
    assert_raise(ArgumentError) { proc0.send(meth, 1) }
    assert_raise(ArgumentError) { proc0.send(meth, 1, 2) }

    proc2 = creator.call {|a, b|}
    assert_raise(ArgumentError) { proc2.send(meth) }
    assert_raise(ArgumentError) { proc2.send(meth, 1) }
    assert_nothing_raised { proc2.send(meth, 1, 2) }
    assert_raise(ArgumentError) { proc2.send(meth, 1, 2, 3) }

    proc_2 = creator.call {|a, *b|}
    assert_raise(ArgumentError) { proc_2.send(meth) }
    assert_nothing_raised { proc_2.send(meth, 1) }
    assert_nothing_raised { proc_2.send(meth, 1, 2) }
    assert_nothing_raised { proc_2.send(meth, 1, 2, 3) }
  end

  def util_invoke_non_lambda(creator, meth)
    # Extension to util_invoke for creator(s) that don't use lambda
    # (Proc::new).
    # These creators do not enforce the proc arity.

    proc0 = creator.call {||}
    assert_nothing_raised { proc0.send(meth) }
    assert_nothing_raised { proc0.send(meth, 1) }
    assert_nothing_raised { proc0.send(meth, 1, 2) }

    proc2 = creator.call {|a, b|}
    assert_nothing_raised { proc2.send(meth) }
    assert_nothing_raised { proc2.send(meth, 1) }
    assert_nothing_raised { proc2.send(meth, 1, 2) }
    assert_nothing_raised { proc2.send(meth, 1, 2, 3) }

    proc_2 = creator.call {|a, *b|}
    assert_nothing_raised { proc_2.send(meth) }
    assert_nothing_raised { proc_2.send(meth, 1) }
    assert_nothing_raised { proc_2.send(meth, 1, 2) }
    assert_nothing_raised { proc_2.send(meth, 1, 2, 3) }
  end

end
